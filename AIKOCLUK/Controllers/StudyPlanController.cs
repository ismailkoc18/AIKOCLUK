using AIKOCLUK.DTOs;
using AIKOCLUK.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AIKOCLUK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudyPlanController : ControllerBase
    {
        private readonly StudyPlanService _studyPlanService;

        public StudyPlanController(StudyPlanService studyPlanService)
        {
            _studyPlanService = studyPlanService;
        }

        /// <summary>
        /// Öğrencinin eksik konularına ve günlük boş saatine özel haftalık çalışma programı oluşturur.
        /// </summary>
        [HttpPost("generate/{studentId}")]
        [EnableRateLimiting("ai-analyze")]
        public async Task<ActionResult<WeeklyStudyPlanResponseDto>> GenerateWeeklyPlan(int studentId)
        {
            try
            {
                var plan = await _studyPlanService.GenerateWeeklyPlanAsync(studentId);
                return Ok(plan);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ders programı üretilirken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}