using System.Threading.Tasks;

namespace BatchProcessor.DI.Interfaces.Batch
{
    /// <summary>
    /// Manages completion detection and timeout handling for batch processes
    /// </summary>
    public interface IBatchCompletionManager
    {
        /// <summary>
        /// Waits for completion of a file processing operation
        /// </summary>
        /// <param name="filePath">Path to the file being processed</param>
        /// <param name="scriptPath">Path to the script processing the file</param>
        /// <returns>Tuple indicating success and details of completion</returns>
        Task<(bool success, string details)> WaitForCompletion(string filePath, string scriptPath);

        /// <summary>
        /// Checks if completion files exist for a file
        /// </summary>
        /// <param name="fileName">Name of the file to check</param>
        /// <returns>True if valid completion files exist</returns>
        bool HasValidCompletionFiles(string fileName);
    }
}