using BatchProcessor.Core.Config.Models;
using BatchProcessor.Core.Config.Validation;

/*
Summary: Interface for validating Rhino file name settings and file names. 
         Includes methods that return a ValidationResult for configuration validation.
*/

namespace BatchProcessor.DI.Interfaces.Config
{
    /// <summary>
    /// Defines a contract for validating Rhino-related file name scenarios.
    /// </summary>
    public interface IRhinoFileNameValidator
    {
        /// <summary>
        /// Checks whether the given file name matches the config file naming pattern.
        /// </summary>
        /// <param name="fileName">The file name to validate.</param>
        /// <param name="projectName">The extracted project name if matching succeeds.</param>
        /// <returns>True if the file name is valid; otherwise, false.</returns>
        bool IsValidConfigFileName(string fileName, out string? projectName);

        /// <summary>
        /// Checks whether a Rhino .3dm file name matches the expected naming pattern.
        /// </summary>
        /// <param name="fileName">The file name to validate.</param>
        /// <param name="parts">A tuple capturing (pid, keyword, srNumber) if matching succeeds.</param>
        /// <returns>True if the file name is valid; otherwise, false.</returns>
        bool IsValidRhinoFileName(string fileName, out (string pid, string keyword, string srNumber) parts);

        /// <summary>
        /// Validates the entire RhinoFileNameSettings object.
        /// </summary>
        /// <param name="settings">The Rhino file name settings to validate.</param>
        /// <returns>A ValidationResult indicating success and any errors.</returns>
        ValidationResult ValidateConfig(RhinoFileNameSettings settings);
    }
}
