/// <summary>
/// Handles only the file selection dialog for configuration files.
/// Single responsibility: Configuration file selection via UI dialog.
/// </summary>

namespace BatchProcessor.DI.Interfaces.Config
{
    public interface IConfigSelUI
    {
        /// <summary>
        /// Displays file selection dialog and returns selected configuration file path.
        /// Returns null if user cancels or selection is invalid.
        /// </summary>
        string? SelectConfigurationFile();
    }
}