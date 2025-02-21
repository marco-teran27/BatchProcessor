using System;
using System.Threading.Tasks;
using BatchProcessor.Core.Metrics.Batch;  // Contains BatchMonitor, BatchCollector, BatchMetricStorage
using Microsoft.Extensions.Logging;

namespace BatchProcessor.Core.Services
{
    /// <summary>
    /// BatchMetricsService is a high-level orchestrator that coordinates the collection,
    /// aggregation, and storage of batch processing metrics.
    /// 
    /// It leverages three underlying components:
    ///   - <see cref="BatchMonitor"/> for real-time tracking of individual file processing events.
    ///   - <see cref="BatchCollector"/> for aggregating per-file metrics into overall batch statistics.
    ///   - <see cref="BatchMetricStorage"/> for persisting the aggregated metrics to disk.
    /// 
    /// This service provides a unified interface so that the batch processing pipeline or an
    /// administrator can:
    ///   • Start and stop live metrics collection,
    ///   • Retrieve the current overall metrics,
    ///   • Aggregate detailed per-file data, and
    ///   • Persist metrics for later review.
    /// </summary>
    public class BatchMetricsService
    {
        private readonly BatchMonitor _batchMonitor;
        private readonly BatchCollector _batchCollector;
        private readonly BatchMetricStorage _batchStorage;
        private readonly ILogger<BatchMetricsService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchMetricsService"/> class.
        /// </summary>
        /// <param name="batchMonitor">
        /// The BatchMonitor instance that provides real-time tracking of file processing.
        /// </param>
        /// <param name="batchCollector">
        /// The BatchCollector instance that aggregates individual file metrics.
        /// </param>
        /// <param name="batchStorage">
        /// The BatchMetricStorage instance that handles persisting aggregated metrics.
        /// </param>
        /// <param name="logger">
        /// The logger for emitting diagnostic messages.
        /// </param>
        public BatchMetricsService(
            BatchMonitor batchMonitor,
            BatchCollector batchCollector,
            BatchMetricStorage batchStorage,
            ILogger<BatchMetricsService> logger)
        {
            _batchMonitor = batchMonitor ?? throw new ArgumentNullException(nameof(batchMonitor));
            _batchCollector = batchCollector ?? throw new ArgumentNullException(nameof(batchCollector));
            _batchStorage = batchStorage ?? throw new ArgumentNullException(nameof(batchStorage));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Starts the batch metrics collection.
        /// 
        /// This method instructs the underlying BatchMonitor to begin tracking the processing
        /// of files in real time.
        /// </summary>
        public void StartBatchMetrics()
        {
            _logger.LogInformation("Starting batch metrics collection.");
            _batchMonitor.StartMonitoring();
        }

        /// <summary>
        /// Stops the batch metrics collection.
        /// 
        /// This method instructs the underlying BatchMonitor to cease tracking file processing.
        /// </summary>
        public void StopBatchMetrics()
        {
            _logger.LogInformation("Stopping batch metrics collection.");
            _batchMonitor.StopMonitoring();
        }

        /// <summary>
        /// Retrieves the current live metrics from the BatchMonitor.
        /// </summary>
        /// <returns>
        /// A <see cref="BatchMonitor.BatchMetrics"/> object containing real-time statistics about the batch.
        /// </returns>
        public BatchMonitor.BatchMetrics GetCurrentMetrics()
        {
            return _batchMonitor.GetCurrentMetrics();
        }

        /// <summary>
        /// Aggregates the collected metrics from all files into a single overall summary.
        /// 
        /// This method calls into the BatchCollector to compute statistics such as average
        /// processing time, total file count, and counts of successful and failed operations.
        /// </summary>
        /// <returns>
        /// A <see cref="BatchCollector.BatchMetricSummary"/> instance representing the aggregated metrics.
        /// </returns>
        public BatchCollector.BatchMetricSummary AggregateMetrics()
        {
            return _batchCollector.GetBatchSummary();
        }

        /// <summary>
        /// Saves the aggregated metrics for a given file.
        /// 
        /// This method instructs the BatchMetricStorage component to persist the metrics for
        /// the specified file. This can be used for logging, auditing, or later review.
        /// </summary>
        /// <param name="fileName">The name of the file whose metrics should be saved.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public async Task SaveMetricsAsync(string fileName)
        {
            _logger.LogInformation($"Saving metrics for file: {fileName}");
            await _batchStorage.SaveMetrics(fileName);
        }
    }
}
