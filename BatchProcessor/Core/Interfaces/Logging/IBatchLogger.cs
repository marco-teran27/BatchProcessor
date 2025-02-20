using System;
using System.Threading.Tasks;
using BatchProcessor.Core.Models;

namespace BatchProcessor.DI.Interfaces.Logging
{
    /// <summary>
    /// Defines the contract for persistent logging of batch processing operations
    /// </summary>
    public interface IBatchLogger
    {
        Task LogFileProcessed(
            string fileName,
            bool success,
            string details,
            string processingMode = "ALL",
            ProcessingMetrics? metrics = null,
            DateTime? processingStartTime = null,
            DateTime? processingEndTime = null
        );

        Task LogProcessingStatus(
            string fileName,
            BatchStatus status,
            string details,
            ProcessingMetrics? metrics = null,
            DateTime? processingStartTime = null,
            DateTime? processingEndTime = null
        );

        Task LogError(string fileName, Exception ex);

        Task LogCancellation();

        // Changed return type from BatchProcessingStatistics to BatchResults
        BatchResults GetStatistics();
    }
}