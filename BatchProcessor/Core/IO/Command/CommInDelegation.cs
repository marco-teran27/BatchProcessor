using System;
using System.Threading;
using System.Threading.Tasks;
using BatchProcessor.DI.Interfaces.AbsRhino;
using BatchProcessor.DI.Interfaces.Logging;
using BatchProcessor.Core.Services;

namespace BatchProcessor.Core.IO.Command
{
    /// <summary>
    /// ==============================================================================
    /// SUMMARY:
    ///   This class was previously 'CommLineInput'. 
    ///   Now, it focuses solely on:
    ///     1) Receiving the "start" call from CommLineCommand
    ///     2) Spinning up a CancellationToken
    ///     3) Delegating the entire config parse/validate/batch flow 
    ///        to 'TheOrchestrator'
    ///     4) Returning success/failure
    ///   It no longer does partial JSON reading or business logic.
    /// ==============================================================================
    /// </summary>
    public class CommInDelegation : ICommInDelegation
    {
        private readonly TheOrchestrator _orchestrator;
        private readonly ICommLineOut _output;

        private bool _isActive;
        private CancellationTokenSource? _cancellationSource;

        /// <summary>
        /// Constructor injecting TheOrchestrator and ICommLineOut. 
        /// TheOrchestrator handles the actual config->batch process flow, 
        /// while ICommLineOut writes messages to Rhino's command line.
        /// </summary>
        /// <param name="orchestrator">The orchestrator that does config->batch logic.</param>
        /// <param name="output">Command line output interface.</param>
        public CommInDelegation(
            TheOrchestrator orchestrator,
            ICommLineOut output)
        {
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        /// <summary>
        /// Whether a command is currently active (useful to prevent double starts).
        /// </summary>
        public bool IsCommandActive => _isActive;

        /// <summary>
        /// Initiates the command:
        ///   1) Creates a new CancellationTokenSource
        ///   2) Calls TheOrchestrator to do the config parse/validation/batch flow
        ///   3) Returns true if successful, false if cancelled or invalid
        /// </summary>
        public async Task<bool> InitiateCommand()
        {
            if (_isActive)
            {
                _output.ShowError("Batch processor is already running");
                return false;
            }

            _isActive = true;
            _cancellationSource = new CancellationTokenSource();

            try
            {
                bool success = await _orchestrator.RunFullBatchProcessAsync(_cancellationSource.Token);
                return success;
            }
            finally
            {
                _isActive = false;
                _cancellationSource?.Dispose();
                _cancellationSource = null;
            }
        }

        /// <summary>
        /// Cancels the current batch by triggering the CancellationTokenSource.
        /// </summary>
        public void HandleCancellation()
        {
            if (_isActive)
            {
                _output.ShowMessage("Cancelling batch...");
                _cancellationSource?.Cancel();
            }
        }
    }
}
