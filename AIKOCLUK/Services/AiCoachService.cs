using AIKOCLUK.Models;
using AIKOCLUK.Core.Constants;
using System.Net.Http.Json;
using System.Text.Json;

namespace AIKOCLUK.Services
{
    public class AiCoachService : IAiCoachService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiCoachService> _logger;
        private readonly string? _apiKey;

        private const string GeminiEndpoint =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        public AiCoachService(HttpClient httpClient, IConfiguration configuration, ILogger<AiCoachService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
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

            string fullPrompt = $@"{AiPrompts.YksUzmani}
{AiPrompts.AnalitikZeka}
{AiPrompts.JsonZorlayici}

AŞAĞIDAKİ ÖĞRENCİ VERİLERİNİ ANALİZ ET:
{studentContext}";

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
    }
}