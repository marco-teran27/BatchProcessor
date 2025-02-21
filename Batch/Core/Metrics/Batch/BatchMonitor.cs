using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using System.Linq;

/*
File: BatchProcessor\Core\Metrics\Batch\BatchMonitor.cs
Summary: Monitors batch processing by tracking file metrics.
         Synchronous operations are used as these interactions with Rhino and local data are inherently blocking.
*/

namespace BatchProcessor.Core.Metrics.Batch
{
    /// <summary>
    /// Monitors batch processing operations in real-time.
    /// </summary>
    public class BatchMonitor : IDisposable
    {
        private readonly ILogger<BatchMonitor> _logger;
        private readonly ConcurrentDictionary<string, FileMetric> _activeFiles;
        private bool _isMonitoring;
        private bool _disposed;
        private DateTime _batchStartTime;

        /// <summary>
        /// Initializes a new instance of BatchMonitor.
        /// </summary>
        public BatchMonitor(ILogger<BatchMonitor> logger)
        {
            _logger = logger;
            _activeFiles = new ConcurrentDictionary<string, FileMetric>();
        }

        /// <summary>
        /// Starts the batch monitoring session.
        /// </summary>
        public void StartMonitoring()
        {
            _batchStartTime = DateTime.Now;
            _isMonitoring = true;
            _logger.LogInformation("Batch monitoring started");
        }

        /// <summary>
        /// Stops the batch monitoring session.
        /// </summary>
        public void StopMonitoring()
        {
            _isMonitoring = false;
            _logger.LogInformation("Batch monitoring stopped");
        }

        /// <summary>
        /// Tracks the start of processing for a given file.
        /// </summary>
        public void TrackFileStart(string fileName)
        {
            if (!_isMonitoring) return;

            var metric = new FileMetric
            {
                FileName = fileName,
                StartTime = DateTime.Now,
                Status = FileStatus.Running
            };

            _activeFiles.TryAdd(fileName, metric);
        }

        /// <summary>
        /// Marks the completion of file processing and records metrics.
        /// </summary>
        public void TrackFileComplete(string fileName, bool success)
        {
            if (!_isMonitoring) return;

            if (_activeFiles.TryGetValue(fileName, out var metric))
            {
                metric.EndTime = DateTime.Now;
                metric.Status = success ? FileStatus.Pass : FileStatus.Fail;
                metric.ProcessingTimeSeconds = (metric.EndTime - metric.StartTime).TotalSeconds;
            }
        }

        /// <summary>
        /// Retrieves metrics for a specified file.
        /// </summary>
        public virtual FileMetric GetFileMetrics(string fileName)
        {
            return _activeFiles.TryGetValue(fileName, out var metric)
                ? metric
                : new FileMetric { FileName = fileName, Status = FileStatus.Missing };
        }

        /// <summary>
        /// Gets current batch metrics.
        /// </summary>
        public BatchMetrics GetCurrentMetrics()
        {
            return new BatchMetrics
            {
                TotalFiles = _activeFiles.Count,
                ActiveFiles = _activeFiles.Count(f => f.Value.Status == FileStatus.Running),
                CompletedFiles = _activeFiles.Count(f => f.Value.Status == FileStatus.Pass),
                FailedFiles = _activeFiles.Count(f => f.Value.Status == FileStatus.Fail),
                BatchRunTime = DateTime.Now - _batchStartTime
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _activeFiles.Clear();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Disposes the BatchMonitor.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public enum FileStatus
        {
            Missing,
            Running,
            Pass,
            Fail,
            Cancelled,
            Timeout
        }

        /// <summary>
        /// Represents metrics for a single file.
        /// </summary>
        public class FileMetric
        {
            public string FileName { get; set; } = string.Empty;
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public double ProcessingTimeSeconds { get; set; }
            public FileStatus Status { get; set; }
        }

        /// <summary>
        /// Represents aggregated batch metrics.
        /// </summary>
        public class BatchMetrics
        {
            public int TotalFiles { get; set; }
            public int ActiveFiles { get; set; }
            public int CompletedFiles { get; set; }
            public int FailedFiles { get; set; }
            public TimeSpan BatchRunTime { get; set; }
        }
    }
}
