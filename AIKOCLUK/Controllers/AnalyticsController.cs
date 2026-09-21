using AIKOCLUK.DTOs;
using AIKOCLUK.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIKOCLUK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly ExamAnalyticsService _analyticsService;
        private readonly TopicAnalyticsService _topicAnalyticsService;

        public AnalyticsController(
            ExamAnalyticsService analyticsService,
            TopicAnalyticsService topicAnalyticsService)
        {
            _analyticsService = analyticsService;
            _topicAnalyticsService = topicAnalyticsService;
        }

        /// <summary>
        /// Öğrencinin deneme grafik verilerini (Net trendi, Hata kök nedenleri, Konu hakimiyeti, Hedef kıyası) getirir.
        /// </summary>
        /// <param name="studentId">Öğrenci ID (Seed data için varsayılan: 1)</param>
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<DetailedExamAnalyticsDto>> GetStudentAnalytics(int studentId)
        {
            try
            {
                var data = await _analyticsService.GetStudentAnalyticsAsync(studentId);
                return Ok(data);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Grafik verileri hazırlanırken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Öğrencinin konu bazlı müfredat ağacını ve durum seviyelerini (Kritik, Gelişim Alanı, Kuvvetli) getirir.
        /// </summary>
        /// <param name="studentId">Öğrenci ID</param>
        [HttpGet("topic-tree/{studentId}")]
        public async Task<IActionResult> GetTopicTree(int studentId)
        {
            try
            {
                var tree = await _topicAnalyticsService.GetStudentTopicTreeAsync(studentId);
                return Ok(tree);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Müfredat ağacı verileri hazırlanırken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}