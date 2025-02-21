namespace BatchProcessor.DI.Interfaces.Error
{
    /// <summary>
    /// Represents the response returned after an error handling operation.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Gets or sets detailed information about the error.
        /// This property is automatically initialized to a new instance of <see cref="ErrorInfo"/>.
        /// </summary>
        public ErrorInfo ErrorInfo { get; set; } = new ErrorInfo();
    }

    /// <summary>
    /// Contains detailed information about an error.
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>
        /// Gets or sets the error message.
        /// This property is automatically initialized to an empty string.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
