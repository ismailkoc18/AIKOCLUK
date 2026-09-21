using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIKOCLUK.Services
{
    public interface IGeminiService
    {
        Task<string> GetDailyBriefingAsync(string studentName, int solvedQuestions, int targetQuestions);
        Task<string> SendCoachChatMessageAsync(List<ChatMessageDto> history, string userMessage, string persona);
    }

    public class ChatMessageDto
    {
        public string Sender { get; set; } = string.Empty; // "user" veya "ai"
        public string Text { get; set; } = string.Empty;
    }
}