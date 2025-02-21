using BatchProcessor.Core.Config.Validation;

/*
Summary: Interface for validating timeout settings. Methods now return a ValidationResult.
*/


namespace BatchProcessor.DI.Interfaces.Config
{
    /// <summary>
    /// Defines a contract for validating the timeout value.
    /// </summary>
    public interface ITimeOutValidator
    {
        /// <summary>
        /// Validates the specified timeout in minutes.
        /// </summary>
        /// <param name="timeoutMinutes">The timeout value to validate.</param>
        /// <returns>A ValidationResult containing the outcome and error messages.</returns>
        ValidationResult ValidateTimeout(int timeoutMinutes);
    }
}
