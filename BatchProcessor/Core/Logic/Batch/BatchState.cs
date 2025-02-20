using System;
using System.IO;                   // For Directory, File, and Path
using System.Threading;            // For Interlocked
using System.Linq;                 // For ToDictionary and other LINQ extensions
using System.Collections.Generic;  // For Dictionary<,>
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text.Json;
using BatchProcessor.Core.Models;
using Microsoft.Extensions.Logging;

namespace BatchProcessor.Core.Logic.Batch
{
    /// <summary>
    /// Manages state and checkpoints during batch processing
    /// </summary>
    public class BatchState
    {
        private readonly ILogger<BatchState> _logger;
        private readonly string _checkpointDir;
        private readonly ConcurrentDictionary<string, FileState> _fileStates;
        private int _totalFiles;
        private int _completedFiles;
        private bool _isProcessing;
        private DateTime _lastCheckpoint;

        public BatchState(ILogger<BatchState> logger, string checkpointDir)
        {
            _logger = logger;
            _checkpointDir = checkpointDir;
            _fileStates = new ConcurrentDictionary<string, FileState>();
            Directory.CreateDirectory(checkpointDir);
        }

        /// <summary>
        /// Initializes the batch process with the total number of files
        /// </summary>
        public void InitializeBatch(int totalFiles)
        {
            _totalFiles = totalFiles;
            _completedFiles = 0;
            _isProcessing = true;
            _lastCheckpoint = DateTime.Now;
            _fileStates.Clear();
            _logger.LogInformation($"Batch initialized with {totalFiles} files");
        }

        /// <summary>
        /// Updates the state of a specific file
        /// </summary>
        public void UpdateFileState(string fileName, BatchStatus status, string details = "")
        {
            var state = new FileState
            {
                FileName = fileName,
                Status = status,
                LastUpdateTime = DateTime.Now,
                Details = details
            };

            _fileStates.AddOrUpdate(fileName, state, (_, _) => state);

            if (status is BatchStatus.Pass or BatchStatus.Fail)
            {
                Interlocked.Increment(ref _completedFiles);
            }
        }

        /// <summary>
        /// Completes the batch processing
        /// </summary>
        public void CompleteBatch()
        {
            _isProcessing = false;
            _logger.LogInformation($"Batch completed. Processed {_completedFiles} of {_totalFiles} files");
        }

        /// <summary>
        /// Creates a checkpoint of the current state
        /// </summary>
        public async Task CreateCheckpoint()
        {
            try
            {
                var checkpoint = new BatchCheckpoint
                {
                    Timestamp = DateTime.Now,
                    TotalFiles = _totalFiles,
                    CompletedFiles = _completedFiles,
                    IsProcessing = _isProcessing,
                    FileStates = _fileStates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                };

                var checkpointPath = GetCheckpointPath();
                await File.WriteAllTextAsync(
                    checkpointPath,
                    JsonSerializer.Serialize(checkpoint)
                );

                _lastCheckpoint = DateTime.Now;
                _logger.LogInformation($"Checkpoint created at {checkpointPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create checkpoint");
            }
        }

        private string GetCheckpointPath()
        {
            return Path.Combine(
                _checkpointDir,
                $"checkpoint_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            );
        }

        private class FileState
        {
            public string FileName { get; set; } = string.Empty;
            public BatchStatus Status { get; set; }
            public DateTime LastUpdateTime { get; set; }
            public string Details { get; set; } = string.Empty;
        }

        private class BatchCheckpoint
        {
            public DateTime Timestamp { get; set; }
            public int TotalFiles { get; set; }
            public int CompletedFiles { get; set; }
            public bool IsProcessing { get; set; }
            public Dictionary<string, FileState> FileStates { get; set; } = new();
        }
    }
}