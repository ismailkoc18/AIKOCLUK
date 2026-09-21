using AIKOCLUK.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIKOCLUK.Services
{
    public interface IAiCoachService
    {
        Task<AiExamAnalysisResult> GeneratePersonalizedAdviceAsync(
            Student student,
            List<ExamResult> recentExams,
            string? studentMessage);
    }
}