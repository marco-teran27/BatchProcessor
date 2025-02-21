using BatchProcessor.Core.Models;  // BatchResults is defined in this namespace

namespace BatchProcessor.DI.Interfaces.Logging
{
    /// <summary>
    /// Defines a contract for formatting log entries.
    /// Implementers of this interface format batch processing statistics
    /// into a structured log format.
    /// </summary>
    public interface ILogFormatter
    {
        /// <summary>
        /// Formats the provided batch processing statistics into a structured log object.
        /// </summary>
        /// <param name="stats">
        /// A <see cref="BatchResults"/> object containing the statistics of the batch processing operation.
        /// </param>
        /// <returns>
        /// An object representing the formatted log entry.
        /// </returns>
        object FormatLog(BatchResults stats);
    }
}
