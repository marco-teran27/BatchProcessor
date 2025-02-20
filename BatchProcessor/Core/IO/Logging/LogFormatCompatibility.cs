using System;
using System.Linq;
using System.Text.Json;
using BatchProcessor.Core.Models; // Contains BatchResults, FileProcessingResult, ProcessingMetrics, BatchStatus

namespace BatchProcessor.Core.IO.Logging
{
    /// <summary>
    /// LogFormatCompatibility ensures compatibility between legacy and current log formats.
    /// It converts a current <see cref="BatchResults"/> instance into a legacy format.
    /// 
    /// Note:
    /// - The legacy format expects properties such as "ProjectName" and "MissingCount" on BatchResults,
    ///   and properties like "CpuUsage", "MemoryUsageMB", and "ScriptExecutionTimeSeconds" on ProcessingMetrics.
    ///   Since our current types do not include these members, default values are supplied.
    /// </summary>
    public class LogFormatCompatibility
    {
        /// <summary>
        /// Converts the current log format to a legacy format if needed.
        /// </summary>
        /// <param name="results">A BatchResults object with current log format.</param>
        /// <param name="useLegacyFormat">
        /// If true, the log is converted to a legacy format; otherwise, the current format is serialized.
        /// </param>
        /// <returns>A JSON string representing the formatted log.</returns>
        public string EnsureCompatibleFormat(BatchResults results, bool useLegacyFormat = false)
        {
            if (!useLegacyFormat)
            {
                return JsonSerializer.Serialize(results);
            }

            // Create a legacy format object with default or mapped values.
            var legacyFormat = new
            {
                // Since BatchResults does not have ProjectName, use a default (empty string).
                ProjectName = "",
                StartTime = results.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                EndTime = results.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                TotalFiles = results.TotalFiles,
                // Map SuccessfulFiles to SuccessCount.
                SuccessCount = results.SuccessfulFiles,
                // Map FailedFiles to FailureCount.
                FailureCount = results.FailedFiles,
                // No MissingFiles property exists; supply default 0.
                MissingCount = 0,
                WasCancelled = results.WasCancelled,
                FileResults = results.FileResults.Select(fr => new
                {
                    FileName = fr.FileName,
                    Status = fr.Status.ToString(),
                    Details = fr.Details,
                    ProcessingStartTime = fr.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ProcessingEndTime = fr.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Metrics = new
                    {
                        // ProcessingMetrics does not include CpuUsage or MemoryUsageMB,
                        // so we supply default values.
                        CpuUsage = 0.0,
                        MemoryUsageMB = 0.0,
                        // For ScriptExecutionTimeSeconds, we use ProcessingTimeSeconds as a proxy.
                        ScriptExecutionTimeSeconds = fr.Metrics.ProcessingTimeSeconds,
                        // Assume ResourceCleanupConfirmed is false.
                        ResourceCleanupConfirmed = false
                    }
                })
            };

            return JsonSerializer.Serialize(legacyFormat, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Attempts to deserialize a log string in the current format.
        /// </summary>
        /// <param name="logContent">The JSON content of the log.</param>
        /// <returns>A BatchResults object if deserialization succeeds; otherwise, an exception is thrown.</returns>
        public BatchResults ReadLog(string logContent)
        {
            try
            {
                // Try to deserialize using the current format.
                return JsonSerializer.Deserialize<BatchResults>(logContent)
                    ?? throw new JsonException("Failed to deserialize log");
            }
            catch
            {
                // If deserialization fails, fall back to legacy parsing.
                return ReadLegacyLog(logContent);
            }
        }

        /// <summary>
        /// Legacy log parsing.
        /// </summary>
        /// <param name="logContent">The JSON content of the legacy log.</param>
        /// <returns>A BatchResults object parsed from the legacy log content.</returns>
        private BatchResults ReadLegacyLog(string logContent)
        {
            // Legacy log parsing logic to be implemented as needed.
            throw new NotImplementedException("Legacy log parsing to be implemented");
        }
    }
}
