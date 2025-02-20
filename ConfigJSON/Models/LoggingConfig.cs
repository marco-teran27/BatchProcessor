using System;
using System.Collections.Generic;
using System.IO;
using BatchProcessor.Core.Config.Validation;

/*
Summary: Represents logging configuration settings that control how logging is managed.
         Provides a Validate() method to check configuration integrity, returning a ValidationResult.
*/

namespace ConfigJSON.Models
{
    /// <summary>
    /// Represents configuration settings for logging.
    /// </summary>
    public class LoggingConfig
    {
        /// <summary>
        /// Number of command line output lines to buffer.
        /// </summary>
        public int CommandLineBufferSize { get; set; } = 1000;

        /// <summary>
        /// Enables detailed debug logging.
        /// </summary>
        public bool EnableDebugLogging { get; set; } = false;

        /// <summary>
        /// Directory path for log file storage.
        /// </summary>
        public string LogDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether to keep completion files after processing.
        /// </summary>
        public bool PreserveCompletionFiles { get; set; } = false;

        /// <summary>
        /// Maximum size in bytes for a single log file.
        /// </summary>
        public int MaxLogFileSize { get; set; } = 10 * 1024 * 1024; // 10MB default

        /// <summary>
        /// Validates the logging configuration settings.
        /// </summary>
        /// <returns>A ValidationResult indicating whether the configuration is valid, and any error messages.</returns>
        public ValidationResult Validate()
        {
            var errors = new List<string>();

            if (CommandLineBufferSize <= 0)
                errors.Add("Command line buffer size must be greater than 0");

            if (MaxLogFileSize <= 0)
                errors.Add("Max log file size must be greater than 0");

            if (!string.IsNullOrEmpty(LogDirectory) && !Directory.Exists(LogDirectory))
                errors.Add($"Log directory not found: {LogDirectory}");

            return new ValidationResult(errors.Count == 0, errors);
        }
    }
}
