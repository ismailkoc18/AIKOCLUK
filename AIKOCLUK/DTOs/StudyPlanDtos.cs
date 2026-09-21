using System.Text.Json.Serialization;

namespace AIKOCLUK.DTOs
{
    public class StudyBlockDto
    {
        [JsonPropertyName("timeSlot")]
        public string TimeSlot { get; set; } = string.Empty; // Örn: "09:00 - 10:30"

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;  // Örn: "Matematik"

        [JsonPropertyName("topic")]
        public string Topic { get; set; } = string.Empty;    // Örn: "Türev"

        [JsonPropertyName("activityType")]
        public string ActivityType { get; set; } = string.Empty; // Örn: "Soru Çözümü (60 Soru)"

        [JsonPropertyName("adviceNote")]
        public string AdviceNote { get; set; } = string.Empty; // Örn: "Formül kağıdını yanına al."
    }

    public class DailyScheduleDto
    {
        [JsonPropertyName("day")]
        public string Day { get; set; } = string.Empty; // Örn: "Pazartesi"

        [JsonPropertyName("totalTargetHours")]
        public int TotalTargetHours { get; set; }

        [JsonPropertyName("blocks")]
        public List<StudyBlockDto> Blocks { get; set; } = new();
    }

    public class WeeklyStudyPlanResponseDto
    {
        [JsonPropertyName("studentId")]
        public int StudentId { get; set; }

        [JsonPropertyName("targetSummary")]
        public string TargetSummary { get; set; } = string.Empty;

        [JsonPropertyName("generatedAt")]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("weeklySchedule")]
        public List<DailyScheduleDto> WeeklySchedule { get; set; } = new();
    }
}