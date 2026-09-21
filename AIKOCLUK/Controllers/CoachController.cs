using AIKOCLUK.Data; // AppDbContext için
using AIKOCLUK.Models;
using AIKOCLUK.Repositories;
using AIKOCLUK.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AIKOCLUK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoachController : ControllerBase
    {
        private readonly ICoachRepository _repository;
        private readonly IAiCoachService _aiCoachService; // Interface üzerinden çağırmak en iyisidir
        private readonly AppDbContext _context; // Veritabanı kayıtları için

        public CoachController(ICoachRepository repository, IAiCoachService aiCoachService, AppDbContext context)
        {
            _repository = repository;
            _aiCoachService = aiCoachService;
            _context = context;
        }

        // 1. Son 5 Deneme & Kronikleşen Konu Hatalarına Dayalı AI Analizi ve Veritabanına Kayıt
        [HttpPost("analyze/{studentId}")]
        [EnableRateLimiting("ai-analyze")]
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

            // Yapay zekadan yapılandırılmış JSON sonucunu (AiExamAnalysisResult) alıyoruz
            AiExamAnalysisResult analysisResult = await _aiCoachService.GeneratePersonalizedAdviceAsync(
                student,
                recentExams,
                request?.StudentNote);

            // Yeni tablomuz için geçmiş koçluk nesnesini oluşturuyoruz
            var adviceHistory = new AiAdviceHistory
            {
                StudentId = student.Id,
                CreatedAt = DateTime.UtcNow,
                GenelDegerlendirme = analysisResult.GenelDegerlendirme,
                HaftalikOdakTavsiyesi = analysisResult.HaftalikOdakTavsiyesi,
                // Listeyi SQLite'ta saklayabilmek için virgülle birleştiriyoruz
                KirmiziAlarmDersleri = analysisResult.KirmiziAlarm != null ? string.Join(", ", analysisResult.KirmiziAlarm) : string.Empty
            };

            // Yapay zekanın önerdiği hedef konuları alt tabloya ekliyoruz
            if (analysisResult.HedefKonular != null)
            {
                foreach (var target in analysisResult.HedefKonular)
                {
                    adviceHistory.HedefKonular.Add(new AiTargetSubject
                    {
                        Ders = target.Ders,
                        Konu = target.Konu,
                        Neden = target.Neden,
                        Taktik = target.Taktik
                    });
                }
            }

            // Veritabanına kaydediyoruz
            _context.AiAdviceHistories.Add(adviceHistory);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                StudentName = student.Name,
                Target = $"{student.TargetUniversity} - {student.TargetDepartment}",
                Analysis = analysisResult,
                CreatedAt = adviceHistory.CreatedAt
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

            var firstExam = recentExams.First();
            var lastExam = recentExams.Last();

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

        // 5. Öğrenci Koçluk Geçmişi (Yeni veritabanı tablosundan çekiyoruz)
        [HttpGet("history/{studentId}")]
        public async Task<IActionResult> GetStudentHistory(int studentId)
        {
            var studentExists = await _repository.StudentExistsAsync(studentId);
            if (!studentExists)
            {
                return NotFound(new { message = $"ID'si {studentId} olan öğrenci bulunamadı." });
            }

            // Yeni AiAdviceHistories tablosundan öğrenciye ait geçmişi ve alt hedef konuları çekiyoruz
            var history = await _context.AiAdviceHistories
                .Include(h => h.HedefKonular)
                .Where(h => h.StudentId == studentId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            return Ok(history);
        }
    }
}