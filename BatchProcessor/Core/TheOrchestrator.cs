// File: BatchProcessor\Core\TheOrchestrator.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using BatchProcessor.DI.Interfaces;
using ConfigJSON;

namespace BatchProcessor.Core
{
    /// <summary>
    /// Coordinates the configuration pipeline for batch processing.
    /// </summary>
    public class TheOrchestrator : IBatchOrchestrator
    {
        private readonly ConfigSelector _selector;
        private readonly ConfigParser _parser;
        private readonly IRhinoIntegration _rhino;

        /// <summary>
        /// Initializes a new instance of TheOrchestrator.
        /// </summary>
        /// <param name="selector">Config file selector.</param>
        /// <param name="parser">Config file parser.</param>
        /// <param name="rhino">Rhino integration interface.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        public TheOrchestrator(ConfigSelector selector, ConfigParser parser, IRhinoIntegration rhino)
        {
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _rhino = rhino ?? throw new ArgumentNullException(nameof(rhino));
        }

        /// <summary>
        /// Runs the configuration pipeline asynchronously.
        /// </summary>
        /// <param name="configPath">Optional config file path; if null, prompts user.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>True if successful; false otherwise.</returns>
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