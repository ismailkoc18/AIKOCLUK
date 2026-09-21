using AIKOCLUK.Data;
using AIKOCLUK.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AIKOCLUK.Services
{
    public class TopicAnalyticsService
    {
        private readonly AppDbContext _context;

        public TopicAnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SubjectTopicTreeDto>> GetStudentTopicTreeAsync(int studentId)
        {
            var topicErrors = await _context.ExamResults
                .Where(e => e.StudentId == studentId)
                .SelectMany(e => e.TopicErrors)
                .ToListAsync();

            var groupedBySubject = topicErrors
                .GroupBy(t => t.Subject)
                .Select(subjectGroup => new SubjectTopicTreeDto
                {
                    SubjectName = subjectGroup.Key,
                    TotalErrorsInSubject = subjectGroup.Sum(x => x.IncorrectCount + x.BlankCount),
                    Topics = subjectGroup
                        .GroupBy(t => t.TopicName)
                        .Select(topicGroup =>
                        {
                            int inc = topicGroup.Sum(x => x.IncorrectCount);
                            int blk = topicGroup.Sum(x => x.BlankCount);
                            int total = inc + blk;

                            string status = total switch
                            {
                                >= 5 => "Kritik",
                                >= 2 => "GelisimAlani",
                                _ => "Kuvvetli"
                            };

                            return new TopicPerformanceDto
                            {
                                TopicName = topicGroup.Key,
                                TotalIncorrect = inc,
                                TotalBlank = blk,
                                StatusLevel = status
                            };
                        })
                        .OrderByDescending(t => t.TotalErrors)
                        .ToList()
                })
                .OrderByDescending(s => s.TotalErrorsInSubject)
                .ToList();

            return groupedBySubject;
        }
    }
}