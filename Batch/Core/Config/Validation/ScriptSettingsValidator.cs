using System;
using System.Collections.Generic;
using System.IO;
using BatchProcessor.Core.Config.Models;
using BatchProcessor.DI.Interfaces.Config;

namespace BatchProcessor.Core.Config.Validation
{
    /// <summary>
    /// Validates the script settings configuration.
    /// 
    /// This version uses the ValidationResult pipeline for error reporting.
    /// The dependency on ICommLineOut has been removed so that no direct command-line output occurs.
    /// </summary>
    public class ScriptSettingsValidator : IScriptSettingsValidator
    {
        // ICommLineOut dependency removed; errors are now added to the ValidationResult.

        /// <summary>
        /// Validates the provided script settings against the expected script directory.
        /// </summary>
        /// <param name="settings">The script settings to validate.</param>
        /// <param name="scriptDir">The directory in which the script is expected to reside.</param>
        /// <returns>A ValidationResult indicating whether the settings are valid and containing any error messages.</returns>
        public ValidationResult ValidateConfig(ScriptSettings settings, string scriptDir)
        {
            var errors = new List<string>();

            // Validate script name is provided.
            if (string.IsNullOrWhiteSpace(settings.ScriptName))
            {
                errors.Add("Script name cannot be empty");
            }

            // Validate that the provided script type is a defined enum value.
            if (!Enum.IsDefined(typeof(ScriptType), settings.ScriptType))
            {
                errors.Add($"Invalid script type: {settings.ScriptType}");
            }

            // Determine file extension based on script type.
            string extension = settings.ScriptType switch
            {
                ScriptType.Python => ".py",
                ScriptType.Grasshopper => ".gh",
                ScriptType.GrasshopperXml => ".ghx",
                _ => throw new ArgumentException($"Unsupported script type: {settings.ScriptType}")
            };

            // Build the expected script file path and check for its existence.
            var scriptPath = Path.Combine(scriptDir, $"{settings.ScriptName}{extension}");
            if (!File.Exists(scriptPath))
            {
                errors.Add($"Script file not found: {scriptPath}");
            }

            return new ValidationResult(errors.Count == 0, errors);
        }
    }
}
