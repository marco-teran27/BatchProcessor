// File: Batch\Core\TheOrchestrator.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Commons; // For ITheOrchestrator, IRhinoCommOut
using Commons.Interfaces;
using ConfigJSON; // For ConfigSelector, ConfigParser

namespace Batch.Core
{
    public class TheOrchestrator : ITheOrchestrator
    {
        private readonly ConfigSelector _selector;
        private readonly ConfigParser _parser;
        private readonly IRhinoCommOut _rhino;

        public TheOrchestrator(ConfigSelector selector, ConfigParser parser, IRhinoCommOut rhino)
        {
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _rhino = rhino ?? throw new ArgumentNullException(nameof(rhino));
        }

        public async Task<bool> RunBatchAsync(string? configPath, CancellationToken ct)
        {
            try
            {
                _rhino.ShowMessage("Starting BatchProcessor...");
                configPath ??= _selector.SelectConfigFile();
                if (string.IsNullOrEmpty(configPath) || ct.IsCancellationRequested)
                {
                    _rhino.ShowError("Configuration selection canceled.");
                    return false;
                }
                var config = await _parser.ParseConfigAsync(configPath); // Instance method
                _rhino.ShowMessage($"Config parsed from {config.FilePath}");
                return true;
            }
            catch (Exception ex)
            {
                _rhino.ShowError($"Config pipeline failed: {ex.Message}");
                return false;
            }
        }
    }
}