namespace AIKOCLUK.DTOs
{
    public class AiAskRequestDto
    {
        public int StudentId { get; set; }
        public string Question { get; set; } = string.Empty;
    }

    public class AiAskResponseDto
    {
        public string Answer { get; set; } = string.Empty;
        public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
    }

    public class AiAdviceHistoryResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string AdviceType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}