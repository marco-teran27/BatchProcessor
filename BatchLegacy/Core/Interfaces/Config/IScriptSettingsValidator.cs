using BatchProcessor.Core.Config.Models;
using BatchProcessor.Core.Config.Validation;

/*
Summary: Interface for validating script settings configuration. Methods now return a ValidationResult.
*/

namespace BatchProcessor.DI.Interfaces.Config
{
    /// <summary>
    /// Defines a contract for validating script settings configuration.
    /// </summary>
    public interface IScriptSettingsValidator
    {
        /// <summary>
        /// Validates the script settings given a script directory.
        /// </summary>
        /// <param name="settings">The script settings to validate.</param>
        /// <param name="scriptDir">The directory where the script is expected to be found.</param>
        /// <returns>A ValidationResult with validation status and errors.</returns>
        ValidationResult ValidateConfig(ScriptSettings settings, string scriptDir);
    }
}
