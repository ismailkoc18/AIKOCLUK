namespace AIKOCLUK.Models
{
    public class ExamTopicError
    {
        public int Id { get; set; }
        public int ExamResultId { get; set; }
        public ExamResult? ExamResult { get; set; }
        public string Subject { get; set; } = string.Empty; // Matematik, Türkçe, Fen, Sosyal
        public string TopicName { get; set; } = string.Empty; // Trigonometri, Optik, Paragraf
        public int IncorrectCount { get; set; }
        public int BlankCount { get; set; }
    }
}