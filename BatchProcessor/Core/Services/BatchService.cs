using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BatchProcessor.Core.Config.Models;    // Contains DirectorySettings, ScriptSettings, ReprocessSettings, TimeOutSettings, BatchResults, FileProcessingResult, ProcessingMetrics, BatchStatus
using BatchProcessor.DI.Interfaces.Batch;
using BatchProcessor.DI.Interfaces.Services;  // For IBatchService, IResourceCleanUpService
using BatchProcessor.Core.Models;
using BatchProcessor.DI.Interfaces.AbsRhino; // For ICommLineOut
using BatchProcessor.Core.IO.Batch;           // For BatchDirScanner
using BatchProcessor.Core.Logic.Batch;        // For BatchFileOpen, BatchFileEnd, BatchKill, BatchState
using BatchProcessor.Core.Logic.Error;        // For ErrorManager (logic version)
using BatchProcessor.Core.Services;           // For BatchMetricsService
using Microsoft.Extensions.Logging;
using BatchProcessor.DI.Interfaces.Error;   // For IErrorManager

/// <summary>
/// BatchService coordinates the processing of Rhino files in a batch operation.
/// 
/// Responsibilities include:
///   1. Scanning the input directory (using BatchDirScanner) for Rhino (.3dm) files.
///   2. Initializing batch state and starting overall metrics collection via BatchMetricsService.
///   3. Processing each file:
///         - Opening the file.
///         - Executing file processing (e.g. running a script).
///         - Measuring per‑file processing time locally.
///         - Logging per‑file results and handling errors.
///   4. Stopping overall metrics collection and aggregating batch metrics via BatchMetricsService.
///   5. Returning a BatchResults object that includes individual file results and overall statistics.
/// 
/// This updated version integrates IResourceCleanUpService to perform resource cleanup
/// after processing completes (whether the batch finishes normally or is cancelled),
/// ensuring that the plugin resets its state before a new run.
/// 
/// Cancellation is checked at each iteration to allow for prompt termination.
/// </summary>
namespace BatchProcessor.Core.Services
{
    public class BatchService : IBatchService
    {
        private readonly ILogger<BatchService> _logger;
        private readonly ICommLineOut _output;
        private readonly IErrorManager _errormanager;
        private readonly BatchDirScanner _dirScanner;
        private readonly BatchFileOpen _fileOpener;
        private readonly BatchFileEnd _fileCloser;
        private readonly BatchKill _killSwitch;
        private readonly BatchState _state;
        private readonly IResourceCleanUpService _resourceCleanUpService;
        private readonly BatchMetricsService _metrics;

        /// <summary>
        /// Initializes a new instance of BatchService.
        /// </summary>
        public BatchService(
            ILogger<BatchService> logger,
            ICommLineOut output,
            BatchDirScanner dirScanner,
            BatchFileOpen fileOpener,
            BatchFileEnd fileCloser,
            BatchKill killSwitch,
            BatchState state,
            IErrorManager errorManager,
            IResourceCleanUpService resourceCleanUpService,
            BatchMetricsService metrics)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _dirScanner = dirScanner ?? throw new ArgumentNullException(nameof(dirScanner));
            _fileOpener = fileOpener ?? throw new ArgumentNullException(nameof(fileOpener));
            _fileCloser = fileCloser ?? throw new ArgumentNullException(nameof(fileCloser));
            _killSwitch = killSwitch ?? throw new ArgumentNullException(nameof(killSwitch));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _errormanager = errorManager ?? throw new ArgumentNullException(nameof(errorManager));
            _resourceCleanUpService = resourceCleanUpService ?? throw new ArgumentNullException(nameof(resourceCleanUpService));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }

