// File: Commons\ValidationResult.cs
namespace Commons
{
    /// <summary>
    /// Represents the outcome of a validation operation with success status and error messages.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets whether the validation succeeded.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets a read-only list of error messages if validation failed.
        /// </summary>
        public IReadOnlyList<string> Errors { get; }

        /// <summary>
        /// Initializes a new ValidationResult.
        /// </summary>
        /// <param name="isValid">True if valid; false otherwise.</param>
        /// <param name="errors">List of error messages (empty if valid).</param>
        /// <exception cref="ArgumentNullException">Thrown if errors is null.</exception>
        public ValidationResult(bool isValid, IReadOnlyList<string>? errors)
        {
            IsValid = isValid;
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        /// <summary>
        /// Gets a successful validation result with no errors.
        /// </summary>
        public static ValidationResult Success => new(true, Array.Empty<string>());
    }
}