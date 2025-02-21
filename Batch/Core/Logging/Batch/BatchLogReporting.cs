// File: Core/Logic/Batch/BatchLogReporting.cs

using System;
using System.Threading.Tasks;
using BatchProcessor.DI.Interfaces.AbsRhino;
using BatchProcessor.DI.Interfaces.Logging;
using BatchProcessor.Core.Models;

namespace BatchProcessor.Core.Logic.Batch
{
    /// <summary>
    /// Handles batch processing progress reporting and logging
    /// </summary>
    public class BatchLogReporting
    {
        private readonly ICommLineOut _output;
        private readonly IBatchLogger _logger;
        private DateTime _startTime;
        private int _totalFiles;
        private int _processedFiles;

        public BatchLogReporting(ICommLineOut output, IBatchLogger logger)
        {
            _output = output;
            _logger = logger;
        }

        /// <summary>
        /// Initializes reporting for a new batch
        /// </summary>
        public void InitializeReporting(int totalFiles)
        {
            _startTime = DateTime.Now;
            _totalFiles = totalFiles;
            _processedFiles = 0;
            
            _output.ShowMessage($"Starting batch processing of {totalFiles} files");
        }

        /// <summary>
        /// Updates progress for current file
        /// </summary>
        public void UpdateProgress(string currentFile)
        {
            _processedFiles++;
            var estimatedTime = CalculateEstimatedTimeRemaining();
            _output.UpdateProgress(_processedFiles, _totalFiles, currentFile, estimatedTime);
        }

        /// <summary>
        /// Logs file completion status
        /// </summary>
        public void LogFileCompletion(
            string fileName, 
            bool success, 
            string details, 
            ProcessingMetrics metrics)
        {
            _logger.LogFileProcessed(
                fileName,
                success,
                details,
                metrics: metrics,
                processingStartTime: DateTime.Now
            );
        }

        private TimeSpan CalculateEstimatedTimeRemaining()
        {
            if (_processedFiles == 0) return TimeSpan.FromMinutes(5); // Initial estimate

            var elapsed = DateTime.Now - _startTime;
            var averageTimePerFile = elapsed.TotalSeconds / _processedFiles;
            var remainingFiles = _totalFiles - _processedFiles;
            
            return TimeSpan.FromSeconds(averageTimePerFile * remainingFiles);
        }
    }
}