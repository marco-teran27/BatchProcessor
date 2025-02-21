using BatchProcessor.Core.Config.Models;
using BatchProcessor.Core.Config.Validation;

/*
Summary: Declares the interface for validating the project name.
         The validator ensures that the JSON-supplied project name matches the actual configuration file name.
         The validation result is encapsulated in a ValidationResult.
*/

namespace BatchProcessor.DI.Interfaces.Config
{
    /// <summary>
    /// Defines a contract for validating a ProjectName instance.
    /// </summary>
    public interface IProjectNameValidator
    {
        /// <summary>
        /// Validates the project name by comparing the JSON project name with the actual config file name.
        /// </summary>
        /// <param name="projectName">The ProjectName object containing both the JSON value and actual file name.</param>
        /// <returns>
        /// A <see cref="ValidationResult"/> that indicates success or failure and includes any error messages.
        /// </returns>
        ValidationResult ValidateConfig(ProjectName projectName);
    }
}
