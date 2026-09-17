using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIKOCLUK.Data;
using AIKOCLUK.Models;
using AIKOCLUK.Services;

namespace AIKOCLUK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoachController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AiCoachService _aiCoachService;

        public CoachController(AppDbContext context, AiCoachService aiCoachService)
        {
            _context = context;
            _aiCoachService = aiCoachService;
        }

        // POST: api/coach/generate-plan/{studentId}
        [HttpPost("generate-plan/{studentId}")]
        public async Task<IActionResult> GeneratePlan(int studentId)
        {
            // 1. Öğrenciyi ve son eklenen deneme sonucunu veritabanından buluyoruz
            var student = await _context.Students
                .Include(s => s.ExamResults)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound(new { message = "Öğrenci bulunamadı." });
            }

            var lastExam = student.ExamResults.OrderByDescending(e => e.Date).FirstOrDefault();

            if (lastExam == null)
            {
                return BadRequest(new { message = "Öğrenciye ait kayıtlı bir deneme sonucu bulunamadı. Önce deneme eklemelisiniz." });
            }

            try
            {
                // 2. Gemini Yapay Zeka Servisini tetikliyoruz
                string studyPlan = await _aiCoachService.GenerateStudyPlanAsync(student, lastExam);

                // 3. Sonucu dış dünyaya dönüyoruz
                return Ok(new
                {
                    StudentName = student.Name,
                    Target = student.TargetDepartment,
                    AiStudyPlan = studyPlan
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Yapay zeka planı oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}