using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BatchProcessor.Core.Metrics.Batch
{
    /// <summary>
    /// Stores and retrieves batch metrics in standardized formats
    /// </summary>
    public class BatchMetricStorage
    {
        private readonly ILogger<BatchMetricStorage> _logger;
        private readonly BatchCollector _collector;
        private readonly string _storageDirectory;
        private readonly string _projectName;

        public BatchMetricStorage(
            ILogger<BatchMetricStorage> logger,
            BatchCollector collector,
            string storageDirectory,
            string projectName)
        {
            _logger = logger;
            _collector = collector;
            _storageDirectory = storageDirectory;
            _projectName = projectName;

            Directory.CreateDirectory(storageDirectory);
        }

        /// <summary>
        /// Gets metrics formatted for command line display
        /// </summary>
        public CommandLineMetrics GetCommandLineMetrics(string fileName)
        {
            var summary = _collector.GetFileMetricSummary(fileName);
            return new CommandLineMetrics
            {
                FileName = summary.FileName,
                Status = summary.LastStatus.ToString(),
                ProcessingTime = $"{summary.AverageProcessingTime:F2}s",
                Attempts = summary.TotalAttempts,
                LastUpdate = summary.LastAttemptTime
            };
        }

        /// <summary>
        /// Gets metrics formatted for log file
        /// </summary>
        public LogMetrics GetLogMetrics(string fileName)
        {
            var summary = _collector.GetFileMetricSummary(fileName);
            var batchSummary = _collector.GetBatchSummary();

            return new LogMetrics
            {
                FileName = summary.FileName,
                Status = summary.LastStatus,
                ProcessingDetails = new ProcessingDetails
                {
                    TotalAttempts = summary.TotalAttempts,
                    TotalProcessingTime = summary.TotalProcessingTime,
                    AverageProcessingTime = summary.AverageProcessingTime,
                    FirstAttempt = summary.FirstAttemptTime,
                    LastAttempt = summary.LastAttemptTime
                },
                BatchContext = new BatchContext
                {
                    TotalFiles = batchSummary.TotalFiles,
                    CompletedFiles = batchSummary.CompletedFiles,
                    SuccessRate = batchSummary.SuccessRate
                }
            };
        }

        /// <summary>
        /// Saves metrics to storage
        /// </summary>
        public async Task SaveMetrics(string fileName)
        {
            try
            {
                var metrics = GetLogMetrics(fileName);
                var filePath = GetMetricFilePath(fileName);

                await File.WriteAllTextAsync(
                    filePath,
                    JsonSerializer.Serialize(metrics, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    })
                );

                _logger.LogInformation($"Saved metrics for {fileName} to {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving metrics for {fileName}");
            }
        }

        private string GetMetricFilePath(string fileName)
        {
            return Path.Combine(
                _storageDirectory,
                $"metrics_{_projectName}_{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            );
        }

        public class CommandLineMetrics
        {
            public string FileName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string ProcessingTime { get; set; } = string.Empty;
            public int Attempts { get; set; }
            public DateTime LastUpdate { get; set; }
        }

        public class LogMetrics
        {
            public string FileName { get; set; } = string.Empty;
            public BatchMonitor.FileStatus Status { get; set; }
            public ProcessingDetails ProcessingDetails { get; set; } = new();
            public BatchContext BatchContext { get; set; } = new();
        }

        public class ProcessingDetails
        {
            public int TotalAttempts { get; set; }
            public double TotalProcessingTime { get; set; }
            public double AverageProcessingTime { get; set; }
            public DateTime FirstAttempt { get; set; }
            public DateTime LastAttempt { get; set; }
        }

        public class BatchContext
        {
            public int TotalFiles { get; set; }
            public int CompletedFiles { get; set; }
            public double SuccessRate { get; set; }
        }
    }
}