using System.Collections.Concurrent;

namespace Application.Services
{
    public class AiMonitoringService
    {
        private readonly ConcurrentDictionary<string, EndpointStats> _stats = new(StringComparer.OrdinalIgnoreCase);

        public void Record(string endpoint, bool success, long durationMs)
        {
            var stats = _stats.GetOrAdd(endpoint, _ => new EndpointStats());
            stats.Record(success, durationMs);
        }

        public Dictionary<string, AiMonitoringSnapshot> GetSnapshot()
        {
            return _stats.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToSnapshot(),
                StringComparer.OrdinalIgnoreCase);
        }

        public class AiMonitoringSnapshot
        {
            public long TotalCalls { get; set; }
            public long SuccessCalls { get; set; }
            public long FailedCalls { get; set; }
            public double ErrorRatePercent { get; set; }
            public long LastDurationMs { get; set; }
            public double AverageDurationMs { get; set; }
        }

        private sealed class EndpointStats
        {
            private readonly object _lock = new();

            private long _totalCalls;
            private long _successCalls;
            private long _failedCalls;
            private long _lastDurationMs;
            private long _totalDurationMs;

            public void Record(bool success, long durationMs)
            {
                lock (_lock)
                {
                    _totalCalls++;
                    _lastDurationMs = durationMs;
                    _totalDurationMs += durationMs;

                    if (success)
                    {
                        _successCalls++;
                    }
                    else
                    {
                        _failedCalls++;
                    }
                }
            }

            public AiMonitoringSnapshot ToSnapshot()
            {
                lock (_lock)
                {
                    var errorRatePercent = _totalCalls == 0
                        ? 0
                        : (_failedCalls * 100.0) / _totalCalls;

                    var averageDurationMs = _totalCalls == 0
                        ? 0
                        : _totalDurationMs / (double)_totalCalls;

                    return new AiMonitoringSnapshot
                    {
                        TotalCalls = _totalCalls,
                        SuccessCalls = _successCalls,
                        FailedCalls = _failedCalls,
                        ErrorRatePercent = Math.Round(errorRatePercent, 2),
                        LastDurationMs = _lastDurationMs,
                        AverageDurationMs = Math.Round(averageDurationMs, 2)
                    };
                }
            }
        }
    }
}
