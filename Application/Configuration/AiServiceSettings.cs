namespace Application.Configurations
{
    public class AiServiceSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:8001";
        public int TimeoutSeconds { get; set; } = 60;
        public int RetryCount { get; set; } = 2;
        public int RetryBaseDelayMs { get; set; } = 500;
        public int QuizTimeoutSeconds { get; set; } = 45;
        public int ChatTimeoutSeconds { get; set; } = 30;
        public int SummaryTimeoutSeconds { get; set; } = 30;
        public int HealthTimeoutSeconds { get; set; } = 10;
    }
}
