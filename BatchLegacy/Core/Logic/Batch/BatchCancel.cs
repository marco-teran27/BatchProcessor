using System;
using System.Threading;
using System.Threading.Tasks;
using BatchProcessor.DI.Interfaces.AbsRhino;
using BatchProcessor.DI.Interfaces.Logging;
using BatchProcessor.DI.Interfaces.Services; // Provides IResourceCleanUpService
using Microsoft.Extensions.Logging;
using BatchProcessor.Core.Models;

namespace BatchProcessor.Core.Logic.Batch
{
    /// <summary>
    /// BatchCancel manages cancellation of the batch process.
    /// 
    /// This module monitors for the ESC key (via RhinoEscapeKeyHandler) and, upon detection,
    /// logs the cancellation event to both the Rhino command line (through ICommLineOut) and the batch logger.
    /// It then signals cancellation to the entire workflow by canceling the associated CancellationTokenSource
    /// and invokes the ResourceCleanupService to perform necessary cleanup. Only one ESC monitor is active.
    /// </summary>
    public class BatchCancel : IDisposable
    {
        private readonly ICommLineOut _output;
        private readonly IBatchLogger _logger;
        private readonly ILogger<BatchCancel> _loggerService;
        private readonly IResourceCleanUpService _resourceCleanUpService;
        private CancellationTokenSource? _cancellationSource;
        private readonly RhinoEscapeKeyHandler _escapeHandler;
        private bool _isCancellationRequested;
        private bool _disposed;
        private string? _currentFile;

        /// <summary>
        /// Initializes a new instance of BatchCancel.
        /// Injects ICommLineOut, IBatchLogger, ILogger, and IResourceCleanUpService.
        /// </summary>
        /// <param name="output">The command line output interface.</param>
        /// <param name="logger">The batch logger instance.</param>
        /// <param name="loggerService">The ILogger for BatchCancel diagnostics.</param>
        /// <param name="resourceCleanUpService">Service to clean up resources after cancellation.</param>
        public BatchCancel(ICommLineOut output, IBatchLogger logger, ILogger<BatchCancel> loggerService, IResourceCleanUpService resourceCleanUpService)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
            _resourceCleanUpService = resourceCleanUpService ?? throw new ArgumentNullException(nameof(resourceCleanUpService));
            _escapeHandler = new RhinoEscapeKeyHandler();
        }

        /// <summary>
        /// Initializes cancellation monitoring and starts ESC key monitoring.
        /// Returns a CancellationToken to be used by the overall workflow.
        /// </summary>
        public CancellationToken InitializeCancellation()
        {
            _cancellationSource?.Dispose();
            _cancellationSource = new CancellationTokenSource();
            _isCancellationRequested = false;
            _currentFile = null;

            // Start ESC key monitoring
            _escapeHandler.Start();

            // Start a single background task to monitor the ESC key.
            Task.Run(async () =>
            {
                try
                {
                    while (!_isCancellationRequested && _cancellationSource != null)
                    {
                        if (_escapeHandler.WasEscapePressed())
                        {
                            await PerformCancellationAsync();
                            break;
                        }
                        await Task.Delay(100);
                    }
                }
                catch (Exception ex)
                {
                    _loggerService.LogError(ex, "Error in ESC key monitoring");
                }
            }, _cancellationSource.Token);

            return _cancellationSource.Token;
        }

        /// <summary>
        /// Updates the current file being processed.
        /// </summary>
        /// <param name="fileName">Name of the file currently being processed.</param>
        public void UpdateCurrentFile(string fileName)
        {
            _currentFile = fileName;
        }

        /// <summary>
        /// Performs cancellation of the batch process.
        /// Logs the cancellation event to both the Rhino command line and batch logger,
        /// signals cancellation to the workflow, and calls the ResourceCleanupService.
        /// </summary>
        private async Task PerformCancellationAsync()
        {
            if (_isCancellationRequested)
            {
                _output.ShowMessage("Cancellation already in progress...");
                return;
            }
            _isCancellationRequested = true;

            string message = _currentFile != null
                ? $"Cancellation requested while processing {_currentFile}."
                : "Cancellation requested.";

            // Log cancellation event to command line and batch logger.
            _output.ShowMessage(message);
            await _logger.LogProcessingStatus(_currentFile ?? "Batch", BatchStatus.Cancelled, "Processing cancelled by user", null, null, null);
            await _logger.LogCancellation();

            // Signal cancellation to the workflow.
            try
            {
                _cancellationSource?.Cancel();
                _output.ShowMessage("Cancellation signal sent successfully.");
            }
            catch (Exception ex)
            {
                _output.ShowError($"Cancellation signal failed: {ex.Message}");
            }

            // Perform resource cleanup.
            try
            {
                await _resourceCleanUpService.CleanUpAsync();
                _output.ShowMessage("Resource cleanup completed.");
            }
            catch (Exception ex)
            {
                _output.ShowError($"Resource cleanup failed: {ex.Message}");
            }

            // Stop ESC key monitoring.
            _escapeHandler.Stop();
        }

        /// <summary>
        /// Public method to explicitly request cancellation.
        /// This method triggers the unified ESC cancellation pathway.
        /// </summary>
        public void RequestCancellation()
        {
            // Fire and forget the cancellation process.
            Task.Run(async () => await PerformCancellationAsync());
        }

        /// <summary>
        /// Handles final cancellation processing.
        /// Logs final cancellation status and ensures ESC monitoring is stopped.
        /// </summary>
        public async Task HandleCancellation()
        {
            if (_currentFile != null)
            {
                await _logger.LogProcessingStatus(_currentFile, BatchStatus.Cancelled, "Processing cancelled by user", null, null, null);
            }
            _output.ShowMessage("Batch processing cancelled");
            await _logger.LogCancellation();

            // Ensure ESC key monitoring is stopped.
            _escapeHandler.Stop();
        }

        /// <summary>
        /// Disposes of resources used by BatchCancel.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected disposal method.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _escapeHandler.Dispose();
                    _cancellationSource?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
