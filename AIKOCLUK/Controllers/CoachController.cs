using AIKOCLUK.Data;
using AIKOCLUK.Models;
using AIKOCLUK.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIKOCLUK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoachController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AiCoachService _aiCoachService;

        public CoachController(AppDbContext context, AiCoachService aiCoachService)
        {
            _context = context;
            _aiCoachService = aiCoachService;
        }

        [HttpPost("analyze/{studentId}")]
        public async Task<IActionResult> GenerateAdvice(int studentId, [FromBody] AdviceRequestDto request)
        {
            // 1. Öğrenciyi ve son deneme sonucunu veritabanından çekiyoruz
            var student = await _context.Students
                .Include(s => s.ExamResults)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound(new { message = "Öğrenci bulunamadı." });
            }

            var lastExam = student.ExamResults
                .OrderByDescending(e => e.ExamDate)
                .FirstOrDefault();

            try
            {
                // 2. Gemini Yapay Zeka Servisini çağırıyoruz
                string advice = await _aiCoachService.GeneratePersonalizedAdviceAsync(student, lastExam, request?.StudentNote);

                // 3. Üretilen tavsiyeyi veritabanına (AiFeedback) kaydediyoruz
                var aiFeedback = new AiFeedback
                {
                    StudentId = student.Id,
                    GeneratedAdvice = advice,
                    StudentNote = request?.StudentNote ?? string.Empty,
                    DetectedSentiment = student.CurrentStressLevel >= 7 ? "Stresli/Kaygılı" : "Dengeli",
                    Date = DateTime.UtcNow
                };

                _context.AiFeedbacks.Add(aiFeedback);
                await _context.SaveChangesAsync();

                // 4. İstemciye yanıtı dönüyoruz
                return Ok(new
                {
                    StudentName = student.Name,
                    Target = $"{student.TargetUniversity} - {student.TargetDepartment}",
                    Advice = advice,
                    CreatedAt = aiFeedback.Date
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "AI Tavsiyesi üretilirken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}