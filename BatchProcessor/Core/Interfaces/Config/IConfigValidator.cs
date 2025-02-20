using BatchProcessor.Core.Config.Models;
using BatchProcessor.Core.Config.Validation;

/*
Summary: Updated to include a ProjectName parameter for overall configuration validation.
*/

namespace BatchProcessor.DI.Interfaces.Config
{
    /// <summary>
    /// Defines a contract for validating the overall configuration.
    /// </summary>
    public interface IConfigValidator
    {
        /// <summary>
        /// Validates the overall configuration from various settings.
        /// </summary>
        /// <param name="projectName">Project name configuration to validate.</param>
        /// <param name="directorySettings">Directory settings.</param>
        /// <param name="pidSettings">PID settings.</param>
        /// <param name="fileNameSettings">Rhino file name settings.</param>
        /// <param name="scriptSettings">Script settings.</param>
        /// <param name="reprocessSettings">Reprocess settings.</param>
        /// <param name="timeoutSettings">Timeout settings.</param>
        /// <returns>A ValidationResult with overall validation status and error messages.</returns>
        ValidationResult ValidateConfig(
            ProjectName projectName,
            DirectorySettings directorySettings,
            PIDSettings pidSettings,
            RhinoFileNameSettings fileNameSettings,
            ScriptSettings scriptSettings,
            ReprocessSettings reprocessSettings,
            TimeOutSettings timeoutSettings
        );
    }
}
