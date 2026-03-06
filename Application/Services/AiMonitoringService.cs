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
            public long MinDurationMs { get; set; }
            public long MaxDurationMs { get; set; }
            public long P95DurationMs { get; set; }
            public long P99DurationMs { get; set; }
            public double RequestsPerMinute { get; set; }
        }

        private sealed class EndpointStats
        {
            private readonly object _lock = new();

            private long _totalCalls;
            private long _successCalls;
            private long _failedCalls;
            private long _lastDurationMs;
            private long _totalDurationMs;
            private long _minDurationMs = long.MaxValue;
            private long _maxDurationMs;
            private readonly List<long> _recentDurations = new(1000);
            private DateTime _firstCallUtc = DateTime.UtcNow;

            public void Record(bool success, long durationMs)
            {
                lock (_lock)
                {
                    if (_totalCalls == 0)
                        _firstCallUtc = DateTime.UtcNow;

                    _totalCalls++;
                    _lastDurationMs = durationMs;
                    _totalDurationMs += durationMs;

                    if (durationMs < _minDurationMs) _minDurationMs = durationMs;
                    if (durationMs > _maxDurationMs) _maxDurationMs = durationMs;

                    // Keep last 1000 durations for percentile calculation
                    if (_recentDurations.Count >= 1000)
                        _recentDurations.RemoveAt(0);
                    _recentDurations.Add(durationMs);

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

                    var elapsed = DateTime.UtcNow - _firstCallUtc;
                    var requestsPerMinute = elapsed.TotalMinutes > 0
                        ? _totalCalls / elapsed.TotalMinutes
                        : _totalCalls;

                    long p95 = 0, p99 = 0;
                    if (_recentDurations.Count > 0)
                    {
                        var sorted = _recentDurations.OrderBy(d => d).ToList();
                        p95 = sorted[(int)(sorted.Count * 0.95)];
                        p99 = sorted[(int)(sorted.Count * 0.99)];
                    }

                    return new AiMonitoringSnapshot
                    {
                        TotalCalls = _totalCalls,
                        SuccessCalls = _successCalls,
                        FailedCalls = _failedCalls,
                        ErrorRatePercent = Math.Round(errorRatePercent, 2),
                        LastDurationMs = _lastDurationMs,
                        AverageDurationMs = Math.Round(averageDurationMs, 2),
                        MinDurationMs = _totalCalls == 0 ? 0 : _minDurationMs,
                        MaxDurationMs = _maxDurationMs,
                        P95DurationMs = p95,
                        P99DurationMs = p99,
                        RequestsPerMinute = Math.Round(requestsPerMinute, 2)
                    };
                }
            }
        }
    }
}
