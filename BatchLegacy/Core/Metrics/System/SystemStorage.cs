using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
// Create an alias so we can refer to the nested type ResourceSnapshot simply
using ResourceSnapshot = BatchProcessor.Core.Metrics.System.SystemMonitor.ResourceSnapshot;

namespace BatchProcessor.Core.Metrics.System
{
    /// <summary>
    /// SystemStorage is responsible for persisting system metrics data to disk,
    /// as well as retrieving and formatting those metrics for display or logging.
    /// It stores data such as CPU and memory usage captured during a metrics session.
    /// </summary>
    public class SystemStorage
    {
        private readonly ILogger<SystemStorage> _logger;
        private readonly string _storagePath;
        private readonly object _lockObject = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemStorage"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for diagnostic messages.</param>
        /// <param name="storagePath">The directory path where metrics will be stored.</param>
        public SystemStorage(ILogger<SystemStorage> logger, string storagePath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _storagePath = storagePath;
            Directory.CreateDirectory(storagePath);
        }

        /// <summary>
        /// Asynchronously stores a collection of system resource snapshots for a given session.
        /// </summary>
        /// <param name="sessionId">The identifier for the current metrics session.</param>
        /// <param name="metrics">The collection of ResourceSnapshot instances to store.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StoreMetrics(string sessionId, IEnumerable<ResourceSnapshot> metrics)
        {
            try
            {
                var storageFile = GetStorageFilePath(sessionId);
                var formattedMetrics = new SystemMetricStorage
                {
                    SessionId = sessionId,
                    StorageTime = DateTime.Now,
                    Metrics = metrics.ToList()
                };

                await File.WriteAllTextAsync(
                    storageFile,
                    JsonSerializer.Serialize(formattedMetrics, new JsonSerializerOptions { WriteIndented = true })
                );

                _logger.LogInformation($"Stored metrics for session {sessionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error storing metrics for session {sessionId}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves and formats the stored system metrics for command-line display.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>
        /// A <see cref="SystemMetricDisplay"/> object containing summary information,
        /// or with NoDataAvailable set to true if no metrics were found.
        /// </returns>
        public SystemMetricDisplay GetDisplayMetrics(string sessionId)
        {
            try
            {
                var metrics = LoadMetrics(sessionId);
                if (metrics == null || !metrics.Metrics.Any())
                {
                    return new SystemMetricDisplay
                    {
                        SessionId = sessionId,
                        NoDataAvailable = true
                    };
                }

                var recentMetrics = metrics.Metrics
                    .OrderByDescending(m => m.Timestamp)
                    .Take(10)
                    .ToList();

                return new SystemMetricDisplay
                {
                    SessionId = sessionId,
                    LastUpdateTime = recentMetrics.First().Timestamp,
                    CurrentCpuPercentage = recentMetrics.First().CpuPercentage,
                    CurrentMemoryMB = recentMetrics.First().MemoryUsageMB,
                    AverageCpuPercentage = recentMetrics.Average(m => m.CpuPercentage),
                    AverageMemoryMB = recentMetrics.Average(m => m.MemoryUsageMB)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting display metrics for session {sessionId}");
                return new SystemMetricDisplay
                {
                    SessionId = sessionId,
                    NoDataAvailable = true,
                    ErrorMessage = "Error retrieving metrics"
                };
            }
        }

        /// <summary>
        /// Retrieves and formats the stored system metrics for logging purposes.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>
        /// A <see cref="SystemMetricLog"/> object containing detailed log metrics,
        /// or a default instance with an error message if retrieval fails.
        /// </returns>
        public SystemMetricLog GetLogMetrics(string sessionId)
        {
            try
            {
                var metrics = LoadMetrics(sessionId);
                if (metrics == null)
                    return new SystemMetricLog { SessionId = sessionId };

                return new SystemMetricLog
                {
                    SessionId = sessionId,
                    StartTime = metrics.Metrics.Min(m => m.Timestamp),
                    EndTime = metrics.Metrics.Max(m => m.Timestamp),
                    TotalSamples = metrics.Metrics.Count,
                    AverageCpuPercentage = metrics.Metrics.Average(m => m.CpuPercentage),
                    AverageMemoryMB = metrics.Metrics.Average(m => m.MemoryUsageMB),
                    PeakCpuPercentage = metrics.Metrics.Max(m => m.CpuPercentage),
                    PeakMemoryMB = metrics.Metrics.Max(m => m.MemoryUsageMB)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting log metrics for session {sessionId}");
                return new SystemMetricLog
                {
                    SessionId = sessionId,
                    ErrorMessage = "Error retrieving metrics"
                };
            }
        }

        /// <summary>
        /// Constructs a file path for storing metrics data based on the session identifier and current timestamp.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>A full file path for the metrics file.</returns>
        private string GetStorageFilePath(string sessionId)
        {
            return Path.Combine(
                _storagePath,
                $"metrics_{sessionId}_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            );
        }

        /// <summary>
        /// Loads the stored system metrics for a given session.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>A <see cref="SystemMetricStorage"/> object if the file exists; otherwise, null.</returns>
        private SystemMetricStorage? LoadMetrics(string sessionId)
        {
            var storageFile = GetStorageFilePath(sessionId);
            if (!File.Exists(storageFile))
                return null;

            var json = File.ReadAllText(storageFile);
            return JsonSerializer.Deserialize<SystemMetricStorage>(json);
        }
    }

    /// <summary>
    /// Represents the stored system metrics along with session metadata.
    /// </summary>
    public class SystemMetricStorage
    {
        /// <summary>
        /// The session identifier.
        /// </summary>
        public string SessionId { get; set; } = string.Empty;
        /// <summary>
        /// The timestamp when the metrics were stored.
        /// </summary>
        public DateTime StorageTime { get; set; }
        /// <summary>
        /// The list of collected system resource snapshots.
        /// </summary>
        public List<ResourceSnapshot> Metrics { get; set; } = new List<ResourceSnapshot>();
    }

    /// <summary>
    /// Represents a summary of system metrics for command-line display.
    /// </summary>
    public class SystemMetricDisplay
    {
        /// <summary>
        /// The session identifier.
        /// </summary>
        public string SessionId { get; set; } = string.Empty;
        /// <summary>
        /// Indicates whether no data is available.
        /// </summary>
        public bool NoDataAvailable { get; set; }
        /// <summary>
        /// An error message if data could not be retrieved.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        /// <summary>
        /// The timestamp of the most recent metric sample.
        /// </summary>
        public DateTime LastUpdateTime { get; set; }
        /// <summary>
        /// The current CPU usage percentage.
        /// </summary>
        public double CurrentCpuPercentage { get; set; }
        /// <summary>
        /// The current memory usage in MB.
        /// </summary>
        public double CurrentMemoryMB { get; set; }
        /// <summary>
        /// The average CPU usage percentage over recent samples.
        /// </summary>
        public double AverageCpuPercentage { get; set; }
        /// <summary>
        /// The average memory usage in MB over recent samples.
        /// </summary>
        public double AverageMemoryMB { get; set; }
    }

    /// <summary>
    /// Represents detailed system metrics for logging purposes.
    /// </summary>
    public class SystemMetricLog
    {
        /// <summary>
        /// The session identifier.
        /// </summary>
        public string SessionId { get; set; } = string.Empty;
        /// <summary>
        /// An error message if metric retrieval failed.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        /// <summary>
        /// The earliest timestamp among the samples.
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// The latest timestamp among the samples.
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// The total number of metric samples collected.
        /// </summary>
        public int TotalSamples { get; set; }
        /// <summary>
        /// The average CPU usage percentage over all samples.
        /// </summary>
        public double AverageCpuPercentage { get; set; }
        /// <summary>
        /// The average memory usage in MB over all samples.
        /// </summary>
        public double AverageMemoryMB { get; set; }
        /// <summary>
        /// The highest CPU usage percentage observed.
        /// </summary>
        public double PeakCpuPercentage { get; set; }
        /// <summary>
        /// The highest memory usage in MB observed.
        /// </summary>
        public double PeakMemoryMB { get; set; }
    }
}
