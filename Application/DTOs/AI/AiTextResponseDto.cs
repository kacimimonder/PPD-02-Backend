namespace Application.DTOs.AI
{
    public class AiTextResponseDto
    {
        public string Output { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string? ConversationId { get; set; }
        public int? QuizId { get; set; }

        /// <summary>
        /// Server-side processing time in milliseconds (including AI call)
        /// </summary>
        public long? DurationMs { get; set; }

        /// <summary>
        /// Indicates if this is a fallback response (AI was unavailable)
        /// </summary>
        public bool IsFallback { get; set; } = false;

        /// <summary>
        /// User-friendly status hint: "success", "fallback", "partial"
        /// </summary>
        public string Status { get; set; } = "success";

        public string? Sentiment { get; set; }
        public string? Emotion { get; set; }
        public bool AdaptationApplied { get; set; }
    }
}
