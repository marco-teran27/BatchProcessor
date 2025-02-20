namespace BatchProcessor.DI.Interfaces.Script
{
    /// <summary>
    /// Defines the contract for validating a script file path.
    /// Implementers should verify that the path is non‐empty, the file exists,
    /// and that the file is accessible (for example, that read permissions are available).
    /// </summary>
    public interface IScriptPathValidator
    {
        /// <summary>
        /// Validates that the provided script path is valid and accessible.
        /// </summary>
        /// <param name="scriptPath">The full path to the script file.</param>
        /// <returns>
        /// <c>true</c> if the script path is valid (non‐empty, exists, and accessible); otherwise, <c>false</c>.
        /// </returns>
        bool ValidatePath(string scriptPath);
    }
}
