using AIKOCLUK.Data;
using AIKOCLUK.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// CS0104 çakışmasını önlemek için DTO'ları açıkça tanımlıyoruz
using ExamResultCreateDto = AIKOCLUK.DTOs.ExamResultCreateDto;
using ExamResultResponseDto = AIKOCLUK.DTOs.ExamResultResponseDto;
using ExamResultUpdateDto = AIKOCLUK.DTOs.ExamResultUpdateDto;

namespace AIKOCLUK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamResultsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExamResultsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Bir öğrenciye ait tüm deneme sonuçlarını getirir.
        /// </summary>
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<ExamResultResponseDto>>> GetExamResultsByStudent(int studentId)
        {
            var exists = await _context.Students.AnyAsync(s => s.Id == studentId);
            if (!exists)
                return NotFound(new { message = $"{studentId} ID'li öğrenci bulunamadı." });

            var exams = await _context.ExamResults
                .Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.ExamDate)
                .Select(e => new ExamResultResponseDto
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    ExamType = e.ExamType,
                    ExamDate = e.ExamDate,
                    TurkishNet = e.TurkishNet,
                    MathNet = e.MathNet,
                    ScienceNet = e.ScienceNet,
                    SocialNet = e.SocialNet,
                    TimeManagementIssue = e.TimeManagementIssue
                })
                .ToListAsync();

            return Ok(exams);
        }

        /// <summary>
        /// Tekil bir deneme sonucunun detayını getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ExamResultResponseDto>> GetExamResult(int id)
        {
            var exam = await _context.ExamResults.FindAsync(id);

            if (exam == null)
                return NotFound(new { message = $"{id} ID'li deneme sonucu bulunamadı." });

            var response = new ExamResultResponseDto
            {
                Id = exam.Id,
                StudentId = exam.StudentId,
                ExamType = exam.ExamType,
                ExamDate = exam.ExamDate,
                TurkishNet = exam.TurkishNet,
                MathNet = exam.MathNet,
                ScienceNet = exam.ScienceNet,
                SocialNet = exam.SocialNet,
                TimeManagementIssue = exam.TimeManagementIssue
            };

            return Ok(response);
        }

        /// <summary>
        /// Öğrenci için yeni bir deneme sonucu kaydeder.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ExamResultResponseDto>> CreateExamResult([FromBody] ExamResultCreateDto dto)
        {
            var studentExists = await _context.Students.AnyAsync(s => s.Id == dto.StudentId);
            if (!studentExists)
                return BadRequest(new { message = $"Geçersiz StudentId: {dto.StudentId} bulunamadı." });

            var exam = new ExamResult
            {
                StudentId = dto.StudentId,
                ExamType = dto.ExamType,
                ExamDate = dto.ExamDate,
                TurkishNet = dto.TurkishNet,
                MathNet = dto.MathNet,
                ScienceNet = dto.ScienceNet,
                SocialNet = dto.SocialNet,
                TimeManagementIssue = dto.TimeManagementIssue
            };

            _context.ExamResults.Add(exam);
            await _context.SaveChangesAsync();

            var response = new ExamResultResponseDto
            {
                Id = exam.Id,
                StudentId = exam.StudentId,
                ExamType = exam.ExamType,
                ExamDate = exam.ExamDate,
                TurkishNet = exam.TurkishNet,
                MathNet = exam.MathNet,
                ScienceNet = exam.ScienceNet,
                SocialNet = exam.SocialNet,
                TimeManagementIssue = exam.TimeManagementIssue
            };

            return CreatedAtAction(nameof(GetExamResult), new { id = exam.Id }, response);
        }

        /// <summary>
        /// Mevcut bir deneme sonucunu günceller.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExamResult(int id, [FromBody] ExamResultUpdateDto dto)
        {
            var exam = await _context.ExamResults.FindAsync(id);

            if (exam == null)
                return NotFound(new { message = $"{id} ID'li deneme sonucu bulunamadı." });

            exam.ExamType = dto.ExamType;
            exam.ExamDate = dto.ExamDate;
            exam.TurkishNet = dto.TurkishNet;
            exam.MathNet = dto.MathNet;
            exam.ScienceNet = dto.ScienceNet;
            exam.SocialNet = dto.SocialNet;
            exam.TimeManagementIssue = dto.TimeManagementIssue;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Hatalı veya silinmek istenen bir deneme sonucunu siler.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExamResult(int id)
        {
            var exam = await _context.ExamResults.FindAsync(id);

            if (exam == null)
                return NotFound(new { message = $"{id} ID'li deneme sonucu bulunamadı." });

            _context.ExamResults.Remove(exam);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}