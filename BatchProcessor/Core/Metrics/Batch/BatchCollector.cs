using System;
using System.Linq;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace BatchProcessor.Core.Metrics.Batch
{
    /// <summary>
    /// Collects and aggregates batch processing metrics
    /// </summary>
    public class BatchCollector
    {
        private readonly ILogger<BatchCollector> _logger;
        private readonly BatchMonitor _monitor;
        private readonly ConcurrentDictionary<string, List<BatchMonitor.FileMetric>> _fileHistory;

        public BatchCollector(
            ILogger<BatchCollector> logger,
            BatchMonitor monitor)
        {
            _logger = logger;
            _monitor = monitor;
            _fileHistory = new ConcurrentDictionary<string, List<BatchMonitor.FileMetric>>();
        }

        /// <summary>
        /// Collects current metrics and adds to history
        /// </summary>
        public void CollectMetrics(string fileName)
        {
            var currentMetrics = _monitor.GetFileMetrics(fileName);
            
            _fileHistory.AddOrUpdate(
                fileName,
                new List<BatchMonitor.FileMetric> { currentMetrics },
                (_, list) =>
                {
                    list.Add(currentMetrics);
                    return list;
                }
            );
        }

        /// <summary>
        /// Gets aggregated metrics for a file
        /// </summary>
        public FileMetricSummary GetFileMetricSummary(string fileName)
        {
            if (!_fileHistory.TryGetValue(fileName, out var history))
            {
                return new FileMetricSummary { FileName = fileName };
            }

            return new FileMetricSummary
            {
                FileName = fileName,
                TotalAttempts = history.Count,
                LastStatus = history[^1].Status,
                TotalProcessingTime = history.Sum(m => m.ProcessingTimeSeconds),
                AverageProcessingTime = history.Average(m => m.ProcessingTimeSeconds),
                FirstAttemptTime = history[0].StartTime,
                LastAttemptTime = history[^1].EndTime
            };
        }

        /// <summary>
        /// Gets overall batch summary
        /// </summary>
        public BatchMetricSummary GetBatchSummary()
        {
            var currentMetrics = _monitor.GetCurrentMetrics();
            var allFiles = _fileHistory.Values.SelectMany(x => x).ToList();

            return new BatchMetricSummary
            {
                TotalFiles = currentMetrics.TotalFiles,
                CompletedFiles = currentMetrics.CompletedFiles,
                FailedFiles = currentMetrics.FailedFiles,
                TotalProcessingTime = currentMetrics.BatchRunTime,
                AverageFileProcessingTime = allFiles.Average(f => f.ProcessingTimeSeconds),
                MaxFileProcessingTime = allFiles.Max(f => f.ProcessingTimeSeconds),
                MinFileProcessingTime = allFiles.Min(f => f.ProcessingTimeSeconds),
                SuccessRate = (double)currentMetrics.CompletedFiles / currentMetrics.TotalFiles * 100
            };
        }

        public class FileMetricSummary
        {
            public string FileName { get; set; } = string.Empty;
            public int TotalAttempts { get; set; }
            public BatchMonitor.FileStatus LastStatus { get; set; }
            public double TotalProcessingTime { get; set; }
            public double AverageProcessingTime { get; set; }
            public DateTime FirstAttemptTime { get; set; }
            public DateTime LastAttemptTime { get; set; }
        }

        public class BatchMetricSummary
        {
            public int TotalFiles { get; set; }
            public int CompletedFiles { get; set; }
            public int FailedFiles { get; set; }
            public TimeSpan TotalProcessingTime { get; set; }
            public double AverageFileProcessingTime { get; set; }
            public double MaxFileProcessingTime { get; set; }
            public double MinFileProcessingTime { get; set; }
            public double SuccessRate { get; set; }
        }
    }
}