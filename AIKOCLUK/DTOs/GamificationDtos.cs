using System;
using System.Collections.Generic;

namespace AIKOCLUK.DTOs
{
    public class StudentStreakDto
    {
        public int CurrentStreakDays { get; set; }
        public int LongestStreakDays { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public int TotalActiveDays { get; set; }
    }

    public class BadgeDto
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsUnlocked { get; set; }
    }

    public class GamificationOverviewDto
    {
        public int StudentId { get; set; }
        public int TotalXp { get; set; }
        public int Level { get; set; }
        public StudentStreakDto Streak { get; set; } = new();
        public List<BadgeDto> Badges { get; set; } = new();
    }
}