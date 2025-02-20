using BatchProcessor.Core.Config.Models;

namespace BatchProcessor.DI.Interfaces.Script
{
    /// <summary>
    /// Defines contract for scanning and locating scripts.
    /// </summary>
    public interface IScriptScanner
    {
        /// <summary>
        /// Finds a script in the specified directory.
        /// </summary>
        /// <param name="directory">Directory to search</param>
        /// <param name="scriptName">Name of the script to find</param>
        /// <param name="type">Type of script to look for</param>
        /// <returns>Full path to script if found, null if not found</returns>
        string? FindScript(string directory, string scriptName, ScriptType type);

        /// <summary>
        /// Verifies if a script exists and is valid.
        /// </summary>
        /// <param name="scriptPath">Full path to script</param>
        /// <param name="type">Expected script type</param>
        /// <returns>True if script is valid</returns>
        bool ValidateScript(string scriptPath, ScriptType type);
    }
}