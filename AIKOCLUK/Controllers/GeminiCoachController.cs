using Microsoft.AspNetCore.Mvc;
using AIKOCLUK.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AIKOCLUK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiCoachController : ControllerBase
    {
        private readonly IGeminiService _geminiService;

        public GeminiCoachController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        /// <summary>
        /// Gemini Pro ile Canlı AI Koç Sohbet Uç Noktası
        /// Route: POST /api/GeminiCoach/chat
        /// </summary>
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] GeminiCoachChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { success = false, message = "Mesaj alanı boş bırakılamaz." });
            }

            try
            {
                var responseText = await _geminiService.SendCoachChatMessageAsync(
                    request.History ?? new List<ChatMessageDto>(),
                    request.Message,
                    request.Persona ?? "YKS Derece Koçu"
                );

                return Ok(new { success = true, response = responseText });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Gemini servisiyle iletişim kurulurken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Gemini Flash ile Hızlı Günlük Motivasyon ve Açılış Özet Uç Noktası
        /// Route: GET /api/GeminiCoach/daily-briefing
        /// </summary>
        [HttpGet("daily-briefing")]
        public async Task<IActionResult> GetDailyBriefing(
            [FromQuery] string name = "Öğrenci",
            [FromQuery] int solved = 120,
            [FromQuery] int target = 200)
        {
            try
            {
                var briefing = await _geminiService.GetDailyBriefingAsync(name, solved, target);
                return Ok(new { success = true, briefing });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Özet oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Chat isteği için model sınıfı
    /// </summary>
    public class GeminiCoachChatRequest
    {
        public List<ChatMessageDto>? History { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Persona { get; set; }
    }
}