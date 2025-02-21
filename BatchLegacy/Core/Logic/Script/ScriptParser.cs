using System;
using System.Threading.Tasks;
using BatchProcessor.Core.Config.Models; // For ScriptType
using BatchProcessor.DI.Interfaces.Services; // Provides IResourceCleanUpService
using BatchProcessor.DI.Interfaces.Script;   // Provides IScriptParser interface
using Microsoft.Extensions.Logging;

/*
File: BatchProcessor\Core\Logic\Script\ScriptParser.cs
Summary: Parses script files and prepares them for execution during batch processing.
         This module is responsible for converting raw script content into an executable form,
         formatting script commands based on the script type, and triggering resource cleanup as needed.
         Implements the IScriptParser interface to enable dependency injection and integration
         with other system components.
*/

namespace BatchProcessor.Core.Logic.Script
{
    /// <summary>
    /// Parses script files and prepares them for execution during batch processing.
    /// This module is responsible for converting raw script content into an executable form,
    /// formatting script commands based on the script type, and triggering resource cleanup as needed.
    /// Implements the IScriptParser interface for integration with dependency injection.
    /// </summary>
    public class ScriptParser : IScriptParser
    {
        // Service for cleaning up temporary resources during script parsing.
        private readonly IResourceCleanUpService _resourceCleanUpService;
        // Logger for diagnostic messages.
        private readonly ILogger<ScriptParser> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptParser"/> class.
        /// </summary>
        /// <param name="resourceCleanUpService">
        /// The resource cleanup service used to manage temporary resources during script parsing.
        /// </param>
        /// <param name="logger">Logger for diagnostic messages.</param>
        public ScriptParser(IResourceCleanUpService resourceCleanUpService, ILogger<ScriptParser> logger)
        {
            _resourceCleanUpService = resourceCleanUpService ?? throw new ArgumentNullException(nameof(resourceCleanUpService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Parses the given script content asynchronously.
        /// Note: Any call to track individual files (via TrackFile) has been removed.
        /// </summary>
        /// <param name="scriptContent">The raw content of the script.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, returning true if parsing was successful; otherwise, false.
        /// </returns>
        public async Task<bool> ParseAsync(string scriptContent)
        {
            try
            {
                // Insert actual script parsing logic here.
                // Legacy file tracking (TrackFile) has been removed.
                await Task.Delay(50); // Simulate asynchronous work.
                _logger.LogInformation("Script parsed successfully.");

                // Optionally trigger resource cleanup after parsing.
                await _resourceCleanUpService.CleanUpAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing script.");
                return false;
            }
        }

        /// <summary>
        /// Formats a script command based on script type.
        /// </summary>
        /// <param name="scriptPath">Path to the script file.</param>
        /// <param name="type">Type of script (Python, Grasshopper, etc.).</param>
        /// <returns>
        /// Formatted command string ready for execution.
        /// </returns>
        public string FormatScriptCommand(string scriptPath, ScriptType type)
        {
            // Implement script command formatting logic based on script type.
            // For demonstration purposes, we'll use a simple switch-case.
            switch (type)
            {
                case ScriptType.Python:
                    return $"python \"{scriptPath}\"";
                case ScriptType.Grasshopper:
                    return $"grasshopper -run \"{scriptPath}\"";
                default:
                    // Default formatting if no specific type matches.
                    return $"execute \"{scriptPath}\"";
            }
        }
    }
}
