namespace AIKOCLUK.Models
{
    public class TopicErrorDto
    {
        public string Subject { get; set; } = string.Empty; // Örn: Matematik, Fen
        public string TopicName { get; set; } = string.Empty; // Örn: Trigonometri, Optik
        public int IncorrectCount { get; set; }
        public int BlankCount { get; set; }
    }

    public class ExamResultDetailedCreateDto : ExamResultCreateDto
    {
        public List<TopicErrorDto> TopicErrors { get; set; } = new();
    }
}