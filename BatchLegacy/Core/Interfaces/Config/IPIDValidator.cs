using BatchProcessor.Core.Config.Models;
using BatchProcessor.Core.Config.Validation;

/*
Summary: Interface for validating PID settings configuration. Methods now return a ValidationResult.
*/

namespace BatchProcessor.DI.Interfaces.Config
{
    /// <summary>
    /// Defines a contract for validating PID settings configuration.
    /// </summary>
    public interface IPIDValidator
    {
        /// <summary>
        /// Validates the PID settings.
        /// </summary>
        /// <param name="settings">The PID settings to validate.</param>
        /// <returns>A ValidationResult with validation status and error messages.</returns>
        ValidationResult ValidateConfig(PIDSettings settings);
    }
}
