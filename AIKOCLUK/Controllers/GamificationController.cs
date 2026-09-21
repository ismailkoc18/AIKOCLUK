using AIKOCLUK.DTOs;
using AIKOCLUK.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIKOCLUK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamificationController : ControllerBase
    {
        private readonly GamificationService _gamificationService;

        public GamificationController(GamificationService gamificationService)
        {
            _gamificationService = gamificationService;
        }

        /// <summary>
        /// Öğrencinin Seri (Streak), XP, Seviye ve Rozet durumunu getirir.
        /// </summary>
        [HttpGet("overview/{studentId}")]
        public async Task<ActionResult<GamificationOverviewDto>> GetOverview(int studentId)
        {
            try
            {
                var data = await _gamificationService.GetStudentGamificationOverviewAsync(studentId);
                return Ok(data);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Gamification verisi alınırken hata oluştu.", error = ex.Message });
            }
        }
    }
}
