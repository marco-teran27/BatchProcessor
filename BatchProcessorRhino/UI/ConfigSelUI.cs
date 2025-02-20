using System;
using System.Windows.Forms;
using BatchProcessor.Core.Interfaces.Config;

namespace BatchProcessorRhino.UI
{
    /// <summary>
    /// =============================================================================
    /// SUMMARY:
    ///   ConfigSelUI is responsible solely for presenting a Windows OpenFileDialog to
    ///   allow the user to select a configuration file (.json). It does not perform any
    ///   file name validation or detailed error reporting; it simply returns the full file
    ///   path selected by the user. Detailed validation and error handling occur later in
    ///   the workflow (in ConfigParser and the validation pipeline).
    /// =============================================================================
    /// </summary>
    public class ConfigSelUI : IConfigSelUI
    {
        /// <summary>
        /// Opens a file open dialog to select a JSON configuration file.
        /// Returns the full file path if a file is selected; otherwise, returns null.
        /// </summary>
        /// <returns>
        /// The selected configuration file path or null if the dialog is cancelled.
        /// </returns>
        public string? SelectConfigurationFile()
        {
            try
            {
                using var dialog = new OpenFileDialog
                {
                    Filter = "JSON Files (*.json)|*.json",
                    Title = "Select Configuration File"
                };

                // Show the dialog and return null if the user cancels.
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return null;
                }

                // Simply return the selected file path.
                return dialog.FileName;
            }
            catch (Exception)
            {
                // Any exceptions here (e.g. if the dialog cannot be shown) are left to be handled by the caller.
                return null;
            }
        }
    }
}
