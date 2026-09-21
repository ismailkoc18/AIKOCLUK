using AIKOCLUK.Data;
using AIKOCLUK.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AIKOCLUK.Services
{
    public class GamificationService
    {
        private readonly AppDbContext _context;

        public GamificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GamificationOverviewDto> GetStudentGamificationOverviewAsync(int studentId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new KeyNotFoundException($"{studentId} ID'li öğrenci bulunamadı.");

            var examDates = await _context.ExamResults
                .Where(e => e.StudentId == studentId)
                .Select(e => e.ExamDate.Date)
                .ToListAsync();

            var adviceDates = await _context.AiAdviceHistories
                .Where(a => a.StudentId == studentId)
                .Select(a => a.CreatedAt.Date)
                .ToListAsync();

            var allActivityDates = examDates
                .Concat(adviceDates)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            var streak = CalculateStreak(allActivityDates);
            var examCount = examDates.Count;
            var adviceCount = adviceDates.Count;

            var badges = EvaluateBadges(examCount, adviceCount, streak.CurrentStreakDays);

            int totalXp = (examCount * 100) + (adviceCount * 50) + (streak.CurrentStreakDays * 20);
            int level = (totalXp / 500) + 1;

            return new GamificationOverviewDto
            {
                StudentId = studentId,
                TotalXp = totalXp,
                Level = level,
                Streak = streak,
                Badges = badges
            };
        }

        private StudentStreakDto CalculateStreak(List<DateTime> sortedDates)
        {
            if (!sortedDates.Any())
            {
                return new StudentStreakDto();
            }

            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            int currentStreak = 0;
            int longestStreak = 0;
            int tempStreak = 0;

            DateTime? lastDate = null;

            foreach (var date in sortedDates)
            {
                if (lastDate == null)
                {
                    tempStreak = 1;
                }
                else if ((lastDate.Value - date).Days == 1)
                {
                    tempStreak++;
                }
                else if ((lastDate.Value - date).Days > 1)
                {
                    if (tempStreak > longestStreak) longestStreak = tempStreak;
                    tempStreak = 1;
                }

                lastDate = date;
            }

            if (tempStreak > longestStreak) longestStreak = tempStreak;

            var mostRecent = sortedDates.First();
            if (mostRecent == today || mostRecent == yesterday)
            {
                currentStreak = 1;
                for (int i = 0; i < sortedDates.Count - 1; i++)
                {
                    if ((sortedDates[i] - sortedDates[i + 1]).Days == 1)
                        currentStreak++;
                    else
                        break;
                }
            }

            return new StudentStreakDto
            {
                CurrentStreakDays = currentStreak,
                LongestStreakDays = Math.Max(longestStreak, currentStreak),
                LastActivityDate = sortedDates.FirstOrDefault(),
                TotalActiveDays = sortedDates.Count
            };
        }

        private List<BadgeDto> EvaluateBadges(int examCount, int adviceCount, int currentStreak)
        {
            return new List<BadgeDto>
            {
                new BadgeDto
                {
                    Code = "FIRST_EXAM",
                    Title = "İlk Adım",
                    Description = "Sisteme ilk deneme sonucunu kaydettin.",
                    Icon = "🎯",
                    IsUnlocked = examCount >= 1
                },
                new BadgeDto
                {
                    Code = "EXAM_MASTER",
                    Title = "Deneme Canavarı",
                    Description = "Toplam 5 deneme sınavı tamamladın.",
                    Icon = "📝",
                    IsUnlocked = examCount >= 5
                },
                new BadgeDto
                {
                    Code = "STREAK_3",
                    Title = "İstikrar Yolcusu",
                    Description = "3 gün üst üste aktif oldun.",
                    Icon = "🔥",
                    IsUnlocked = currentStreak >= 3
                },
                new BadgeDto
                {
                    Code = "STREAK_7",
                    Title = "Alev Aldı",
                    Description = "7 günlük kesintisiz çalışma serisine ulaştın.",
                    Icon = "⚡",
                    IsUnlocked = currentStreak >= 7
                },
                new BadgeDto
                {
                    Code = "AI_FRIEND",
                    Title = "AI Koç Dostu",
                    Description = "Yapay zeka koçundan en az 3 kez tavsiye aldın.",
                    Icon = "🤖",
                    IsUnlocked = adviceCount >= 3
                }
            };
        }
    }
}