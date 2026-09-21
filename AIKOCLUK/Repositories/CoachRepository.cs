using AIKOCLUK.Data;
using AIKOCLUK.Models;
using Microsoft.EntityFrameworkCore;

namespace AIKOCLUK.Repositories
{
    public class CoachRepository : ICoachRepository
    {
        private readonly AppDbContext _context;

        public CoachRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Student?> GetStudentWithExamsAsync(int studentId)
        {
            return await _context.Students
                .Include(s => s.ExamResults)
                .FirstOrDefaultAsync(s => s.Id == studentId);
        }

        public async Task<bool> StudentExistsAsync(int studentId)
        {
            return await _context.Students.AnyAsync(s => s.Id == studentId);
        }

        public async Task<List<AiFeedback>> GetStudentHistoryAsync(int studentId)
        {
            return await _context.AiFeedbacks
                .Where(f => f.StudentId == studentId)
                .OrderByDescending(f => f.Date)
                .ToListAsync();
        }

        public async Task AddAiFeedbackAsync(AiFeedback feedback)
        {
            await _context.AiFeedbacks.AddAsync(feedback);
        }

        public async Task AddExamResultAsync(ExamResult examResult)
        {
            await _context.ExamResults.AddAsync(examResult);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // --- SON 5 DENEME VE KONU DETAYI METOTLARI ---

        public async Task<List<ExamResult>> GetRecentExamsWithTopicsAsync(int studentId, int count = 5)
        {
            var recentExams = await _context.ExamResults
                .Include(e => e.TopicErrors)
                .Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.ExamDate)
                .Take(count)
                .ToListAsync();

            return recentExams.OrderBy(e => e.ExamDate).ToList();
        }

        public async Task AddExamResultWithTopicsAsync(ExamResult examResult, List<ExamTopicError> topicErrors)
        {
            // İki ayrı SaveChanges arada başarısız olursa konu hatası içermeyen bir
            // "yetim" deneme sonucu kalmasın diye tek transaction'da yapıyoruz.
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.ExamResults.AddAsync(examResult);
                await _context.SaveChangesAsync();

                foreach (var error in topicErrors)
                {
                    error.ExamResultId = examResult.Id;
                }

                await _context.ExamTopicErrors.AddRangeAsync(topicErrors);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}