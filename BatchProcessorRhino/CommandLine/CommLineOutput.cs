using System;
using Rhino;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using BatchProcessor.Core.Interfaces.AbsRhino;

namespace BatchProcessorRhino.CommandLine
{
    /// <summary>
    /// ==============================================================================
    /// SUMMARY:
    ///   CommLineOutput is the implementation of ICommLineOut, used to display messages 
    ///   and errors to Rhino's command line. We've added two constructors:
    ///
    ///   1) A parameterless constructor that defaults to using a NullLogger for 
    ///      situations where no actual ILogger is provided.
    ///   2) A constructor that accepts an ILogger<CommLineOutput> if real logging 
    ///      is desired (e.g., from a DI container).
    ///
    ///   This design allows easy instantiation (new CommLineOutput()) while also
    ///   permitting advanced logging when needed.
    /// ==============================================================================
    /// </summary>
    public class CommLineOutput : ICommLineOut
    {
        private readonly ILogger<CommLineOutput> _logger;

        /// <summary>
        /// Parameterless constructor. Uses a NullLogger by default, allowing 
        /// call sites to instantiate CommLineOutput() without passing a logger.
        /// </summary>
        public CommLineOutput()
            : this(NullLogger<CommLineOutput>.Instance)
        {
        }

        /// <summary>
        /// Primary constructor that requires an ILogger<CommLineOutput>. This is used
        /// if the calling code (or DI system) wants to provide a real logger instance.
        /// </summary>
        /// <param name="logger">An ILogger used for logging messages and errors.</param>
        public CommLineOutput(ILogger<CommLineOutput> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays dependency verification status in Rhino's command line.
        /// </summary>
        /// <param name="dependencies">A collection of (Item, Status) pairs to display.</param>
        public void ShowDependencyStatus(IEnumerable<(string Item, string Status)> dependencies)
        {
            foreach (var (item, status) in dependencies)
            {
                RhinoApp.WriteLine($"{item}: {status}");
                _logger.LogInformation($"{item}: {status}");
            }
        }

        /// <summary>
        /// Updates progress display with a progress bar or simple message, plus logs the information.
        /// </summary>
        /// <param name="current">Current progress count.</param>
        /// <param name="total">Total count.</param>
        /// <param name="currentFile">The file currently being processed.</param>
        /// <param name="estimatedTimeRemaining">Estimated time remaining.</param>
        public void UpdateProgress(int current, int total, string currentFile, TimeSpan estimatedTimeRemaining)
        {
            var percentage = (double)current / total * 100.0;
            RhinoApp.WriteLine($"Processing {currentFile}: {current}/{total} ({percentage:F1}%)");
            _logger.LogInformation($"Progress: {current}/{total} => {percentage:F1}% - {currentFile}");
        }

        /// <summary>
        /// Displays an error message in Rhino's command line and logs it at error level.
        /// </summary>
        /// <param name="message">The error message to show.</param>
        public void ShowError(string message)
        {
            RhinoApp.WriteLine($"Error: {message}");
            _logger.LogError(message);
        }

        /// <summary>
        /// Displays a general informational message in Rhino's command line, and logs it at info level.
        /// </summary>
        /// <param name="message">Message to display in Rhino's command line.</param>
        public void ShowMessage(string message)
        {
            RhinoApp.WriteLine(message);
            _logger.LogInformation(message);
        }

        /// <summary>
        /// Clears Rhino's command line output window.
        /// </summary>
        public void Clear()
        {
            RhinoApp.ClearCommandHistoryWindow();
            _logger.LogDebug("Command line cleared.");
        }
    }
}
