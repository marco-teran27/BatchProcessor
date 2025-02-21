// File: Batch\Core\TheOrchestrator.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using ConfigJSON;
using Commons.Interfaces; // Updated from BatchProcessor.DI.Interfaces

namespace Batch
{
    /// <summary>
    /// Coordinates the configuration pipeline for batch processing.
    /// </summary>
    public class TheOrchestrator : ITheOrchestrator
    {
        private readonly ConfigSelector _selector;
        private readonly ConfigParser _parser;
        private readonly IRhinoCommOut _rhino;

        /// <summary>
        /// Initializes a new instance of TheOrchestrator.
        /// </summary>
        public TheOrchestrator(ConfigSelector selector, ConfigParser parser, IRhinoCommOut rhino)
        {
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _rhino = rhino ?? throw new ArgumentNullException(nameof(rhino));
        }

        /// <summary>
        /// Runs the configuration pipeline asynchronously.
        /// </summary>
        public async Task<bool> RunBatchAsync(string? configPath, CancellationToken ct)
        {
            try
            {
                _rhino.ShowMessage("Starting BatchProcessor...");

                // Step 3: Select config file
                configPath ??= _selector.SelectConfigFile();
                if (string.IsNullOrEmpty(configPath) || ct.IsCancellationRequested)
                {
                    _rhino.ShowError("Configuration selection canceled.");
                    return false;
                }

                // Step 4: Parse config
                var config = _parser.ParseConfig(configPath);
                _rhino.ShowMessage($"Config parsed from {config.FilePath}");
                return true; // Validation deferred to next section
            }
            catch (Exception ex)
            {
                _rhino.ShowError($"Config pipeline failed: {ex.Message}");
                return false;
            }
        }
    }
}