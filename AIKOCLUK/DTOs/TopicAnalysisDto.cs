using System.Collections.Generic;

namespace AIKOCLUK.DTOs
{
    public class SubjectTopicTreeDto
    {
        public string SubjectName { get; set; } = string.Empty; // Örn: Matematik
        public int TotalErrorsInSubject { get; set; }
        public List<TopicPerformanceDto> Topics { get; set; } = new();
    }

    public class TopicPerformanceDto
    {
        public string TopicName { get; set; } = string.Empty; // Örn: Türev
        public int TotalIncorrect { get; set; }
        public int TotalBlank { get; set; }
        public int TotalErrors => TotalIncorrect + TotalBlank;

        // "Kritik", "GelisimAlani", "Kuvvetli"
        public string StatusLevel { get; set; } = "Kuvvetli";
    }
}