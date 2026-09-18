using AIKOCLUK.Models;
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

        public async Task<string> GeneratePersonalizedAdviceAsync(Student student, ExamResult? lastExam, string? studentMessage)
        {
            // Model adı güncel gemini-2.5-flash sürümüne güncellendi
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            // Öğrencinin durumuna ve veritabanı kayıtlarına göre dinamik sistem konsepti inşa ediyoruz
            string prompt = $"""
                Sen YKS (Yükseköğretim Kurumları Sınavı) hazırlık sürecinde uzman, empati yeteneği yüksek ve veriye dayalı yönlendirme yapan profesyonel bir Eğitim Koçusun.
                
                ÖĞRENCİ PROFİLİ:
                - İsim: {student.Name}
                - Hedef: {student.TargetUniversity} - {student.TargetDepartment} ({student.Field})
                - Öğrenme Stili: {student.LearningStyle}
                - Günlük Çalışma Kapasitesi: {student.DailyAvailableStudyHours} Saat
                - Mevcut Stres Seviyesi (1-10): {student.CurrentStressLevel}/10
                - En Çok Zorlandığı Konular/Dersler: {student.WeakSubjects}

                {(lastExam != null ? $"""
                SON DENEME PERFORMANSI ({lastExam.ExamType} - {lastExam.ExamDate:dd.MM.yyyy}):
                - Türkçe Net: {lastExam.TurkishNet}
                - Matematik Net: {lastExam.MathNet}
                - Fen Net: {lastExam.ScienceNet}
                - Sosyal Net: {lastExam.SocialNet}
                - Toplam Net: {lastExam.TotalNet}
                - Zaman Yönetimi Sorunu Yaşadı Mı?: {(lastExam.TimeManagementIssue ? "Evet" : "Hayır")}
                """ : "Henüz sistemde bir deneme sonucu kayıtlı değil.")}

                ÖĞRENCİNİN ANLIK NOTU / SORUSU:
                "{(string.IsNullOrWhiteSpace(studentMessage) ? "Bugün nasıl bir çalışma stratejisi izlemeliyim?" : studentMessage)}"

                YÖNERGELER:
                1. Öğrencinin stres seviyesi 7 veya üzerindeyse önce motivasyonel ve rahatlatıcı bir ton kullan, ardından somut aksiyon adımlarına geç.
                2. Yanıtında zayıf olduğu derslere ({student.WeakSubjects}) ve son denemedeki eksiklerine özel, kısa ama etkili taktikler ver.
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