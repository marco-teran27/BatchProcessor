using System;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using BatchProcessor.DI.Interfaces.Monitoring; // Required for ISystemsMonitor

/*
File: BatchProcessor\Core\Metrics\System\SystemMonitor.cs
Summary: Provides real-time monitoring of system resources such as CPU usage, memory usage,
         thread count, and working set memory. It captures snapshots of these metrics over time,
         logs monitoring start/stop events, and evaluates the resource state based on predefined thresholds.
         This class implements the ISystemsMonitor interface, making it compatible with dependency injection.
*/

namespace BatchProcessor.Core.Metrics.System
{
    /// <summary>
    /// Provides real-time monitoring of system resources.
    /// Implements the ISystemsMonitor interface for integration with dependency injection.
    /// </summary>
    public class SystemMonitor : ISystemsMonitor, IDisposable
    {
        // Logger for logging events, warnings, and errors.
        private readonly ILogger<SystemMonitor> _logger;

        // Performance counter for monitoring the CPU usage of the current process.
        private readonly PerformanceCounter _cpuCounter;

        // Reference to the current process for gathering memory and thread information.
        private readonly Process _currentProcess;

        // Thread-safe queue that stores snapshots of system resource usage.
        private readonly ConcurrentQueue<ResourceSnapshot> _snapshots;

        // Indicates whether system monitoring is currently active.
        private bool _isMonitoring;

        // Flag to indicate if the instance has been disposed.
        private bool _disposed;

        // Timestamp marking when resource monitoring started.
        private DateTime _collectionStartTime;

        // Predefined CPU usage thresholds for warning and critical states (percentage).
        private const double CPU_WARNING_THRESHOLD = 80.0;
        private const double CPU_CRITICAL_THRESHOLD = 90.0;

        // Predefined memory usage thresholds for warning and critical states (in MB).
        private const double MEMORY_WARNING_THRESHOLD_MB = 1000.0;
        private const double MEMORY_CRITICAL_THRESHOLD_MB = 1500.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemMonitor"/> class.
        /// Sets up the performance counter and initializes snapshot storage.
        /// </summary>
        /// <param name="logger">The logger instance used for logging events and errors.</param>
        public SystemMonitor(ILogger<SystemMonitor> logger)
        {
            _logger = logger;
            _currentProcess = Process.GetCurrentProcess();

            // Initialize a performance counter to monitor CPU usage for the current process.
            _cpuCounter = new PerformanceCounter("Process", "% Processor Time", _currentProcess.ProcessName, true);

            _snapshots = new ConcurrentQueue<ResourceSnapshot>();
        }