        /// <summary>
        /// Processes Rhino files in a batch operation.
        /// Steps:
        /// 1. Scan the input directory for Rhino files.
        /// 2. Initialize batch state and start metrics collection.
        /// 3. For each file:
        ///    - Check for cancellation.
        ///    - Open the file and process it.
        ///    - Log results and errors.
        /// 4. Stop metrics collection and perform resource cleanup (in a finally block).
        /// 5. Aggregate metrics and determine overall batch status.
        /// </summary>
        /// <param name="directorySettings">Settings for file directories.</param>
        /// <param name="scriptSettings">Settings for the script to run.</param>
        /// <param name="reprocessSettings">Settings for reprocessing.</param>
        /// <param name="timeoutSettings">Timeout settings for processing.</param>
        /// <param name="cancellationToken">Token to observe cancellation requests.</param>
        /// <returns>A BatchResults object containing processing outcomes.</returns>
        public async Task<BatchResults> ProcessFilesAsync(
            DirectorySettings directorySettings,
            ScriptSettings scriptSettings,
            ReprocessSettings reprocessSettings,
            TimeOutSettings timeoutSettings,
            CancellationToken cancellationToken)
        {
            // Initialize overall batch results.
            var results = new BatchResults
            {
                StartTime = DateTime.Now,
                Status = BatchStatus.Running,
                Details = "Batch processing initialized."
            };

            // Scan the input directory for Rhino files (.3dm).
            var foundFiles = _dirScanner.ScanDirectory(directorySettings.FileDir, aggregatedCriteria: null);
            if (!foundFiles.Any())
            {
                results.Status = BatchStatus.Pass;
                results.Details = "No files found for processing.";
                // Trigger resource cleanup before returning.
                await _resourceCleanUpService.CleanUpAsync();
                return results;
            }

            // Initialize batch state and start overall metrics collection.
            _state.InitializeBatch(foundFiles.Count);
            _metrics.StartBatchMetrics();
            _logger.LogInformation($"Starting batch processing of {foundFiles.Count} files.");
            _output.ShowMessage("Starting batch processing.");

            try
            {
                // Process each file.
                foreach (var file in foundFiles)
                {
                    // Check for cancellation at the beginning of each iteration.
                    if (cancellationToken.IsCancellationRequested)
                    {
                        results.WasCancelled = true;
                        _output.ShowMessage("Batch processing cancelled by user.");
                        break;
                    }

                    var startTime = DateTime.Now;

                    // Open the Rhino file.
                    var (openSuccess, doc, openMessage) = _fileOpener.OpenFile(file, directorySettings);
                    if (!openSuccess || doc == null)
                    {
                        results.FileResults.Add(new FileProcessingResult
                        {
                            FileName = file,
                            Status = BatchStatus.Fail,
                            Details = openMessage,
                            StartTime = startTime,
                            EndTime = DateTime.Now,
                            Metrics = new ProcessingMetrics()
                        });
                        continue;
                    }

                    bool success = false;
                    try
                    {
                        _state.UpdateFileState(file, BatchStatus.Running);
                        _output.ShowMessage($"Processing started for file: {file}");

                        // Execute processing (e.g. run the script). Cancellation token is passed to allow early exit.
                        success = await ExecuteProcessing(doc, file, directorySettings, scriptSettings, cancellationToken);
                        _output.ShowMessage($"Processing completed for file: {file}. Success: {success}");
                    }
                    catch (Exception ex)
                    {
                        var errorResponse = await _errormanager.HandleError(file, ex);
                        results.FileResults.Add(new FileProcessingResult
                        {
                            FileName = file,
                            Status = BatchStatus.Fail,
                            Details = errorResponse.ErrorInfo.Message,
                            StartTime = startTime,
                            EndTime = DateTime.Now,
                            Metrics = new ProcessingMetrics()
                        });
                        continue;
                    }
                    finally
                    {
                        // Always close the file, even if errors occur.
                        _fileCloser.CloseFile(doc, file);
                    }

                    // Compute per-file processing metrics.
                    var endTime = DateTime.Now;
                    var processingTime = (endTime - startTime).TotalSeconds;
                    var fileMetrics = new ProcessingMetrics
                    {
                        ProcessingTimeSeconds = processingTime
                    };

                    results.FileResults.Add(new FileProcessingResult
                    {
                        FileName = file,
                        Status = success ? BatchStatus.Pass : BatchStatus.Fail,
                        Details = success ? "Processing completed" : "Processing failed",
                        StartTime = startTime,
                        EndTime = endTime,
                        Metrics = fileMetrics
                    });
                }
            }
            finally
            {
                // Stop overall metrics collection.
                _metrics.StopBatchMetrics();

                // Trigger resource cleanup regardless of outcome.
                await _resourceCleanUpService.CleanUpAsync();
                _output.ShowMessage("Resource cleanup completed after batch processing.");
            }

            // Aggregate overall batch metrics.
            var batchSummary = _metrics.AggregateMetrics();
            results.TotalFiles = batchSummary.TotalFiles;
            results.SuccessfulFiles = batchSummary.CompletedFiles;
            results.FailedFiles = batchSummary.FailedFiles;

            // Determine overall batch status.
            results.Status = results.WasCancelled ? BatchStatus.Cancelled :
                             (results.FailedFiles > 0 ? BatchStatus.Fail : BatchStatus.Pass);
            _output.ShowMessage($"Batch completed: {results.SuccessfulFiles} succeeded, {results.FailedFiles} failed.");
            _logger.LogInformation("Batch processing completed.");

            return results;
        }

        /// <summary>
        /// Executes processing for a single Rhino file.
        /// This is a placeholder method to be replaced with actual script execution logic.
        /// </summary>
        /// <param name="doc">The open Rhino document.</param>
        /// <param name="fileName">The name of the file being processed.</param>
        /// <param name="directorySettings">Directory settings for file paths.</param>
        /// <param name="scriptSettings">Script settings specifying the script to execute.</param>
        /// <param name="cancellationToken">Token to observe cancellation requests.</param>
        /// <returns>True if processing succeeds; otherwise, false.</returns>
        private async Task<bool> ExecuteProcessing(
            Rhino.RhinoDoc doc,
            string fileName,
            DirectorySettings directorySettings,
            ScriptSettings scriptSettings,
            CancellationToken cancellationToken)
        {
            // Placeholder: Replace with actual script execution logic.
            await Task.CompletedTask; // Ensures the method is truly asynchronous.
            return true;
        }
    }
}
