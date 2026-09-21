using AIKOCLUK.DTOs;
using AIKOCLUK.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AIKOCLUK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiChatController : ControllerBase
    {
        private readonly AiChatService _chatService;

        public AiChatController(AiChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// AI Eğitim Koçu ile sohbet mesajı gönderir ve canlı yanıt alır.
        /// </summary>
        [HttpPost("chat")]
        [EnableRateLimiting("ai-analyze")]
        public async Task<ActionResult<ChatResponseDto>> Chat([FromBody] ChatRequestDto request)
        {
            try
            {
                var response = await _chatService.SendMessageAsync(request);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sohbet işlenirken hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Öğrencinin geçmiş AI tavsiyelerini ve koçluk geçmişini getirir.
        /// </summary>
        [HttpGet("advice-history/{studentId}")]
        public async Task<ActionResult<List<AdviceHistoryDto>>> GetAdviceHistory(int studentId)
        {
            var history = await _chatService.GetStudentAdviceHistoryAsync(studentId);
            return Ok(history);
        }
    }
}