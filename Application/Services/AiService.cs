using System.Text;
using System.Text.Json;
using System.Diagnostics;
using Application.Configurations;
using Application.DTOs.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        private readonly AiServiceSettings _settings;

        public AiService(HttpClient httpClient, ILogger<AiService> logger,
            AiMonitoringService monitoringService, IOptions<AiServiceSettings> settings)
        {
            _httpClient = httpClient;
            _logger = logger;
            _monitoringService = monitoringService;
            _settings = settings.Value;
        }

        public Task<AiTextResponseDto> ChatAsync(AiChatRequestDto request, CancellationToken cancellationToken = default)
            => PostAsync<AiChatRequestDto>("chat", request, _settings.ChatTimeoutSeconds, cancellationToken);

        public Task<AiTextResponseDto> SummarizeAsync(AiSummaryRequestDto request, CancellationToken cancellationToken = default)
            => PostAsync<AiSummaryRequestDto>("summary", request, _settings.SummaryTimeoutSeconds, cancellationToken);

        public Task<AiTextResponseDto> GenerateQuizAsync(AiQuizRequestDto request, CancellationToken cancellationToken = default)
            => PostAsync<AiQuizRequestDto>("quiz", request, _settings.QuizTimeoutSeconds, cancellationToken);

        public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(_settings.HealthTimeoutSeconds));

                using var response = await _httpClient.GetAsync("health", cts.Token);
                stopwatch.Stop();
                _monitoringService.Record("health", response.IsSuccessStatusCode, stopwatch.ElapsedMilliseconds);

                _logger.LogInformation(
                    "AI health check finished with status {StatusCode} in {DurationMs}ms",
                    (int)response.StatusCode,
                    stopwatch.ElapsedMilliseconds);

                return response.IsSuccessStatusCode;
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                _monitoringService.Record("health", false, stopwatch.ElapsedMilliseconds);
                _logger.LogWarning("AI health check timed out after {DurationMs}ms", stopwatch.ElapsedMilliseconds);
                return false;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _monitoringService.Record("health", false, stopwatch.ElapsedMilliseconds);
                _logger.LogError(ex, "AI health check failed after {DurationMs}ms", stopwatch.ElapsedMilliseconds);
                return false;
            }
        }

        private async Task<AiTextResponseDto> PostAsync<TRequest>(
            string endpoint, TRequest request, int timeoutSeconds, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(
                "AI request started. Endpoint={Endpoint}, RequestType={RequestType}, TimeoutSec={Timeout}",
                endpoint,
                typeof(TRequest).Name,
                timeoutSeconds);

            var json = JsonSerializer.Serialize(request, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

                using var response = await _httpClient.PostAsync(endpoint, content, cts.Token);
                var body = await response.Content.ReadAsStringAsync(cts.Token);
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

                    var friendlyMessage = MapStatusToFriendlyMessage((int)response.StatusCode, endpoint);
                    throw new InvalidOperationException(friendlyMessage);
                }

                var parsed = JsonSerializer.Deserialize<AiTextResponseDto>(body, JsonOptions);
                if (parsed == null)
                {
                    _monitoringService.Record(endpoint, false, stopwatch.ElapsedMilliseconds);
                    throw new InvalidOperationException(
                        "We received an empty response from the AI service. Please try again.");
                }

                parsed.DurationMs = stopwatch.ElapsedMilliseconds;
                parsed.Status = "success";

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
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();
                _monitoringService.Record(endpoint, false, stopwatch.ElapsedMilliseconds);
                _logger.LogWarning(
                    "AI request timed out. Endpoint={Endpoint}, DurationMs={DurationMs}, TimeoutSec={Timeout}",
                    endpoint, stopwatch.ElapsedMilliseconds, timeoutSeconds);
                throw new InvalidOperationException(
                    $"The AI service is taking longer than expected. Please try again with a simpler request or try later.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                _monitoringService.Record(endpoint, false, stopwatch.ElapsedMilliseconds);
                _logger.LogError(ex,
                    "AI service connection failed. Endpoint={Endpoint}, DurationMs={DurationMs}",
                    endpoint, stopwatch.ElapsedMilliseconds);
                throw new InvalidOperationException(
                    "Unable to reach the AI service. Please try again in a moment.");
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

        private static string MapStatusToFriendlyMessage(int statusCode, string endpoint)
        {
            return statusCode switch
            {
                429 => "The AI service is currently handling too many requests. Please wait a moment and try again.",
                503 => "The AI service is temporarily unavailable for maintenance. Please try again shortly.",
                504 => "The AI service timed out while processing your request. Try a simpler question or shorter content.",
                >= 500 => "The AI service encountered an internal error. Our team has been notified. Please try again.",
                422 => "The request content could not be processed by the AI. Please rephrase your input.",
                _ => $"The AI service returned an unexpected error (code {statusCode}). Please try again."
            };
        }

        public Dictionary<string, AiMonitoringService.AiMonitoringSnapshot> GetMonitoringSnapshot()
            => _monitoringService.GetSnapshot();
    }
}
