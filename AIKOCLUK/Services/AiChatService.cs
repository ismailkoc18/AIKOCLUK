using AIKOCLUK.Data;
using AIKOCLUK.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text.Json;

namespace AIKOCLUK.Services
{
    public class AiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly ILogger<AiChatService> _logger;
        private readonly string? _apiKey;

        private const string GeminiEndpoint =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        public AiChatService(
            HttpClient httpClient,
            AppDbContext context,
            IConfiguration configuration,
            ILogger<AiChatService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
            _apiKey = configuration["Gemini:ApiKey"];
        }

        public async Task<ChatResponseDto> SendMessageAsync(ChatRequestDto request)
        {
            if (string.IsNullOrEmpty(_apiKey))
                throw new InvalidOperationException("Gemini API anahtarı tanımlı değil.");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == request.StudentId);

            if (student == null)
                throw new KeyNotFoundException($"{request.StudentId} ID'li öğrenci bulunamadı.");

            // Sohbet geçmişini Gemini formatına dönüştürüyoruz
            var contentsList = new List<object>
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = $"Sistem Talimatı: Sen {student.Name} isimli YKS adayının kişisel AI Eğitim Koçusun. Öğrencinin Hedefi: {student.TargetUniversity} - {student.TargetDepartment}. Alanı: {student.Field}. Samimi, motive edici, yapıcı ve doğrudan hedefe yönelik Türkçe yanıtlar ver." } }
                },
                new
                {
                    role = "model",
                    parts = new[] { new { text = "Anlaşıldı! Öğrencimin hedeflerine ulaşması için rehberlik etmeye hazırım." } }
                }
            };

            foreach (var msg in request.ConversationHistory)
            {
                contentsList.Add(new
                {
                    role = msg.Sender.ToLower() == "user" ? "user" : "model",
                    parts = new[] { new { text = msg.Message } }
                });
            }

            contentsList.Add(new
            {
                role = "user",
                parts = new[] { new { text = request.UserMessage } }
            });

            var requestBody = new
            {
                contents = contentsList,
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 1000
                }
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, GeminiEndpoint)
            {
                Content = JsonContent.Create(requestBody)
            };
            httpRequest.Headers.Add("x-goog-api-key", _apiKey);

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini Chat Hatası: {Error}", err);
                throw new HttpRequestException("AI Koç ile iletişim kurulırken bir hata oluştu.");
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
            var aiReply = jsonResponse
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "Üzgünüm, şu an yanıt üretemiyorum.";

            return new ChatResponseDto
            {
                AiReply = aiReply,
                RespondedAt = DateTime.UtcNow
            };
        }

        public async Task<List<AdviceHistoryDto>> GetStudentAdviceHistoryAsync(int studentId)
        {
            return await _context.AiAdviceHistories
                .Where(h => h.StudentId == studentId)
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new AdviceHistoryDto
                {
                    Id = h.Id,
                    StudentId = h.StudentId,
                    AdviceType = h.AdviceType,
                    Content = h.Content,
                    GenelDegerlendirme = h.GenelDegerlendirme,
                    HaftalikOdakTavsiyesi = h.HaftalikOdakTavsiyesi,
                    KirmiziAlarmDersleri = h.KirmiziAlarmDersleri,
                    CreatedAt = h.CreatedAt
                })
                .ToListAsync();
        }
    }
}