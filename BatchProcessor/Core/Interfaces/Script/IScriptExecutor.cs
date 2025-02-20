using System.Threading.Tasks;

namespace BatchProcessor.DI.Interfaces.Script
{
    /// <summary>
    /// Defines the contract for executing a script associated with a Rhino file.
    /// Implementers should run the given script (typically by wrapping RhinoApp.RunScript)
    /// and return a tuple indicating whether the execution succeeded and any additional details.
    /// </summary>
    public interface IScriptExecutor
    {
        /// <summary>
        /// Executes the script for a specified Rhino file.
        /// </summary>
        /// <param name="scriptPath">
        /// The full file path of the script to execute.
        /// </param>
        /// <param name="rhinoFile">
        /// The name of the Rhino file being processed.
        /// </param>
        /// <returns>
        /// A task that returns a tuple:
        /// (bool success, string details) where <c>success</c> indicates whether the script execution succeeded,
        /// and <c>details</c> contains any message or error details.
        /// </returns>
        Task<(bool success, string details)> ExecuteScript(string scriptPath, string rhinoFile);
    }
}
