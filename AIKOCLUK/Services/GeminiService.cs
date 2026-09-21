using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AIKOCLUK.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // secrets.json veya appsettings.json içindeki Gemini:ApiKey değerini çeker
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new Exception("Gemini API Key bulunamadı!");
        }

        // 1. Düşük Maliyetli Günlük Özet (Gemini Flash)
        public async Task<string> GetDailyBriefingAsync(string studentName, int solvedQuestions, int targetQuestions)
        {
            var prompt = $@"
                Sen AIKOCLUK platformunda görev yapan motive edici bir YKS sınav koçusun.
                Öğrenci Adı: {studentName}
                Bugün Çözülen Soru: {solvedQuestions} / Hedef: {targetQuestions}

                Öğrenciye günün açılışında gösterilmek üzere 2 cümlelik enerjik, motive edici ve aksiyon odaklı bir mesaj yaz.";

            return await CallGeminiApiAsync("gemini-2.5-flash", prompt);
        }

        // 2. Canlı YKS Koçluk Sohbeti (Gemini Pro)
        public async Task<string> SendCoachChatMessageAsync(List<ChatMessageDto> history, string userMessage, string persona)
        {
            var contents = new List<object>();

            // Konuşma geçmişini Gemini formatına çevir
            if (history != null)
            {
                foreach (var msg in history)
                {
                    var role = msg.Sender.ToLower() == "user" ? "user" : "model";
                    contents.Add(new
                    {
                        role = role,
                        parts = new[] { new { text = msg.Text } }
                    });
                }
            }

            // Yeni mesajı ekle
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = userMessage } }
            });

            var requestBody = new
            {
                contents = contents,
                systemInstruction = new
                {
                    parts = new[] { new { text = $"Sen AIKOCLUK sisteminde görev yapan uzman bir '{persona}'sın. Türkiye YKS müfredatına hakimsin. Yanıtların samimi, yapılandırılmış ve yönlendirici olmalı." } }
                }
            };

            return await PostToGeminiApiAsync("gemini-2.5-pro", requestBody);
        }

        private async Task<string> CallGeminiApiAsync(string model, string prompt)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            return await PostToGeminiApiAsync(model, requestBody);
        }

        private async Task<string> PostToGeminiApiAsync(string model, object requestBody)
        {
            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    return "Üzgünüm, Gemini API yanıt veremedi. Lütfen API key'inizi kontrol edin.";
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);

                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "Boş yanıt alındı.";
            }
            catch (Exception ex)
            {
                return $"Bir hata oluştu: {ex.Message}";
            }
        }
    }
}