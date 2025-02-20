using System;
using System.Threading.Tasks;
using BatchProcessor.DI.Interfaces.Services; // Provides IResourceCleanUpService
using BatchProcessor.DI.Interfaces.Script;   // Provides IScriptExecutor interface
using Microsoft.Extensions.Logging;

/*
File: BatchProcessor\Core\Logic\Script\ScriptExecutor.cs
Summary: Executes scripts associated with Rhino files during batch processing.
         The ScriptExecutor runs the script specified by its file path for a given Rhino file
         and, upon completion, triggers a global resource cleanup. It implements the IScriptExecutor
         interface to ensure compatibility with dependency injection.
*/

namespace BatchProcessor.Core.Logic.Script
{
    /// <summary>
    /// Responsible for executing scripts associated with Rhino files during batch processing.
    /// This module executes the provided script and, upon completion, triggers a resource cleanup.
    /// Implements the IScriptExecutor interface for integration with dependency injection.
    /// </summary>
    public class ScriptExecutor : IScriptExecutor
    {
        // Service for cleaning up temporary resources.
        private readonly IResourceCleanUpService _resourceCleanUpService;
        // Logger for diagnostic messages.
        private readonly ILogger<ScriptExecutor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptExecutor"/> class.
        /// </summary>
        /// <param name="resourceCleanUpService">
        /// The resource cleanup service used to manage and clean up temporary resources.
        /// </param>
        /// <param name="logger">Logger for diagnostic messages.</param>
        public ScriptExecutor(IResourceCleanUpService resourceCleanUpService, ILogger<ScriptExecutor> logger)
        {
            _resourceCleanUpService = resourceCleanUpService ?? throw new ArgumentNullException(nameof(resourceCleanUpService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the script for the specified Rhino file.
        /// </summary>
        /// <param name="scriptPath">
        /// The full file path of the script to execute.
        /// </param>
        /// <param name="rhinoFile">
        /// The name of the Rhino file being processed.
        /// </param>
        /// <returns>
        /// A task that returns a tuple:
        /// (bool success, string details) where <c>success</c> indicates whether the script execution succeeded,
        /// and <c>details</c> contains any message or error details.
        /// </returns>
        public async Task<(bool success, string details)> ExecuteScript(string scriptPath, string rhinoFile)
        {
            try
            {
                // Insert actual script execution logic here.
                // Simulate asynchronous work using Task.Delay.
                await Task.Delay(100);

                _logger.LogInformation("Script executed successfully for Rhino file '{rhinoFile}' using script at '{scriptPath}'.", rhinoFile, scriptPath);

                // Trigger global resource cleanup after execution.
                await _resourceCleanUpService.CleanUpAsync();

                return (true, "Script executed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing script for Rhino file '{rhinoFile}' using script at '{scriptPath}'.", rhinoFile, scriptPath);
                return (false, $"Error executing script: {ex.Message}");
            }
        }
    }
}
