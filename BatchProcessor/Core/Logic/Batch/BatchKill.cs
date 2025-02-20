using System;
using BatchProcessor.DI.Interfaces.AbsRhino;
using Microsoft.Extensions.Logging;
// Add this alias so that ResourceSnapshot refers to the nested type in SystemMonitor.
using ResourceSnapshot = BatchProcessor.Core.Metrics.System.SystemMonitor.ResourceSnapshot;

namespace BatchProcessor.Core.Logic.Batch
{
    /// <summary>
    /// BatchKill monitors system resource thresholds and implements a circuit breaker pattern
    /// to protect against system overload during batch processing.
    /// 
    /// It evaluates current system metrics (CPU and memory usage) and, if either exceeds defined thresholds,
    /// records a failure. If too many consecutive failures occur, the circuit breaker is opened to halt operations
    /// until sufficient time has elapsed for recovery.
    /// </summary>
    public class BatchKill
    {
        private readonly ICommLineOut _output;
        private readonly ILogger<BatchKill> _logger;
        private readonly int _failureThreshold;
        private readonly TimeSpan _resetTimeout;
        private int _failureCount;
        private DateTime _lastFailure;
        private bool _isOpen;
        private readonly object _lockObj = new();

        // Resource thresholds based on system resource metrics:
        private const double CPU_THRESHOLD = 90.0;    // 90% CPU usage threshold
        private const double MEMORY_THRESHOLD = 85.0; // 85% memory usage threshold

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchKill"/> class.
        /// </summary>
        /// <param name="output">Interface for outputting messages to the command line.</param>
        /// <param name="logger">The logger instance for diagnostic messages.</param>
        /// <param name="failureThreshold">
        /// The number of consecutive failures (resource threshold breaches) required to open the circuit breaker.
        /// </param>
        /// <param name="resetMinutes">
        /// The number of minutes to wait after the last failure before the circuit breaker is reset.
        /// </param>
        public BatchKill(
            ICommLineOut output,
            ILogger<BatchKill> logger,
            int failureThreshold = 5,
            int resetMinutes = 15)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _failureThreshold = failureThreshold;
            _resetTimeout = TimeSpan.FromMinutes(resetMinutes);
        }

        /// <summary>
        /// Checks if the batch operation can continue based on current system resource metrics.
        /// 
        /// This method uses a <see cref="ResourceSnapshot"/> (obtained, for example, from a system monitor)
        /// and compares its CPU and memory usage against predefined thresholds.
        /// If either value exceeds the threshold, a failure is recorded.
        /// If too many failures occur, the circuit breaker is opened.
        /// </summary>
        /// <param name="metrics">
        /// The current system resource metrics snapshot. The snapshot provides <c>CpuPercentage</c>
        /// and <c>MemoryUsageMB</c> properties.
        /// </param>
        /// <returns>
        /// True if the resources are within acceptable limits or if the circuit breaker has reset; otherwise, false.
        /// </returns>
        public bool CanContinueOperation(ResourceSnapshot metrics)
        {
            lock (_lockObj)
            {
                // If the circuit breaker is already open, check if the reset timeout has elapsed.
                if (_isOpen)
                {
                    if (DateTime.Now - _lastFailure > _resetTimeout)
                    {
                        ResetCircuit();
                        return true;
                    }
                    _output.ShowMessage("Circuit breaker is open - waiting for system recovery");
                    return false;
                }

                // Compare system metrics against the thresholds.
                // Note: We use CpuPercentage (instead of CpuUsage) as defined in ResourceSnapshot.
                if (metrics.CpuPercentage > CPU_THRESHOLD || metrics.MemoryUsageMB > MEMORY_THRESHOLD)
                {
                    RecordFailure($"Resource threshold exceeded - CPU: {metrics.CpuPercentage}%, Memory: {metrics.MemoryUsageMB}MB");
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Records a failure due to exceeded resource thresholds and, if necessary,
        /// opens the circuit breaker to halt further processing.
        /// </summary>
        /// <param name="reason">A description of the failure reason.</param>
        public void RecordFailure(string reason)
        {
            lock (_lockObj)
            {
                _failureCount++;
                _lastFailure = DateTime.Now;
                _logger.LogWarning($"Failure recorded: {reason}. Count: {_failureCount}");

                if (_failureCount >= _failureThreshold)
                {
                    _isOpen = true;
                    _output.ShowError($"Circuit breaker opened after {_failureCount} failures. Latest: {reason}");
                    _logger.LogError("Circuit breaker opened - batch processing paused");
                }
            }
        }

        /// <summary>
        /// Resets the circuit breaker state, clearing the failure count and allowing batch operations to resume.
        /// </summary>
        public void ResetCircuit()
        {
            lock (_lockObj)
            {
                _isOpen = false;
                _failureCount = 0;
                _logger.LogInformation("Circuit breaker reset - resuming batch processing");
            }
        }

        /// <summary>
        /// Determines whether the circuit breaker is currently open.
        /// </summary>
        /// <returns>True if the circuit breaker is open; otherwise, false.</returns>
        public bool IsCircuitOpen()
        {
            lock (_lockObj)
            {
                return _isOpen;
            }
        }
    }
}
