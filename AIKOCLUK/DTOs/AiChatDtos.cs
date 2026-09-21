using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AIKOCLUK.DTOs
{
    public class ChatMessageDto
    {
        public string Sender { get; set; } = string.Empty; // "User" veya "Ai"
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ChatRequestDto
    {
        public int StudentId { get; set; }
        public string UserMessage { get; set; } = string.Empty;
        public List<ChatMessageDto> ConversationHistory { get; set; } = new();
    }

    public class ChatResponseDto
    {
        public string AiReply { get; set; } = string.Empty;
        public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
    }

    public class AdviceHistoryDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string AdviceType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string GenelDegerlendirme { get; set; } = string.Empty;
        public string HaftalikOdakTavsiyesi { get; set; } = string.Empty;
        public string KirmiziAlarmDersleri { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}