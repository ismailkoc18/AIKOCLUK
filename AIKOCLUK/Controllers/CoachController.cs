using AIKOCLUK.Models;
using AIKOCLUK.Repositories;
using AIKOCLUK.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIKOCLUK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoachController : ControllerBase
    {
        private readonly ICoachRepository _repository;
        private readonly AiCoachService _aiCoachService;

        public CoachController(ICoachRepository repository, AiCoachService aiCoachService)
        {
            _repository = repository;
            _aiCoachService = aiCoachService;
        }

        // 1. Son 5 Deneme & Kronikleşen Konu Hatalarına Dayalı AI Analizi
        [HttpPost("analyze/{studentId}")]
        public async Task<IActionResult> GenerateAdvice(int studentId, [FromBody] AdviceRequestDto request)
        {
            var student = await _repository.GetStudentWithExamsAsync(studentId);
            if (student == null)
            {
                return NotFound(new { message = "Öğrenci bulunamadı." });
            }

            // Öğrencinin son 5 denemesini konu detaylarıyla birlikte çekiyoruz
            var recentExams = await _repository.GetRecentExamsWithTopicsAsync(studentId, 5);
            if (!recentExams.Any())
            {
                return BadRequest(new { message = "Analiz yapılabilmesi için sistemde en az 1 deneme sonucu bulunmalıdır." });
            }

            string advice = await _aiCoachService.GeneratePersonalizedAdviceAsync(student, recentExams, request?.StudentNote);

            var aiFeedback = new AiFeedback
            {
                StudentId = student.Id,
                GeneratedAdvice = advice,
                StudentNote = request?.StudentNote ?? string.Empty,
                DetectedSentiment = student.CurrentStressLevel >= 7 ? "Stresli/Kaygılı" : "Dengeli",
                Date = DateTime.UtcNow
            };

            await _repository.AddAiFeedbackAsync(aiFeedback);
            await _repository.SaveChangesAsync();

            return Ok(new
            {
                StudentName = student.Name,
                Target = $"{student.TargetUniversity} - {student.TargetDepartment}",
                Advice = advice,
                CreatedAt = aiFeedback.Date
            });
        }

        // 2. Son 5 Deneme Gelişim & Kronik Konu İstatistik Raporu
        [HttpGet("progress/{studentId}")]
        public async Task<IActionResult> GetStudentProgress(int studentId)
        {
            var studentExists = await _repository.StudentExistsAsync(studentId);
            if (!studentExists)
            {
                return NotFound(new { message = $"ID'si {studentId} olan öğrenci bulunamadı." });
            }

            var student = await _repository.GetStudentWithExamsAsync(studentId);
            var recentExams = await _repository.GetRecentExamsWithTopicsAsync(studentId, 5);

            if (!recentExams.Any())
            {
                return BadRequest(new { message = "Gelişim raporu oluşturulabilmesi için kayıtlı deneme bulunamadı." });
            }

            var firstExam = recentExams.First(); // Analize giren en eski deneme
            var lastExam = recentExams.Last();   // En güncel deneme

            // Ders bazlı ortalama net ve gelişim hesaplama fonksiyonu
            Func<string, Func<ExamResult, double>, SubjectProgressDto> buildSubjectProgress = (subjectName, selector) =>
            {
                double firstNet = selector(firstExam);
                double lastNet = selector(lastExam);
                double avgNet = Math.Round(recentExams.Average(selector), 2);
                double diff = Math.Round(lastNet - firstNet, 2);
                string status = diff > 0.5 ? "Yükselişte" : (diff < -0.5 ? "Düşüşte" : "Stabil");

                return new SubjectProgressDto
                {
                    SubjectName = subjectName,
                    AverageNet = avgNet,
                    OldestExamNet = firstNet,
                    LatestExamNet = lastNet,
                    NetChange = diff,
                    StatusTrend = status
                };
            };

            var subjectProgresses = new List<SubjectProgressDto>
            {
                buildSubjectProgress("Türkçe", e => e.TurkishNet),
                buildSubjectProgress("Matematik", e => e.MathNet),
                buildSubjectProgress("Fen Bilimleri", e => e.ScienceNet),
                buildSubjectProgress("Sosyal Bilgiler", e => e.SocialNet)
            };

            // Son 5 denemedeki en çok tekrarlanan ilk 5 konu hatası
            var criticalWeakTopics = recentExams
                .SelectMany(e => e.TopicErrors)
                .GroupBy(t => new { t.Subject, t.TopicName })
                .Select(g => new TopWeakTopicDto
                {
                    Subject = g.Key.Subject,
                    TopicName = g.Key.TopicName,
                    TotalErrors = g.Sum(x => x.IncorrectCount + x.BlankCount)
                })
                .OrderByDescending(x => x.TotalErrors)
                .Take(5)
                .ToList();

            var examSummaries = recentExams.Select(e => new ExamSummaryDto
            {
                ExamId = e.Id,
                ExamType = e.ExamType,
                ExamDate = e.ExamDate,
                TotalNet = Math.Round(e.TotalNet, 2)
            }).ToList();

            var report = new StudentProgressReportDto
            {
                StudentId = student!.Id,
                StudentName = student.Name,
                ExamCountAnalyzed = recentExams.Count,
                PeriodStartDate = firstExam.ExamDate,
                PeriodEndDate = lastExam.ExamDate,
                SubjectProgresses = subjectProgresses,
                CriticalWeakTopics = criticalWeakTopics,
                RecentExamsSummary = examSummaries
            };

            return Ok(report);
        }

        // 3. Konu Hataları Detaylı Deneme Sonucu Ekleme
        [HttpPost("exam-result-detailed/{studentId}")]
        public async Task<IActionResult> AddDetailedExamResult(int studentId, [FromBody] ExamResultDetailedCreateDto dto)
        {
            var studentExists = await _repository.StudentExistsAsync(studentId);
            if (!studentExists)
            {
                return NotFound(new { message = $"ID'si {studentId} olan öğrenci bulunamadı." });
            }

            var examResult = new ExamResult
            {
                StudentId = studentId,
                ExamType = dto.ExamType,
                ExamDate = dto.ExamDate,
                TurkishNet = dto.TurkishNet,
                MathNet = dto.MathNet,
                ScienceNet = dto.ScienceNet,
                SocialNet = dto.SocialNet,
                TimeManagementIssue = dto.TimeManagementIssue
            };

            var topicErrors = dto.TopicErrors.Select(t => new ExamTopicError
            {
                Subject = t.Subject,
                TopicName = t.TopicName,
                IncorrectCount = t.IncorrectCount,
                BlankCount = t.BlankCount
            }).ToList();

            await _repository.AddExamResultWithTopicsAsync(examResult, topicErrors);

            return Ok(new
            {
                message = "Konu detaylı deneme sonucu başarıyla kaydedildi.",
                examId = examResult.Id,
                totalNet = examResult.TotalNet
            });
        }

        // 4. Standart Deneme Ekleme (Konu Detaysız)
        [HttpPost("exam-result/{studentId}")]
        public async Task<IActionResult> AddExamResult(int studentId, [FromBody] ExamResultCreateDto dto)
        {
            var studentExists = await _repository.StudentExistsAsync(studentId);
            if (!studentExists)
            {
                return NotFound(new { message = $"ID'si {studentId} olan öğrenci bulunamadı." });
            }

            var examResult = new ExamResult
            {
                StudentId = studentId,
                ExamType = dto.ExamType,
                ExamDate = dto.ExamDate,
                TurkishNet = dto.TurkishNet,
                MathNet = dto.MathNet,
                ScienceNet = dto.ScienceNet,
                SocialNet = dto.SocialNet,
                TimeManagementIssue = dto.TimeManagementIssue
            };

            await _repository.AddExamResultAsync(examResult);
            await _repository.SaveChangesAsync();

            return Ok(new
            {
                message = "Deneme sonucu başarıyla kaydedildi.",
                examId = examResult.Id,
                totalNet = examResult.TotalNet
            });
        }

        // 5. Öğrenci Koçluk Geçmişi
        [HttpGet("history/{studentId}")]
        public async Task<IActionResult> GetStudentHistory(int studentId)
        {
            var studentExists = await _repository.StudentExistsAsync(studentId);
            if (!studentExists)
            {
                return NotFound(new { message = $"ID'si {studentId} olan öğrenci bulunamadı." });
            }

            var history = await _repository.GetStudentHistoryAsync(studentId);

            var response = history.Select(f => new
            {
                f.Id,
                f.Date,
                f.StudentNote,
                f.GeneratedAdvice,
                f.DetectedSentiment
            });

            return Ok(response);
        }
    }
}