        /// <summary>
        /// Starts the system resource monitoring process.
        /// </summary>
        public void StartMonitoring()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SystemMonitor));

            _isMonitoring = true;
            _collectionStartTime = DateTime.Now;
            _logger.LogInformation("System resource monitoring started at {StartTime}", _collectionStartTime);
        }

        /// <summary>
        /// Stops the system resource monitoring process.
        /// </summary>
        public void StopMonitoring()
        {
            _isMonitoring = false;
            _logger.LogInformation("System resource monitoring stopped");
        }

        /// <summary>
        /// Captures a snapshot of the current system resource usage.
        /// </summary>
        /// <returns>
        /// A <see cref="ResourceSnapshot"/> object containing current CPU usage, memory usage, thread count,
        /// working set memory, and the evaluated resource state.
        /// </returns>
        public ResourceSnapshot CaptureSnapshot()
        {
            // If monitoring is not active, return an empty snapshot.
            if (!_isMonitoring)
                return new ResourceSnapshot();

            // Create a snapshot with current system metrics.
            var snapshot = new ResourceSnapshot
            {
                Timestamp = DateTime.Now,
                CpuPercentage = GetCpuUsage(),
                MemoryUsageMB = GetMemoryUsage(),
                ThreadCount = _currentProcess.Threads.Count,
                WorkingSet = _currentProcess.WorkingSet64,
                ResourceState = DetermineResourceState()
            };

            // Enqueue the new snapshot.
            _snapshots.Enqueue(snapshot);

            // Keep the snapshot collection size to a maximum of 1000 entries.
            while (_snapshots.Count > 1000)
                _snapshots.TryDequeue(out _);

            return snapshot;
        }

        /// <summary>
        /// Captures the current system metrics.
        /// This method is provided to implement the ISystemsMonitor interface.
        /// It delegates to the existing <see cref="CaptureSnapshot"/> method.
        /// </summary>
        /// <returns>A <see cref="ResourceSnapshot"/> representing the current metrics snapshot.</returns>
        public ResourceSnapshot CaptureMetrics() => CaptureSnapshot();

        /// <summary>
        /// Retrieves a read-only list of all captured resource snapshots.
        /// This method is provided to implement the ISystemsMonitor interface.
        /// </summary>
        /// <returns>
        /// A read-only list of <see cref="ResourceSnapshot"/> instances representing the historical resource usage.
        /// </returns>
        public IReadOnlyList<ResourceSnapshot> GetMetricsHistory()
        {
            // Convert the concurrent queue to an array, which implements IReadOnlyList<T>.
            return _snapshots.ToArray();
        }

        /// <summary>
        /// Retrieves the current CPU usage percentage of the process.
        /// </summary>
        /// <returns>The CPU usage percentage, rounded to two decimal places.</returns>
        private double GetCpuUsage()
        {
            try
            {
                // Obtain the next value from the CPU performance counter.
                return Math.Round(_cpuCounter.NextValue(), 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving CPU usage");
                return 0;
            }
        }

        /// <summary>
        /// Retrieves the current memory usage of the process in megabytes.
        /// </summary>
        /// <returns>The memory usage in MB, rounded to two decimal places.</returns>
        private double GetMemoryUsage()
        {
            try
            {
                // Convert the working set (bytes) to megabytes.
                return Math.Round(_currentProcess.WorkingSet64 / 1024.0 / 1024.0, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving memory usage");
                return 0;
            }
        }

        /// <summary>
        /// Determines the current resource state based on CPU and memory usage.
        /// </summary>
        /// <returns>
        /// A <see cref="ResourceState"/> value indicating whether the resource usage is Normal, Warning, or Critical.
        /// </returns>
        private ResourceState DetermineResourceState()
        {
            // Retrieve current metrics.
            var cpu = GetCpuUsage();
            var memory = GetMemoryUsage();

            // Check if either CPU or memory usage exceeds the critical thresholds.
            if (cpu >= CPU_CRITICAL_THRESHOLD || memory >= MEMORY_CRITICAL_THRESHOLD_MB)
                return ResourceState.Critical;

            // Check if either CPU or memory usage exceeds the warning thresholds.
            if (cpu >= CPU_WARNING_THRESHOLD || memory >= MEMORY_WARNING_THRESHOLD_MB)
                return ResourceState.Warning;

            // Default to a normal state.
            return ResourceState.Normal;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SystemMonitor"/> and optionally releases managed resources.
        /// </summary>
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose of managed resources.
                    _cpuCounter.Dispose();

                    // Clear all stored snapshots.
                    while (_snapshots.TryDequeue(out _)) { }
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="SystemMonitor"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Represents a snapshot of system resource usage at a specific point in time.
        /// </summary>
        public class ResourceSnapshot
        {
            /// <summary>
            /// Gets or sets the timestamp when the snapshot was captured.
            /// </summary>
            public DateTime Timestamp { get; set; }

            /// <summary>
            /// Gets or sets the CPU usage percentage.
            /// </summary>
            public double CpuPercentage { get; set; }

            /// <summary>
            /// Gets or sets the memory usage in megabytes.
            /// </summary>
            public double MemoryUsageMB { get; set; }

            /// <summary>
            /// Gets or sets the number of threads in the process.
            /// </summary>
            public int ThreadCount { get; set; }

            /// <summary>
            /// Gets or sets the working set memory (in bytes) of the process.
            /// </summary>
            public long WorkingSet { get; set; }

            /// <summary>
            /// Gets or sets the resource state indicating if usage is Normal, Warning, or Critical.
            /// </summary>
            public ResourceState ResourceState { get; set; }
        }

        /// <summary>
        /// Enumerates the possible states of system resource usage.
        /// </summary>
        public enum ResourceState
        {
            /// <summary>
            /// Indicates normal resource usage.
            /// </summary>
            Normal,

            /// <summary>
            /// Indicates that resource usage is in a warning state.
            /// </summary>
            Warning,

            /// <summary>
            /// Indicates that resource usage is in a critical state.
            /// </summary>
            Critical
        }
    }
}
