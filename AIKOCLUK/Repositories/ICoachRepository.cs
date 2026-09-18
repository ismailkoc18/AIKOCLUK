using AIKOCLUK.Models;

namespace AIKOCLUK.Repositories
{
    public interface ICoachRepository
    {
        Task<Student?> GetStudentWithExamsAsync(int studentId);
        Task<bool> StudentExistsAsync(int studentId);
        Task<List<AiFeedback>> GetStudentHistoryAsync(int studentId);
        Task AddAiFeedbackAsync(AiFeedback feedback);
        Task AddExamResultAsync(ExamResult examResult);
        Task SaveChangesAsync();

        // --- YENİ METOTLAR ---
        Task<List<ExamResult>> GetRecentExamsWithTopicsAsync(int studentId, int count = 5);
        Task AddExamResultWithTopicsAsync(ExamResult examResult, List<ExamTopicError> topicErrors);
    }
}