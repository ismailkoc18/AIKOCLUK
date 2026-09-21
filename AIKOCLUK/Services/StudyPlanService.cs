using AIKOCLUK.Data;
using AIKOCLUK.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text.Json;

namespace AIKOCLUK.Services
{
    public class StudyPlanService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly ILogger<StudyPlanService> _logger;
        private readonly string? _apiKey;

        private const string GeminiEndpoint =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        public StudyPlanService(
            HttpClient httpClient,
            AppDbContext context,
            IConfiguration configuration,
            ILogger<StudyPlanService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
            _apiKey = configuration["Gemini:ApiKey"];
        }

        public async Task<WeeklyStudyPlanResponseDto> GenerateWeeklyPlanAsync(int studentId)
        {
            if (string.IsNullOrEmpty(_apiKey))
                throw new InvalidOperationException("Gemini API anahtarı yapılandırılmamış.");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new KeyNotFoundException($"{studentId} ID'li öğrenci bulunamadı.");

            var topErrors = await _context.ExamResults
                .Where(e => e.StudentId == studentId)
                .SelectMany(e => e.TopicErrors)
                .GroupBy(t => new { t.Subject, t.TopicName })
                .Select(g => new
                {
                    Subject = g.Key.Subject,
                    Topic = g.Key.TopicName,
                    Total = g.Sum(x => x.IncorrectCount + x.BlankCount)
                })
                .OrderByDescending(x => x.Total)
                .Take(6)
                .ToListAsync();

            string weakTopicsText = topErrors.Any()
                ? string.Join(", ", topErrors.Select(t => $"{t.Subject}-{t.Topic}"))
                : student.WeakSubjects ?? "Genel Tekrar";

            string prompt = $@"
Sen uzman bir YKS Eğitim Danışmanısın.
Aşağıdaki öğrenci bilgileri ve öncelikli eksik konularına göre tam 7 günlük (Pazartesi - Pazar) detaylı ve dengeli bir çalışma programı hazırla.

ÖĞRENCİ BİLGİLERİ:
- Adı: {student.Name}
- Hedef: {student.TargetUniversity} - {student.TargetDepartment} ({student.Field})
- Günlük Ayırabileceği Çalışma Süresi: {student.DailyAvailableStudyHours} Saat
- Stres Seviyesi: {student.CurrentStressLevel}/10
- En Çok Hata Yapılan Kritik Konular: {weakTopicsText}

YÖNERGE:
1. Yanıtı SADECE geçerli bir JSON nesnesi olarak döndür.
2. JSON şeması tam olarak şu yapıda olmalıdır:
{{
  ""targetSummary"": ""Öğrencinin bu haftaki ana hedefi ve stratejisi"",
  ""weeklySchedule"": [
    {{
      ""day"": ""Pazartesi"",
      ""totalTargetHours"": {student.DailyAvailableStudyHours},
      ""blocks"": [
        {{
          ""timeSlot"": ""09:00 - 10:30"",
          ""subject"": ""Matematik"",
          ""topic"": ""Türev"",
          ""activityType"": ""Konu Anlatımı + 40 Soru"",
          ""adviceNote"": ""Zorlandığın soru tiplerini işaretle.""
        }}
      ]
    }}
  ]
}}";

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    temperature = 0.5
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
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API Çalışma Programı Hatası: {Error}", err);
                throw new HttpRequestException("Çalışma programı üretilemedi.");
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
            var rawJson = jsonResponse
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(rawJson))
                throw new InvalidOperationException("Gemini'den boş çalışma programı döndü.");

            var planResult = JsonSerializer.Deserialize<WeeklyStudyPlanResponseDto>(rawJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new WeeklyStudyPlanResponseDto();

            planResult.StudentId = studentId;
            planResult.GeneratedAt = DateTime.UtcNow;

            return planResult;
        }
    }
}