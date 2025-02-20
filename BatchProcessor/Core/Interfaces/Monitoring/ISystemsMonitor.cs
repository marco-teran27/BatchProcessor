using System.Collections.Generic;
using BatchProcessor.Core.Metrics.System;  // This namespace contains the SystemMonitor class and its nested ResourceSnapshot type

namespace BatchProcessor.DI.Interfaces.Monitoring
{
    /// <summary>
    /// Defines a contract for monitoring system resource usage.
    /// </summary>
    public interface ISystemsMonitor
    {
        /// <summary>
        /// Starts monitoring system resources.
        /// </summary>
        void StartMonitoring();

        /// <summary>
        /// Stops monitoring system resources.
        /// </summary>
        void StopMonitoring();

        /// <summary>
        /// Captures the current system resource usage.
        /// </summary>
        /// <returns>
        /// A <see cref="SystemMonitor.ResourceSnapshot"/> representing the current CPU usage, memory usage, and other resource data.
        /// </returns>
        SystemMonitor.ResourceSnapshot CaptureMetrics();

        /// <summary>
        /// Retrieves historical system resource usage snapshots.
        /// </summary>
        /// <returns>
        /// A read-only list of <see cref="SystemMonitor.ResourceSnapshot"/> instances representing the resource history.
        /// </returns>
        IReadOnlyList<SystemMonitor.ResourceSnapshot> GetMetricsHistory();
    }
}
