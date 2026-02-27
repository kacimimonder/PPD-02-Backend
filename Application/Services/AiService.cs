using System.Text;
using System.Text.Json;
using System.Diagnostics;
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
        private readonly AiMonitoringService _monitoringService;
        public AiService(HttpClient httpClient, ILogger<AiService> logger, AiMonitoringService monitoringService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _monitoringService = monitoringService;
        }

        public Task<AiTextResponseDto> ChatAsync(AiChatRequestDto request, CancellationToken cancellationToken = default)
            => PostAsync<AiChatRequestDto>("chat", request, cancellationToken);

        public Task<AiTextResponseDto> SummarizeAsync(AiSummaryRequestDto request, CancellationToken cancellationToken = default)
            => PostAsync<AiSummaryRequestDto>("summary", request, cancellationToken);

        public Task<AiTextResponseDto> GenerateQuizAsync(AiQuizRequestDto request, CancellationToken cancellationToken = default)
            => PostAsync<AiQuizRequestDto>("quiz", request, cancellationToken);

        public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                using var response = await _httpClient.GetAsync("health", cancellationToken);
                stopwatch.Stop();
                _monitoringService.Record("health", response.IsSuccessStatusCode, stopwatch.ElapsedMilliseconds);

                _logger.LogInformation(
                    "AI health check finished with status {StatusCode} in {DurationMs}ms",
                    (int)response.StatusCode,
                    stopwatch.ElapsedMilliseconds);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _monitoringService.Record("health", false, stopwatch.ElapsedMilliseconds);

                _logger.LogError(ex, "AI health check failed after {DurationMs}ms", stopwatch.ElapsedMilliseconds);
                return false;
            }
        }

        private async Task<AiTextResponseDto> PostAsync<TRequest>(string endpoint, TRequest request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(
                "AI request started. Endpoint={Endpoint}, RequestType={RequestType}",
                endpoint,
                typeof(TRequest).Name);

            var json = JsonSerializer.Serialize(request, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                stopwatch.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    _monitoringService.Record(endpoint, false, stopwatch.ElapsedMilliseconds);
                    _logger.LogWarning(
                        "AI request failed. Endpoint={Endpoint}, Status={StatusCode}, DurationMs={DurationMs}, Body={Body}",
                        endpoint,
                        (int)response.StatusCode,
                        stopwatch.ElapsedMilliseconds,
                        body);
                    throw new InvalidOperationException($"AI service error ({(int)response.StatusCode}).");
                }

                var parsed = JsonSerializer.Deserialize<AiTextResponseDto>(body, JsonOptions);
                if (parsed == null)
                {
                    _monitoringService.Record(endpoint, false, stopwatch.ElapsedMilliseconds);
                    throw new InvalidOperationException("AI service returned an empty/invalid response.");
                }

                _monitoringService.Record(endpoint, true, stopwatch.ElapsedMilliseconds);
                _logger.LogInformation(
                    "AI request succeeded. Endpoint={Endpoint}, Status={StatusCode}, DurationMs={DurationMs}, Provider={Provider}, Model={Model}",
                    endpoint,
                    (int)response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    parsed.Provider,
                    parsed.Model);

                return parsed;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _monitoringService.Record(endpoint, false, stopwatch.ElapsedMilliseconds);
                _logger.LogError(
                    ex,
                    "AI request threw unexpected exception. Endpoint={Endpoint}, DurationMs={DurationMs}",
                    endpoint,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public Dictionary<string, AiMonitoringService.AiMonitoringSnapshot> GetMonitoringSnapshot()
            => _monitoringService.GetSnapshot();
    }
}
