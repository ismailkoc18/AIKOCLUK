using AIKOCLUK.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace AIKOCLUK.Services
{
    public class AiCoachService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AiCoachService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"]
                ?? throw new InvalidOperationException("Gemini API anahtarı appsettings veya User Secrets içinde bulunamadı.");
        }

        public async Task<string> GeneratePersonalizedAdviceAsync(
            Student student,
            List<ExamResult> recentExams,
            string? studentMessage)
        {
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            // Son denemelerdeki en çok tekrarlanan ilk 5 kronik konu eksiğini buluyoruz
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

            // Son denemelerin net ortalamaları
            double avgMath = recentExams.Any() ? Math.Round(recentExams.Average(e => e.MathNet), 2) : 0;
            double avgTurk = recentExams.Any() ? Math.Round(recentExams.Average(e => e.TurkishNet), 2) : 0;
            double avgSci = recentExams.Any() ? Math.Round(recentExams.Average(e => e.ScienceNet), 2) : 0;
            double avgSoc = recentExams.Any() ? Math.Round(recentExams.Average(e => e.SocialNet), 2) : 0;

            var lastExam = recentExams.LastOrDefault();

            string prompt = $"""
                Sen YKS (Yükseköğretim Kurumları Sınavı) hazırlık sürecinde uzman, empati yeteneği yüksek ve veriye dayalı yönlendirme yapan profesyonel bir Eğitim Koçusun.
                
                ÖĞRENCİ PROFİLİ:
                - İsim: {student.Name}
                - Hedef: {student.TargetUniversity} - {student.TargetDepartment} ({student.Field})
                - Öğrenme Stili: {student.LearningStyle}
                - Günlük Çalışma Kapasitesi: {student.DailyAvailableStudyHours} Saat
                - Mevcut Stres Seviyesi (1-10): {student.CurrentStressLevel}/10
                - En Çok Zorlandığı Konular/Dersler: {student.WeakSubjects}

                SON {recentExams.Count} DENEME PERFORMANS ÖZETİ:
                - Analiz Edilen Deneme Sayısı: {recentExams.Count}
                - Ortalama Netler: Türkçe: {avgTurk} | Matematik: {avgMath} | Fen: {avgSci} | Sosyal: {avgSoc}
                {(lastExam != null ? $"- En Son Deneme Toplam Net ({lastExam.ExamType} - {lastExam.ExamDate:dd.MM.yyyy}): {lastExam.TotalNet} (Süre Sorunu Yaşandı Mı: {(lastExam.TimeManagementIssue ? "Evet" : "Hayır")})" : "Henüz sistemde bir deneme sonucu kayıtlı değil.")}

                SON DENEMELERDE KRONİKLEŞEN EN KRİTİK KONU EKSİKLERİ:
                {topicSummary}

                ÖĞRENCİNİN ANLIK NOTU / SORUSU:
                "{(string.IsNullOrWhiteSpace(studentMessage) ? "Bugün nasıl bir çalışma stratejisi izlemeliyim?" : studentMessage)}"

                YÖNERGELER:
                1. Öğrencinin stres seviyesi 7 veya üzerindeyse önce motivasyonel ve rahatlatıcı bir ton kullan, ardından somut aksiyon adımlarına geç.
                2. Son denemelerde kronikleşen konu eksiklerini ({topicSummary}) önceliklendirerek nokta atışı aksiyon planı çıkar.
                3. Günlük {student.DailyAvailableStudyHours} saatlik kapasitesini aşmayacak gerçekçi bir çalışma planı tavsiyesi sun.
                4. Cevabı doğrudan öğrenciye hitap ederek (İkinci tekil şahıs: "Sen") ve anlaşılır maddeler halinde ver.
                """;

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(endpoint, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Gemini API Hatası ({response.StatusCode}): {errorContent}");
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

            var advice = jsonResponse
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return advice ?? "Şu anda koçluk tavsiyesi üretilemedi. Lütfen tekrar deneyin.";
        }
    }
}