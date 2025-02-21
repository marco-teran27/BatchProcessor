using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using BatchProcessor.DI.Interfaces.AbsRhino;
using Microsoft.Extensions.Logging;

/*
File: BatchProcessor\Core\Services\BatchRetry.cs
Summary: Manages retry logic for batch processing operations using async/await.
         Provides retry attempts with configurable policies.
*/

namespace BatchProcessor.Core.Services
{
    /// <summary>
    /// Manages retry logic for batch processing operations.
    /// </summary>
    public class BatchRetry
    {
        private readonly ILogger<BatchRetry> _logger;
        private readonly ICommLineOut _output;
        private readonly ConcurrentDictionary<string, RetryState> _retryStates;
        private readonly int _maxRetries;
        private readonly TimeSpan _baseDelay;
        private readonly TimeSpan _maxDelay;

        /// <summary>
        /// Initializes a new instance of BatchRetry.
        /// </summary>
        public BatchRetry(ILogger<BatchRetry> logger, ICommLineOut output, int maxRetries = 3, int baseDelaySeconds = 5, int maxDelaySeconds = 30)
        {
            _logger = logger;
            _output = output;
            _retryStates = new ConcurrentDictionary<string, RetryState>();
            _maxRetries = maxRetries;
            _baseDelay = TimeSpan.FromSeconds(baseDelaySeconds);
            _maxDelay = TimeSpan.FromSeconds(maxDelaySeconds);
        }

        /// <summary>
        /// Executes an operation with retry logic asynchronously.
        /// </summary>
        public async Task<(bool success, string details)> ExecuteWithRetry(
            string fileName,
            Func<Task<(bool success, string details)>> operation,
            RetryPolicy policy = RetryPolicy.Exponential)
        {
            var state = _retryStates.GetOrAdd(fileName, _ => new RetryState());

            while (state.RetryCount < _maxRetries)
            {
                try
                {
                    var result = await operation();
                    if (result.success)
                    {
                        _retryStates.TryRemove(fileName, out _);
                        return result;
                    }

                    var delay = CalculateDelay(state.RetryCount, policy);
                    state.RetryCount++;
                    _logger.LogWarning($"Retry {state.RetryCount} for {fileName} in {delay.TotalSeconds} seconds");
                    await Task.Delay(delay);
                }
                catch (Exception ex)
                {
                    state.LastError = ex;
                    state.RetryCount++;

                    _logger.LogError(ex, $"Error during retry {state.RetryCount} for {fileName}");
                    if (state.RetryCount >= _maxRetries)
                    {
                        break;
                    }
                }
            }

            var finalError = state.LastError != null
                ? $"Failed after {state.RetryCount} retries: {state.LastError.Message}"
                : $"Failed after {state.RetryCount} retries";
            _retryStates.TryRemove(fileName, out _);
            return (false, finalError);
        }

        private TimeSpan CalculateDelay(int retryCount, RetryPolicy policy)
        {
            var delay = policy switch
            {
                RetryPolicy.Fixed => _baseDelay,
                RetryPolicy.Linear => TimeSpan.FromSeconds(_baseDelay.TotalSeconds * (retryCount + 1)),
                RetryPolicy.Exponential => TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryCount) * _baseDelay.TotalSeconds, _maxDelay.TotalSeconds)),
                _ => _baseDelay
            };

            var jitter = Random.Shared.NextDouble() * delay.TotalSeconds * 0.1;
            return delay + TimeSpan.FromSeconds(jitter);
        }
    }

    public class RetryState
    {
        public int RetryCount { get; set; }
        public Exception? LastError { get; set; }
    }

    public enum RetryPolicy
    {
        Fixed,
        Linear,
        Exponential
    }
}
