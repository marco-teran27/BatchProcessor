using System.Threading.Tasks;
using BatchProcessor.Core.Models;  // BatchResults is defined in this namespace

namespace BatchProcessor.DI.Interfaces.Logging
{
    /// <summary>
    /// Defines a contract for writing log entries.
    /// Implementers of this interface are responsible for persisting batch processing statistics
    /// to a storage medium (for example, writing to a log file in JSON format).
    /// </summary>
    public interface ILogWriter
    {
        /// <summary>
        /// Asynchronously writes batch processing statistics to a log file.
        /// </summary>
        /// <param name="projectName">
        /// The project name used to construct the log file name.
        /// </param>
        /// <param name="stats">
        /// A <see cref="BatchResults"/> object containing the batch processing statistics.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous write operation.
        /// </returns>
        Task WriteLogAsync(string projectName, BatchResults stats);
    }
}
