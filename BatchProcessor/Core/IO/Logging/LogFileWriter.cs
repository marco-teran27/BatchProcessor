using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BatchProcessor.DI.Interfaces.Logging;
using BatchProcessor.Core.Models;  // Contains BatchResults
// Other using directives remain unchanged

namespace BatchProcessor.Core.IO.Logging
{
    /// <summary>
    /// LogFileWriter handles writing batch processing statistics to a log file in JSON format.
    /// It uses an injected ILogFormatter to format the log output and writes the result to a file.
    /// </summary>
    public class LogFileWriter : ILogWriter
    {
        private readonly string _outputDir;
        private readonly ILogFormatter _formatter;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFileWriter"/> class.
        /// </summary>
        /// <param name="outputDir">The directory where log files should be written.</param>
        /// <param name="formatter">An ILogFormatter used to format the log output.</param>
        public LogFileWriter(string outputDir, ILogFormatter formatter)
        {
            _outputDir = outputDir ?? throw new ArgumentNullException(nameof(outputDir));
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        /// <summary>
        /// Asynchronously writes the batch processing statistics to a log file.
        /// The log file is named using the provided project name and a timestamp.
        /// </summary>
        /// <param name="projectName">The project name used for log file naming.</param>
        /// <param name="stats">
        /// A <see cref="BatchResults"/> object containing the batch processing statistics.
        /// (This type is used in place of the non-existent BatchProcessingStatistics.)
        /// </param>
        /// <returns>A task representing the asynchronous write operation.</returns>
        public async Task WriteLogAsync(string projectName, BatchResults stats)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var logPath = Path.Combine(_outputDir, $"log-{projectName}-{timestamp}.json");

            var formattedLog = _formatter.FormatLog(stats);

            try
            {
                Directory.CreateDirectory(_outputDir);
                await File.WriteAllTextAsync(logPath, JsonSerializer.Serialize(formattedLog, _jsonOptions));
            }
            catch (Exception ex)
            {
                // Use the three-argument constructor: message, logPath, innerException.
                throw new LogWriteException($"Failed to write log file: {ex.Message}", logPath, ex);
            }
        }
    }
}
