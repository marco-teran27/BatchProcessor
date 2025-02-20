using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BatchProcessor.Core.Config.Models;       // Contains ProjectName, DirectorySettings, PIDSettings, etc.
using BatchProcessor.DI.Interfaces.Config;    // For IConfigParser, IConfigValidator, IConfigSelUI
using BatchProcessor.DI.Interfaces.AbsRhino;   // For ICommLineOut
using BatchProcessor.DI.Interfaces.Batch;
using BatchProcessor.DI.Interfaces.Services;  // For IResourceCleanUpService

/// <summary>
/// TheOrchestrator coordinates the overall batch workflow:
/// 1. Prompts the user (via IConfigSelUI) to select a configuration file.
/// 2. Parses the configuration using IConfigParser.
/// 3. Validates the configuration via IConfigValidator.
/// 4. Executes the batch process with IBatchService.
/// 5. Triggers resource cleanup via IResourceCleanUpService after processing completes.
/// 
/// Cancellation is checked between steps.
/// </summary>
namespace BatchProcessor.Core.Services
{
    public class TheOrchestrator
    {
        private readonly IConfigSelUI _configSelector;
        private readonly IConfigParser _configParser;
        private readonly IConfigValidator _configValidator;
        private readonly IBatchService _batchService;
        private readonly ICommLineOut _output;
        private readonly IResourceCleanUpService _cleanupService;

        /// <summary>
        /// Initializes a new instance of TheOrchestrator.
        /// </summary>
        /// <param name="configSelector">Handles configuration file selection.</param>
        /// <param name="configParser">Parses the configuration file into a configuration structure.</param>
        /// <param name="configValidator">Validates the configuration settings.</param>
        /// <param name="batchService">Executes the batch process when configuration is valid.</param>
        /// <param name="output">Provides command line output for messages and errors.</param>
        /// <param name="cleanupService">Service for cleaning up resources after batch processing.</param>
        public TheOrchestrator(
            IConfigSelUI configSelector,
            IConfigParser configParser,
            IConfigValidator configValidator,
            IBatchService batchService,
            ICommLineOut output,
            IResourceCleanUpService cleanupService)
        {
            _configSelector = configSelector ?? throw new ArgumentNullException(nameof(configSelector));
            _configParser = configParser ?? throw new ArgumentNullException(nameof(configParser));
            _configValidator = configValidator ?? throw new ArgumentNullException(nameof(configValidator));
            _batchService = batchService ?? throw new ArgumentNullException(nameof(batchService));
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _cleanupService = cleanupService ?? throw new ArgumentNullException(nameof(cleanupService));
        }

        /// <summary>
        /// Runs the full batch processing workflow.
        /// Steps:
        /// 1. Prompt the user to select a configuration file.
        /// 2. Parse and validate the configuration.
        /// 3. Execute the batch process.
        /// 4. Trigger resource cleanup.
        /// Cancellation is checked after each major step.
        /// </summary>
        /// <param name="cancellationToken">Token to observe cancellation requests.</param>
        /// <returns>True if batch processing completed successfully (and was not cancelled); otherwise, false.</returns>
        public async Task<bool> RunFullBatchProcessAsync(CancellationToken cancellationToken)
        {
            // Step 1: Prompt the user to select a configuration file.
            var configPath = _configSelector.SelectConfigurationFile();
            if (string.IsNullOrEmpty(configPath))
            {
                _output.ShowMessage("No configuration file selected.");
                return false;
            }
            // Check for cancellation immediately after file selection.
            if (cancellationToken.IsCancellationRequested)
            {
                _output.ShowMessage("Batch processing cancelled during configuration file selection.");
                return false;
            }

            // Step 2: Parse the configuration file.
            var (configRoot, parseErrors) = _configParser.ParseConfigFile(configPath);
            if (configRoot == null)
            {
                foreach (var error in parseErrors)
                {
                    _output.ShowError(error);
                }
                return false;
            }
            // Check cancellation after parsing.
            if (cancellationToken.IsCancellationRequested)
            {
                _output.ShowMessage("Batch processing cancelled after parsing configuration.");
                return false;
            }

            // Step 3: Validate the configuration.
            var projectNameObj = new ProjectName { Name = configRoot.ProjectName };
            // Set the actual config file name for validation.
            projectNameObj.ActualConfigFileName = Path.GetFileName(configPath);

            var validationResult = _configValidator.ValidateConfig(
                projectNameObj,
                configRoot.Directories,
                configRoot.Pid_Settings,
                configRoot.Rhino_File_Name_Settings,
                configRoot.Script_Settings,
                configRoot.Reprocess_Settings,
                configRoot.Timeout_Minutes
            );
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    _output.ShowError(error);
                }
                return false;
            }
            // Check cancellation after validation.
            if (cancellationToken.IsCancellationRequested)
            {
                _output.ShowMessage("Batch processing cancelled after configuration validation.");
                return false;
            }

            // Step 4: Execute the batch process.
            var result = await _batchService.ProcessFilesAsync(
                configRoot.Directories,
                configRoot.Script_Settings,
                configRoot.Reprocess_Settings,
                configRoot.Timeout_Minutes,
                cancellationToken
            );

            // Step 5: Trigger resource cleanup after batch process completion.
            await _cleanupService.CleanUpAsync();
            _output.ShowMessage("Resource cleanup completed after batch processing.");

            // Return true if the batch was not cancelled.
            return !result.WasCancelled;
        }
    }
}
