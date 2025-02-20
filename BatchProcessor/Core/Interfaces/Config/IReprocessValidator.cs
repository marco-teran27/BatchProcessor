using BatchProcessor.Core.Config.Models;
using BatchProcessor.Core.Config.Validation;

/*
Summary: Interface for validating reprocessing settings configuration. Methods now return a ValidationResult.
*/

namespace BatchProcessor.DI.Interfaces.Config
{
    /// <summary>
    /// Defines a contract for validating reprocessing settings configuration.
    /// </summary>
    public interface IReprocessValidator
    {
        /// <summary>
        /// Validates the reprocessing settings.
        /// </summary>
        /// <param name="settings">The reprocessing settings to validate.</param>
        /// <returns>A ValidationResult with validation status and error messages.</returns>
        ValidationResult ValidateConfig(ReprocessSettings settings);
    }
}
