namespace BatchProcessor.DI.Interfaces.Error
{
    /// <summary>
    /// Enumerates the levels of severity for errors logged within the application.
    /// </summary>
    public enum ErrorSeverity
    {
        /// <summary>
        /// Indicates a non-critical error that does not require immediate attention.
        /// </summary>
        Low,

        /// <summary>
        /// Indicates a moderate error that should be monitored.
        /// </summary>
        Medium,

        /// <summary>
        /// Indicates a serious error that might impact operations.
        /// </summary>
        High,

        /// <summary>
        /// Indicates a critical error that requires immediate intervention.
        /// </summary>
        Critical
    }
}
