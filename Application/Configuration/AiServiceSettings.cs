namespace Application.Configurations
{
    public class AiServiceSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:8001";
        public int TimeoutSeconds { get; set; } = 60;
    }
}
