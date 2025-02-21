using BatchProcessor.Core.Config.Validation;

/*
Summary: Interface for validating directory configurations. Methods now return a ValidationResult.
*/

namespace BatchProcessor.DI.Interfaces.Config
{
    /// <summary>
    /// Defines a contract for validating directory configurations.
    /// </summary>
    public interface IDirectoryValidator
    {
        /// <summary>
        /// Validates the specified directories.
        /// </summary>
        /// <param name="directories">An array of directory paths to validate.</param>
        /// <returns>A ValidationResult containing the success flag and error messages.</returns>
        ValidationResult ValidateDirectories(string[] directories);
    }
}
