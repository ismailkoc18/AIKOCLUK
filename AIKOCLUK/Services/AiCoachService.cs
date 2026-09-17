using Google.GenAI;
using AIKOCLUK.Models;
using Microsoft.Extensions.Configuration;

namespace AIKOCLUK.Services
{
    public class AiCoachService
    {
        private readonly string _apiKey;

        public AiCoachService(IConfiguration configuration)
        {
            _apiKey = configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        }

        public async Task<string> GenerateStudyPlanAsync(Student student, ExamResult lastExam)
        {
            // Gemini istemcisini API anahtarı ile başlatıyoruz
            var client = new Client(apiKey: _apiKey);

            string prompt = $"""
                Sen profesyonel bir YKS (Yükseköğretim Kurumları Sınavı) koçusun.
                Öğrenci Bilgileri:
                - Hedef Bölüm: {student.TargetDepartment}
                - Günlük Çalışma Saati: {student.DailyStudyHours} saat

                Son Deneme Sonuçları:
                - Türkçe Neti: {lastExam.TurkishNet}
                - Matematik Neti: {lastExam.MathNet}
                - Fen Neti: {lastExam.ScienceNet}
                - Sosyal Net: {lastExam.SocialNet}
                - Zayıf / Hatalı Olunan Konular: {lastExam.WeakTopics}

                Bu verilere dayanarak öğrenciye özel, motive edici, haftalık pratik bir çalışma planı ve eksiklerini kapatması için stratejik tavsiyeler hazırla.
                """;

            // En güncel ve hızlı Gemini modelini kullanıyoruz
            var response = await client.Models.GenerateContentAsync(
                model: "gemini-2.5-flash",
                contents: prompt
            );

            return response.Text ?? "Yapay zeka koçluk planı oluşturamadı.";
        }
    }
}