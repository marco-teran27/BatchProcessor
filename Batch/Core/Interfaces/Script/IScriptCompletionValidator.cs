
using System.Threading.Tasks;

namespace BatchProcessor.DI.Interfaces.Script
{
    /// <summary>
    /// Defines contract for validating script completion.
    /// </summary>
    public interface IScriptCompletionValidator
    {
        /// <summary>
        /// Validates whether a script has completed processing.
        /// </summary>
        /// <param name="fileName">The file being processed</param>
        /// <param name="scriptPath">Path to the script being executed</param>
        /// <returns>Success status and completion details</returns>
        Task<(bool success, string details)> ValidateCompletion(string fileName, string scriptPath);

        /// <summary>
        /// Checks if completion files exist and are valid.
        /// </summary>
        /// <param name="fileName">Name of the file to check</param>
        /// <returns>True if valid completion files exist</returns>
        bool HasValidCompletionFiles(string fileName);
    }
}