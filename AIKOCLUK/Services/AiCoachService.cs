using AIKOCLUK.Core.Constants;
using AIKOCLUK.Data;
using AIKOCLUK.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text.Json;

namespace AIKOCLUK.Services
{
    public class AiCoachService : IAiCoachService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiCoachService> _logger;
        private readonly AppDbContext _context;
        private readonly string? _apiKey;

        private const string GeminiEndpoint =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        public AiCoachService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AiCoachService> logger,
            AppDbContext context)
        {
            _httpClient = httpClient;
            _logger = logger;
            _context = context;
            // DİKKAT: Constructor içinde hata fırlatmıyoruz, aksi takdirde migration sırasında uygulama çöker.
            _apiKey = configuration["Gemini:ApiKey"];
        }

        public async Task<AiExamAnalysisResult> GeneratePersonalizedAdviceAsync(
            Student student,
            List<ExamResult> recentExams,
            string? studentMessage)
        {
            // API anahtarı yoksa hatayı burada, gerçek istek atılacağı sırada fırlatıyoruz
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("Gemini API anahtarı appsettings veya User Secrets içinde bulunamadı.");
            }

            var topWeakTopics = recentExams
                .SelectMany(e => e.TopicErrors)
                .GroupBy(t => new { t.Subject, t.TopicName })
                .Select(g => new
                {
                    Subject = g.Key.Subject,
                    Topic = g.Key.TopicName,
                    TotalErrors = g.Sum(x => x.IncorrectCount + x.BlankCount)
                })
                .OrderByDescending(x => x.TotalErrors)
                .Take(5)
                .ToList();

            var topicSummary = topWeakTopics.Any()
                ? string.Join("\n", topWeakTopics.Select(t => $"- {t.Subject} / {t.Topic}: Toplam {t.TotalErrors} Yanlış/Boş"))
                : "Belirgin bir konu eksiği kaydı bulunmuyor.";

            double avgMath = recentExams.Any() ? Math.Round(recentExams.Average(e => e.MathNet), 2) : 0;
            double avgTurk = recentExams.Any() ? Math.Round(recentExams.Average(e => e.TurkishNet), 2) : 0;
            double avgSci = recentExams.Any() ? Math.Round(recentExams.Average(e => e.ScienceNet), 2) : 0;
            double avgSoc = recentExams.Any() ? Math.Round(recentExams.Average(e => e.SocialNet), 2) : 0;

            var lastExam = recentExams.LastOrDefault();

            string studentContext = $"""
                ÖĞRENCİ PROFİLİ:
                - İsim: {student.Name}
                - Hedef: {student.TargetUniversity} - {student.TargetDepartment} ({student.Field})
                - Öğrenme Stili: {student.LearningStyle}
                - Günlük Çalışma Kapasitesi: {student.DailyAvailableStudyHours} Saat
                - Mevcut Stres Seviyesi (1-10): {student.CurrentStressLevel}/10

                SON {recentExams.Count} DENEME PERFORMANS ÖZETİ:
                - Ortalama Netler: Türkçe: {avgTurk} | Matematik: {avgMath} | Fen: {avgSci} | Sosyal: {avgSoc}
                {(lastExam != null ? $"- En Son Deneme Toplam Net: {lastExam.TotalNet}" : "")}

                KRONİKLEŞEN EKSİKLER:
                {topicSummary}

                ÖĞRENCİ MESAJI: "{(string.IsNullOrWhiteSpace(studentMessage) ? "Bugün nasıl bir çalışma stratejisi izlemeliyim?" : studentMessage)}"
                """;

            string fullPrompt = $@"{AiPrompts.YksUzmani}{AiPrompts.AnalitikZeka}{AiPrompts.JsonZorlayici}

AŞAĞIDAKİ ÖĞRENCİ VERİLERİNİ ANALİZ ET:{studentContext}";

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = fullPrompt } } }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    temperature = 0.7
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, GeminiEndpoint)
            {
                Content = JsonContent.Create(requestBody)
            };
            request.Headers.Add("x-goog-api-key", _apiKey);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API Hatası ({StatusCode}): {ErrorContent}", response.StatusCode, errorContent);
                throw new HttpRequestException("Gemini API isteği başarısız oldu.");
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

            var aiJsonString = jsonResponse
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(aiJsonString))
                throw new InvalidOperationException("Gemini'den boş yanıt döndü.");

            var analysisResult = JsonSerializer.Deserialize<AiExamAnalysisResult>(aiJsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return analysisResult ?? throw new InvalidOperationException("JSON dönüştürme başarısız oldu.");
        }

        /// <summary>
        /// AiCoachController tarafından çağrılan genel analiz metodu.
        /// </summary>
        public async Task<string> GenerateStudentAnalysisAsync(int studentId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new KeyNotFoundException($"{studentId} ID'li öğrenci bulunamadı.");

            var recentExams = await _context.ExamResults
                .Include(e => e.TopicErrors)
                .Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.ExamDate)
                .Take(5)
                .ToListAsync();

            var result = await GeneratePersonalizedAdviceAsync(student, recentExams, "Son durumumu ve genel denemelerimi analiz et.");

            string contentText = result.GenelDegerlendirme ?? "Analiz tamamlandı.";

            var history = new AiAdviceHistory
            {
                StudentId = studentId,
                AdviceType = "Gelişmiş Deneme ve Profil Analizi",
                Content = contentText,
                GenelDegerlendirme = contentText,
                HaftalikOdakTavsiyesi = result.HaftalikOdakTavsiyesi ?? "",
                KirmiziAlarmDersleri = result.KirmiziAlarmDersleri != null ? string.Join(", ", result.KirmiziAlarmDersleri) : "",
                CreatedAt = DateTime.UtcNow
            };

            _context.AiAdviceHistories.Add(history);
            await _context.SaveChangesAsync();

            return contentText;
        }

        /// <summary>
        /// AiCoachController tarafından çağrılan anlık soru-cevap metodu.
        /// </summary>
        public async Task<string> AskQuestionAsync(int studentId, string question)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new KeyNotFoundException($"{studentId} ID'li öğrenci bulunamadı.");

            var recentExams = await _context.ExamResults
                .Include(e => e.TopicErrors)
                .Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.ExamDate)
                .Take(5)
                .ToListAsync();

            var result = await GeneratePersonalizedAdviceAsync(student, recentExams, question);

            string responseText = result.GenelDegerlendirme ?? "Sorunuz yanıtlandı.";

            var history = new AiAdviceHistory
            {
                StudentId = studentId,
                AdviceType = $"Soru: {question}",
                Content = responseText,
                GenelDegerlendirme = responseText,
                HaftalikOdakTavsiyesi = result.HaftalikOdakTavsiyesi ?? "",
                CreatedAt = DateTime.UtcNow
            };

            _context.AiAdviceHistories.Add(history);
            await _context.SaveChangesAsync();

            return responseText;
        }
    }
}