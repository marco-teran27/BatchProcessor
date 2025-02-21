// Path: Core/Logic/Timeout/TimeoutManager.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BatchProcessor.Core.Logic.Timeout;

public class TimeoutManager
{
    private readonly ConcurrentDictionary<string, TimeoutInfo> _timeoutHistory = new();
    private readonly object _timeoutLock = new();
    private readonly int _defaultTimeoutMinutes;
    private readonly int _requiredSuccessCount;
    private readonly double _timeoutBuffer;

    public TimeoutManager(
        int defaultTimeoutMinutes = 8,
        int requiredSuccessCount = 3,
        double timeoutBuffer = 1.5)
    {
        _defaultTimeoutMinutes = defaultTimeoutMinutes;
        _requiredSuccessCount = requiredSuccessCount;
        _timeoutBuffer = timeoutBuffer;
    }

    public TimeSpan CalculateTimeout(string operationKey)
    {
        lock (_timeoutLock)
        {
            var history = GetRecentHistory(operationKey);
            
            if (history.Count < _requiredSuccessCount)
            {
                return TimeSpan.FromMinutes(_defaultTimeoutMinutes);
            }

            var avgTime = history.Average(h => h.ProcessingTime.TotalSeconds);
            return TimeSpan.FromSeconds(avgTime * _timeoutBuffer);
        }
    }

    public void UpdateHistory(string operationKey, TimeSpan processingTime)
    {
        lock (_timeoutLock)
        {
            var info = new TimeoutInfo
            {
                ProcessingTime = processingTime,
                Timestamp = DateTime.Now
            };

            _timeoutHistory.AddOrUpdate(operationKey, info, (_, _) => info);
            CleanupOldEntries();
        }
    }

    private List<TimeoutInfo> GetRecentHistory(string operationKey)
    {
        return _timeoutHistory.Values
            .OrderByDescending(t => t.Timestamp)
            .Take(5)
            .ToList();
    }

    private void CleanupOldEntries()
    {
        var oldEntries = _timeoutHistory
            .Where(kvp => (DateTime.Now - kvp.Value.Timestamp).TotalHours > 24)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in oldEntries)
        {
            _timeoutHistory.TryRemove(key, out _);
        }
    }

    private class TimeoutInfo
    {
        public TimeSpan ProcessingTime { get; set; }
        public DateTime Timestamp { get; set; }
    }
}