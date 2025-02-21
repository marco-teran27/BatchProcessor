using BatchProcessor.Core.Config.Models; // for ScriptType

namespace BatchProcessor.DI.Interfaces.Script
{
    /// <summary>
    /// Defines the contract for validating and determining the type of a script file.
    /// Implementers should check that the file extension matches the expected script type,
    /// and be able to determine the type of a script file from its extension.
    /// </summary>
    public interface IScriptTypeValidator
    {
        /// <summary>
        /// Checks whether the script file at the given path matches the expected script type.
        /// </summary>
        /// <param name="scriptPath">The full path to the script file.</param>
        /// <param name="expectedType">The expected script type.</param>
        /// <returns>
        /// <c>true</c> if the file extension matches the expected type; otherwise, <c>false</c>.
        /// </returns>
        bool IsValidScriptType(string scriptPath, ScriptType expectedType);

        /// <summary>
        /// Determines the script type based on the file's extension.
        /// </summary>
        /// <param name="scriptPath">The full path to the script file.</param>
        /// <returns>
        /// The determined <see cref="ScriptType"/> if the extension is recognized; otherwise, <c>null</c>.
        /// </returns>
        ScriptType? DetermineScriptType(string scriptPath);
    }
}
