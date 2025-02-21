using System;
using System.Linq;
using BatchProcessor.Core.Models;           // Contains BatchResults, FileProcessingResult, ProcessingMetrics, BatchStatus
using BatchProcessor.DI.Interfaces.Logging; // For ILogFormatter
using BatchProcessor.Core.IO.Logging; // For ILogFormatter

// Assume LogWriteException is defined in BatchProcessor.Core.IO.Logging or a related namespace.
namespace BatchProcessor.Core.IO.Logging
{
    /// <summary>
    /// LogFormatter formats batch processing statistics into a structured log format.
    /// 
    /// It implements ILogFormatter by accepting a <see cref="BatchResults"/> object and
    /// mapping its properties into a legacy log format. Since the current BatchResults does not
    /// contain properties such as ProjectName or MissingFiles—and ProcessingMetrics lacks properties
    /// like CpuUsage, MemoryUsageMB, and ScriptExecutionTimeSeconds—the formatter supplies default
    /// values for those fields.
    /// </summary>
    public class LogFormatter : ILogFormatter
    {
        /// <summary>
        /// Formats the provided <see cref="BatchResults"/> into a structured log object.
        /// </summary>
        /// <param name="stats">
        /// A <see cref="BatchResults"/> instance containing the batch processing statistics.
        /// </param>
        /// <returns>An object representing the formatted log entry.</returns>
        public object FormatLog(BatchResults stats)
        {
            if (stats == null)
                throw new ArgumentNullException(nameof(stats));

            try
            {
                return new
                {
                    // Since BatchResults does not include ProjectName, we supply an empty string.
                    ProjectName = "",
                    StartTime = stats.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    EndTime = stats.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),

                    // Use existing fields from BatchResults.
                    TotalFiles = stats.TotalFiles,
                    // Map SuccessfulFiles to SuccessCount.
                    SuccessCount = stats.SuccessfulFiles,
                    // Map FailedFiles to FailureCount.
                    FailureCount = stats.FailedFiles,
                    // MissingFiles is not tracked; use default 0.
                    MissingCount = 0,

                    // Calculate an average processing time if any file results exist.
                    ProcessingTimeAverage = stats.FileResults.Any()
                        ? $"{stats.FileResults.Average(fr => fr.Metrics.ProcessingTimeSeconds):F2}s"
                        : "0.00s",

                    // Format individual file entries.
                    Entries = stats.FileResults.Select(FormatEntry).ToList()
                };
            }
            catch (Exception ex)
            {
                // If an error occurs during formatting, wrap and rethrow as a LogWriteException.
                throw new LogWriteException($"Error formatting log: {ex.Message}", "memory", ex);
            }
        }

        /// <summary>
        /// Formats a single file processing result into a legacy log entry.
        /// </summary>
        /// <param name="entry">A <see cref="FileProcessingResult"/> representing one file's processing details.</param>
        /// <returns>An object representing the formatted log entry for that file.</returns>
        private object FormatEntry(FileProcessingResult entry)
        {
            return new
            {
                // Use the StartTime as the entry time.
                Time = entry.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                entry.FileName,
                Status = entry.Status.ToString(),
                entry.Details,
                // Performance Data:
                // ProcessingMetrics in this design does not include CPU or memory usage, so we use default values.
                CPU = "N/A",
                MemoryMB = "N/A",
                // Use ProcessingTimeSeconds as the duration.
                Duration = $"{entry.Metrics.ProcessingTimeSeconds:F1}s",
                // Assume that ResourceCleanupConfirmed is not tracked; default to false.
                ResourcesCleanedUp = false
            };
        }
    }
}
