using System;
using System.Windows.Forms;

namespace ConfigJSON
{
    /// <summary>
    /// Provides a UI for selecting a JSON configuration file.
    /// </summary>
    public class ConfigSelector
    {
        /// <summary>
        /// Opens a file dialog to select a JSON configuration file.
        /// </summary>
        /// <returns>The selected file path or null if canceled.</returns>
        public string? SelectConfigFile()
        {
            try
            {
                using var dialog = new OpenFileDialog
                {
                    Filter = "JSON Files (*.json)|*.json",
                    Title = "Select Batch Configuration File"
                };

                return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting config file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}