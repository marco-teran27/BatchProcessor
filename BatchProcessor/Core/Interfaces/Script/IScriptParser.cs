using BatchProcessor.Core.Config.Models;

namespace BatchProcessor.DI.Interfaces.Script
{
    /// <summary>
    /// Defines contract for parsing and formatting script commands
    /// </summary>
    public interface IScriptParser
    {
        /// <summary>
        /// Formats a script command based on script type
        /// </summary>
        /// <param name="scriptPath">Path to the script file</param>
        /// <param name="type">Type of script (Python, Grasshopper, etc.)</param>
        /// <returns>Formatted command string ready for execution</returns>
        string FormatScriptCommand(string scriptPath, ScriptType type);
    }
}