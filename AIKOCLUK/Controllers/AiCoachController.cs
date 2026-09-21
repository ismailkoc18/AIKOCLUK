using AIKOCLUK.Data;
using AIKOCLUK.DTOs;
using AIKOCLUK.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AIKOCLUK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiCoachController : ControllerBase
    {
        private readonly AiCoachService _aiCoachService;
        private readonly AppDbContext _context;

        public AiCoachController(AiCoachService aiCoachService, AppDbContext context)
        {
            _aiCoachService = aiCoachService;
            _context = context;
        }

        /// <summary>
        /// Öğrencinin son deneme netlerini, stres seviyesini ve zayıf konularını Gemini AI ile analiz eder.
        /// Tavsiyeyi veritabanına kaydeder ve yanıtı döner.
        /// </summary>
        [HttpPost("analyze/{studentId}")]
        [EnableRateLimiting("ai-analyze")]
        public async Task<IActionResult> AnalyzeStudent(int studentId)
        {
            var studentExists = await _context.Students.AnyAsync(s => s.Id == studentId);
            if (!studentExists)
                return NotFound(new { message = $"{studentId} ID'li öğrenci bulunamadı." });

            try
            {
                var analysisResult = await _aiCoachService.GenerateStudentAnalysisAsync(studentId);
                return Ok(analysisResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "AI Koç analizi üretilirken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Öğrencinin geçmişte yapay zekadan aldığı tüm tavsiyeleri listeler.
        /// </summary>
        [HttpGet("history/{studentId}")]
        public async Task<ActionResult<IEnumerable<AiAdviceHistoryResponseDto>>> GetAdviceHistory(int studentId)
        {
            var exists = await _context.Students.AnyAsync(s => s.Id == studentId);
            if (!exists)
                return NotFound(new { message = $"{studentId} ID'li öğrenci bulunamadı." });

            var history = await _context.AiAdviceHistories
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AiAdviceHistoryResponseDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    AdviceType = a.AdviceType,
                    Content = a.Content,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(history);
        }

        /// <summary>
        /// Öğrencinin AI Koç'a anlık soru sorabileceği (örn: "Geometri netlerim artmıyor ne yapmalıyım?") interaktif uç.
        /// </summary>
        [HttpPost("ask")]
        [EnableRateLimiting("ai-analyze")]
        public async Task<ActionResult<AiAskResponseDto>> AskCoach([FromBody] AiAskRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Question))
                return BadRequest(new { message = "Soru alanı boş olamaz." });

            var studentExists = await _context.Students.AnyAsync(s => s.Id == dto.StudentId);
            if (!studentExists)
                return NotFound(new { message = $"{dto.StudentId} ID'li öğrenci bulunamadı." });

            try
            {
                var answer = await _aiCoachService.AskQuestionAsync(dto.StudentId, dto.Question);

                return Ok(new AiAskResponseDto
                {
                    Answer = answer,
                    RespondedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "AI Koç yanıt verirken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}