using System.Text;
using System.Text.Json;
using Application.DTOs.AI;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class AiService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly HttpClient _httpClient;
        private readonly ILogger<AiService> _logger;
        public AiService(HttpClient httpClient, ILogger<AiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public Task<AiTextResponseDto> ChatAsync(AiChatRequestDto request, CancellationToken cancellationToken = default)
            => PostAsync<AiChatRequestDto>("chat", request, cancellationToken);

        public Task<AiTextResponseDto> SummarizeAsync(AiSummaryRequestDto request, CancellationToken cancellationToken = default)
            => PostAsync<AiSummaryRequestDto>("summary", request, cancellationToken);

        public Task<AiTextResponseDto> GenerateQuizAsync(AiQuizRequestDto request, CancellationToken cancellationToken = default)
            => PostAsync<AiQuizRequestDto>("quiz", request, cancellationToken);

        public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await _httpClient.GetAsync("health", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI health check failed.");
                return false;
            }
        }

        private async Task<AiTextResponseDto> PostAsync<TRequest>(string endpoint, TRequest request, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(request, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AI endpoint {Endpoint} returned status {Status}: {Body}", endpoint, (int)response.StatusCode, body);
                throw new InvalidOperationException($"AI service error ({(int)response.StatusCode}).");
            }

            var parsed = JsonSerializer.Deserialize<AiTextResponseDto>(body, JsonOptions);
            if (parsed == null)
            {
                throw new InvalidOperationException("AI service returned an empty/invalid response.");
            }

            return parsed;
        }
    }
}
