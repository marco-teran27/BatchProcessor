using System;
using System.Threading.Tasks;

namespace BatchProcessor.DI.Interfaces.Error
{
    /// <summary>
    /// Provides an abstraction for error management.
    /// Implementations are responsible for logging errors and returning a detailed error response.
    /// </summary>
    public interface IErrorManager
    {
        /// <summary>
        /// Handles an error that occurred during processing.
        /// </summary>
        /// <param name="file">The file associated with the error.</param>
        /// <param name="ex">The exception that was thrown.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an <see cref="ErrorResponse"/>
        /// with details of the error.
        /// </returns>
        Task<ErrorResponse> HandleError(string file, Exception ex);
    }
}
