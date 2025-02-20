using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BatchProcessor.Core.Config.Models;       // Contains ProjectName, DirectorySettings, PIDSettings, RhinoFileNameSettings, ScriptSettings, ReprocessSettings, TimeOutSettings
using BatchProcessor.DI.Interfaces.Config;    // For IConfigValidator, IConfigParser, IConfigSelUI
using BatchProcessor.DI.Interfaces.AbsRhino;   // For ICommLineOut
using Microsoft.Extensions.Logging;
using BatchProcessor.Core.Config.Validation;

namespace BatchProcessor.Core.Services
{
    /// <summary>
    /// BatchConfig is responsible for loading, validating, and managing the batch processing configuration.
    /// 
    /// In this updated version the configuration file is expected to contain all fields:
    /// ProjectName, Directories, PID_Settings, Rhino_File_Name_Settings, Script_Settings, Reprocess_Settings, and Timeout_Minutes.
    /// Validation is performed by calling IConfigValidator.ValidateConfig with all seven parameters.
    /// However, since the batch processing pipeline uses only DirectorySettings, ScriptSettings, ReprocessSettings, and TimeOutSettings,
    /// this class returns (or saves) only that subset.
    /// 
    /// Note: For update (and save) operations the method only receives the subset of settings.
    /// Therefore, default (dummy) values for ProjectName, PIDSettings, and RhinoFileNameSettings are supplied when calling the validator.
    /// </summary>
    public class BatchConfig
    {
        private readonly IConfigValidator _validator;
        private readonly ILogger<BatchConfig> _logger;
        private readonly ICommLineOut _output;
        private readonly IConfigParser _configParser;

        /// <summary>
        /// Initializes a new instance of the BatchConfig class.
        /// </summary>
        /// <param name="validator">The configuration validator instance.</param>
        /// <param name="logger">The logger for diagnostic messages.</param>
        /// <param name="output">The command line output interface.</param>
        /// <param name="configParser">The configuration file parser.</param>
        public BatchConfig(
            IConfigValidator validator,
            ILogger<BatchConfig> logger,
            ICommLineOut output,
            IConfigParser configParser)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _configParser = configParser ?? throw new ArgumentNullException(nameof(configParser));
        }

        /// <summary>
        /// Loads and validates the batch configuration from the specified file.
        /// 
        /// The configuration file must contain all the required fields (including ProjectName, PID_Settings, and Rhino_File_Name_Settings).
        /// This method creates a ProjectName instance from the parsed configuration and calls ValidateConfig with all seven parameters.
        /// If validation succeeds, it returns a tuple with only the settings required by the batch processing pipeline:
        /// DirectorySettings, ScriptSettings, ReprocessSettings, and TimeOutSettings.
        /// </summary>
        /// <param name="path">The full path to the configuration file.</param>
        /// <returns>
        /// A tuple (DirectorySettings, ScriptSettings, ReprocessSettings, TimeOutSettings) if configuration loading and validation succeed;
        /// otherwise, null.
        /// </returns>
        public async Task<(DirectorySettings?, ScriptSettings?, ReprocessSettings?, TimeOutSettings?)?> LoadConfig(string path)
        {
            await Task.CompletedTask; // For asynchronous signature.

            try
            {
                if (!File.Exists(path))
                {
                    _output.ShowError($"Configuration file not found: {path}");
                    return null;
                }

                var (configRoot, parseErrors) = _configParser.ParseConfigFile(path);
                if (configRoot == null)
                {
                    foreach (var error in parseErrors)
                    {
                        _output.ShowError(error);
                    }
                    return null;
                }

                // Create a ProjectName instance from the parsed configuration.
                var projectNameObj = new ProjectName { Name = configRoot.ProjectName };
                // *** IMPORTANT: Set the ActualConfigFileName so that the validator can check it.
                projectNameObj.ActualConfigFileName = Path.GetFileName(path);

                var validationResult = _validator.ValidateConfig(
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
                        _output.ShowError("Configuration validation error: " + error);
                    }
                    return null;
                }

                return (configRoot.Directories, configRoot.Script_Settings, configRoot.Reprocess_Settings, configRoot.Timeout_Minutes);
            }
            catch (Exception ex)
            {
                _output.ShowError("Error loading configuration: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Updates the batch configuration.
        /// 
        /// This method validates the provided configuration settings.
        /// Since the update operation only provides the subset of settings used by the batch processing pipeline
        /// (DirectorySettings, ScriptSettings, ReprocessSettings, and TimeOutSettings), default values are supplied
        /// for the additional parameters required by the validator (ProjectName, PIDSettings, and RhinoFileNameSettings).
        /// </summary>
        /// <param name="directorySettings">The updated directory settings.</param>
        /// <param name="scriptSettings">The updated script settings.</param>
        /// <param name="reprocessSettings">The updated reprocess settings.</param>
        /// <param name="timeoutSettings">The updated timeout settings.</param>
        /// <returns>True if the updated configuration is valid; otherwise, false.</returns>
        public async Task<bool> UpdateConfig(
            DirectorySettings directorySettings,
            ScriptSettings scriptSettings,
            ReprocessSettings reprocessSettings,
            TimeOutSettings timeoutSettings)
        {
            await Task.CompletedTask;
            try
            {
                // Supply default (empty) instances for the missing required parameters.
                var defaultProjectName = new ProjectName { Name = string.Empty };
                var defaultPIDSettings = new PIDSettings();
                var defaultRhinoFileNameSettings = new RhinoFileNameSettings();

                var validationResult = _validator.ValidateConfig(
                    defaultProjectName,
                    directorySettings,
                    defaultPIDSettings,
                    defaultRhinoFileNameSettings,
                    scriptSettings,
                    reprocessSettings,
                    timeoutSettings
                );

                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        _output.ShowError(error);
                    }
                    return false;
                }

                _logger.LogInformation("Configuration updated successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _output.ShowError($"Error updating configuration: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves the current batch configuration to the specified file.
        /// 
        /// Only the settings used by the batch processing pipeline (DirectorySettings, ScriptSettings, ReprocessSettings, TimeOutSettings)
        /// are saved, even though the full configuration includes additional fields.
        /// </summary>
        /// <param name="path">The full path where the configuration will be saved.</param>
        /// <param name="directorySettings">The directory settings.</param>
        /// <param name="scriptSettings">The script settings.</param>
        /// <param name="reprocessSettings">The reprocess settings.</param>
        /// <param name="timeoutSettings">The timeout settings.</param>
        /// <returns>True if the configuration was saved successfully; otherwise, false.</returns>
        public async Task<bool> SaveConfig(string path,
            DirectorySettings directorySettings,
            ScriptSettings scriptSettings,
            ReprocessSettings reprocessSettings,
            TimeOutSettings timeoutSettings)
        {
            await Task.CompletedTask;
            try
            {
                var configObject = new
                {
                    // Save only the settings required for batch processing.
                    Directories = directorySettings,
                    Script_Settings = scriptSettings,
                    Reprocess_Settings = reprocessSettings,
                    Timeout_Minutes = timeoutSettings
                };
                var json = JsonSerializer.Serialize(configObject, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(path, json);

                _logger.LogInformation($"Configuration saved to: {path}");
                return true;
            }
            catch (Exception ex)
            {
                _output.ShowError($"Error saving configuration: {ex.Message}");
                return false;
            }
        }
    }
}
