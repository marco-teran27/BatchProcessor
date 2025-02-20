using System;
using System.Collections.Generic;

namespace BatchProcessor.DI.Interfaces.AbsRhino
{
    /// <summary>
    /// Handles command line output display.
    /// Single responsibility: Command line information display.
    /// </summary>
    public interface ICommLineOut
    {
        /// <summary>
        /// Displays dependency verification status.
        /// </summary>
        /// <param name="dependencies">Collection of items and their status</param>
        void ShowDependencyStatus(IEnumerable<(string Item, string Status)> dependencies);

        /// <summary>
        /// Updates the progress display in the command line.
        /// </summary>
        void UpdateProgress(int current, int total, string currentFile, TimeSpan estimatedTimeRemaining);

        /// <summary>
        /// Displays an error message.
        /// </summary>
        void ShowError(string message);

        /// <summary>
        /// Displays an informational message.
        /// </summary>
        void ShowMessage(string message);

        /// <summary>
        /// Clears the command line display.
        /// </summary>
        void Clear();
    }
}