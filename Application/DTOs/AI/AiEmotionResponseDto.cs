namespace Application.DTOs.AI
{
    public class AiEmotionResponseDto
    {
        public string Emotion { get; set; } = "neutral";
        public double Confidence { get; set; }
        public string Rationale { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }
}
