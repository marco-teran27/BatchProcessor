using System;
using System.Collections.Generic;

namespace BatchProcessor.Core.Models
{
    /// <summary>
    /// BatchResults aggregates the final outcome of a batch processing operation.
    /// 
    /// Process Flow Summary:
    ///  1. At the start of the batch processing, the StartTime is recorded.
    ///  2. As each file is processed, an individual FileProcessingResult is created 
    ///     (with details such as file name, processing status, start/end times, and metrics).
    ///  3. These individual results are collected in the FileResults list.
    ///  4. Overall counts (TotalFiles, SuccessfulFiles, FailedFiles) are updated as processing progresses.
    ///  5. When the batch processing completes (or is cancelled), the EndTime is recorded.
    ///  6. The overall BatchStatus (Pass, Fail, or Cancelled) and any additional summary details are set.
    ///  7. The completed BatchResults object is then available for final reporting and logging.
    /// 
    /// This final aggregator simplifies the reporting process by consolidating detailed per‐file
    /// outcomes and overall metrics into one object, which can then be written to a log file
    /// or displayed to the user.
    /// </summary>
    public class BatchResults
    {
        /// <summary>
        /// Gets or sets the start time of the batch processing operation.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the batch processing operation.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the total number of files intended to be processed.
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Gets or sets the number of files that processed successfully.
        /// </summary>
        public int SuccessfulFiles { get; set; }

        /// <summary>
        /// Gets or sets the number of files that failed processing.
        /// </summary>
        public int FailedFiles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the batch processing was cancelled.
        /// </summary>
        public bool WasCancelled { get; set; }

        /// <summary>
        /// Gets or sets additional summary details or messages regarding the batch processing.
        /// </summary>
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the overall status of the batch processing operation.
        /// </summary>
        public BatchStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the list of individual file processing results.
        /// </summary>
        public List<FileProcessingResult> FileResults { get; set; } = new List<FileProcessingResult>();
    }

    /// <summary>
    /// Represents the result of processing an individual file.
    /// </summary>
    public class FileProcessingResult
    {
        /// <summary>
        /// Gets or sets the name of the file processed.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the processing status for the file.
        /// </summary>
        public BatchStatus Status { get; set; }

        /// <summary>
        /// Gets or sets detailed information regarding the file processing outcome.
        /// </summary>
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the start time for processing the file.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time for processing the file.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets processing metrics for the file, such as processing time.
        /// </summary>
        public ProcessingMetrics Metrics { get; set; } = new ProcessingMetrics();
    }

    /// <summary>
    /// Represents possible statuses for a batch or file processing operation.
    /// </summary>
    public enum BatchStatus
    {
        Running,
        Pass,
        Fail,
        Cancelled
    }

    /// <summary>
    /// Represents processing metrics for an individual file or the entire batch.
    /// Additional properties can be added as needed.
    /// </summary>
    public class ProcessingMetrics
    {
        /// <summary>
        /// Gets or sets the processing time in seconds for an individual file.
        /// </summary>
        public double ProcessingTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the total number of files processed in the batch.
        /// This is used in overall batch metrics.
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Gets or sets the number of files processed successfully.
        /// </summary>
        public int SuccessfulFiles { get; set; }

        /// <summary>
        /// Gets or sets the number of files that failed processing.
        /// </summary>
        public int FailedFiles { get; set; }
    }
}
