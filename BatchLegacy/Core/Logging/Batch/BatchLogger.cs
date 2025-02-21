using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using BatchProcessor.DI.Interfaces.Logging;   // IBatchLogger
using BatchProcessor.DI.Interfaces.AbsRhino;    // ICommLineOut
using BatchProcessor.Core.Models;                // BatchResults, FileProcessingResult, ProcessingMetrics, BatchStatus
using Microsoft.Extensions.Logging;

namespace BatchProcessor.Core.Logging.Batch
{
    /// <summary>
    /// BatchLogger implements IBatchLogger and is responsible for logging batch processing events.
    /// It records events for file processing, status updates, errors, and cancellation,
    /// and later aggregates these events into overall batch statistics.
    /// </summary>
    public class BatchLogger : IBatchLogger
    {
        private readonly ICommLineOut _output;
        private readonly ILogger<BatchLogger> _logger;
        private readonly ConcurrentQueue<BatchEvent> _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchLogger"/> class.
        /// </summary>
        /// <param name="output">Interface for command-line output.</param>
        /// <param name="logger">Logger instance for diagnostic messages.</param>
        public BatchLogger(ICommLineOut output, ILogger<BatchLogger> logger)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _events = new ConcurrentQueue<BatchEvent>();
        }

        /// <summary>
        /// Logs that a file was processed.
        /// </summary>
        /// <param name="fileName">The file name processed.</param>
        /// <param name="success">True if processing succeeded; otherwise, false.</param>
        /// <param name="details">Additional details about the outcome.</param>
        /// <param name="processingMode">The processing mode (e.g., "ALL").</param>
        /// <param name="metrics">Optional processing metrics.</param>
        /// <param name="processingStartTime">Optional processing start time.</param>
        /// <param name="processingEndTime">Optional processing end time.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogFileProcessed(string fileName, bool success, string details, string processingMode, ProcessingMetrics? metrics, DateTime? processingStartTime, DateTime? processingEndTime)
        {
            var batchEvent = new BatchEvent
            {
                Type = EventType.FileProcessed,
                Timestamp = DateTime.Now,
                FileName = fileName,
                Success = success,
                Details = details,
                Metrics = metrics,
                ProcessingStartTime = processingStartTime,
                ProcessingEndTime = processingEndTime
            };

            _events.Enqueue(batchEvent);
            _logger.LogInformation($"File '{fileName}' processed (mode: {processingMode}): Success={success}, Details={details}");
            _output.ShowMessage($"File '{fileName}' processed: {success}");
            await Task.CompletedTask; // Ensures asynchronous signature.
        }

        /// <summary>
        /// Logs a processing status update for a file.
        /// </summary>
        /// <param name="fileName">The file name for which the status update applies.</param>
        /// <param name="status">The overall status of processing (e.g., Pass, Fail).</param>
        /// <param name="details">Additional details regarding the status update.</param>
        /// <param name="metrics">Optional processing metrics.</param>
        /// <param name="processingStartTime">Optional processing start time.</param>
        /// <param name="processingEndTime">Optional processing end time.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogProcessingStatus(string fileName, BatchStatus status, string details, ProcessingMetrics? metrics, DateTime? processingStartTime, DateTime? processingEndTime)
        {
            var batchEvent = new BatchEvent
            {
                Type = EventType.StatusUpdate,
                Timestamp = DateTime.Now,
                FileName = fileName,
                Status = status,
                Details = details,
                Metrics = metrics,
                ProcessingStartTime = processingStartTime,
                ProcessingEndTime = processingEndTime
            };

            _events.Enqueue(batchEvent);
            _logger.LogInformation($"Status update for '{fileName}': {status}, Details: {details}");
            _output.ShowMessage($"Status update for '{fileName}': {status}");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Logs an error that occurred during processing.
        /// </summary>
        /// <param name="fileName">The name of the file for which the error occurred.</param>
        /// <param name="ex">The exception associated with the error.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogError(string fileName, Exception ex)
        {
            var batchEvent = new BatchEvent
            {
                Type = EventType.Error,
                Timestamp = DateTime.Now,
                FileName = fileName,
                Details = ex.Message,
                Success = false,
                Metrics = null,
                ProcessingStartTime = null,
                ProcessingEndTime = null,
                Status = null
            };

            _events.Enqueue(batchEvent);
            _logger.LogError(ex, $"Error processing {fileName}: {ex.Message}");
            _output.ShowError($"Error processing {fileName}: {ex.Message}");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Logs that the batch processing was cancelled.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogCancellation()
        {
            var batchEvent = new BatchEvent
            {
                Type = EventType.SystemEvent,
                Timestamp = DateTime.Now,
                FileName = "Batch",
                Details = "Batch processing cancelled"
            };

            _events.Enqueue(batchEvent);
            _logger.LogWarning("Batch processing cancelled");
            _output.ShowMessage("Batch processing cancelled");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Aggregates the logged events and returns overall batch statistics.
        /// </summary>
        /// <returns>A <see cref="BatchResults"/> object representing the aggregated batch statistics.</returns>
        public BatchResults GetStatistics()
        {
            // Filter for file processed events.
            var fileEvents = _events.Where(e => e.Type == EventType.FileProcessed).ToList();

            var results = new BatchResults
            {
                StartTime = fileEvents.FirstOrDefault()?.Timestamp ?? DateTime.Now,
                EndTime = fileEvents.LastOrDefault()?.Timestamp ?? DateTime.Now,
                TotalFiles = fileEvents.Count,
                SuccessfulFiles = fileEvents.Count(e => e.Success),
                FailedFiles = fileEvents.Count(e => !e.Success),
                WasCancelled = _events.Any(e => e.Type == EventType.SystemEvent && e.Details.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0),
                Details = "Aggregated batch processing statistics",
                FileResults = fileEvents.Select(e => new FileProcessingResult
                {
                    FileName = e.FileName ?? string.Empty,
                    Status = e.Success ? BatchStatus.Pass : BatchStatus.Fail,
                    Details = e.Details,
                    StartTime = e.ProcessingStartTime ?? DateTime.MinValue,
                    EndTime = e.ProcessingEndTime ?? DateTime.MinValue,
                    Metrics = e.Metrics ?? new ProcessingMetrics()
                }).ToList()
            };

            return results;
        }
    }

    /// <summary>
    /// Defines types of events logged during batch processing.
    /// </summary>
    public enum EventType
    {
        FileProcessed,
        StatusUpdate,
        Error,
        SystemEvent
    }

    /// <summary>
    /// Represents a log event captured during batch processing.
    /// </summary>
    public class BatchEvent
    {
        /// <summary>
        /// Gets or sets the type of event.
        /// </summary>
        public EventType Type { get; set; }
        /// <summary>
        /// Gets or sets the timestamp when the event was logged.
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Gets or sets the associated file name (if applicable).
        /// </summary>
        public string? FileName { get; set; }
        /// <summary>
        /// For file processed events, indicates whether processing was successful.
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Gets or sets additional details about the event.
        /// </summary>
        public string Details { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets optional processing metrics.
        /// </summary>
        public ProcessingMetrics? Metrics { get; set; }
        /// <summary>
        /// Gets or sets the processing start time for the event.
        /// </summary>
        public DateTime? ProcessingStartTime { get; set; }
        /// <summary>
        /// Gets or sets the processing end time for the event.
        /// </summary>
        public DateTime? ProcessingEndTime { get; set; }
        /// <summary>
        /// For status update events, holds the current status.
        /// </summary>
        public BatchStatus? Status { get; set; }
    }
}
