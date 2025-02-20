using System.Threading.Tasks;
using BatchProcessor.Core.Config.Models;        // for ScriptType
using BatchProcessor.Core.Config.Validation;      // for ValidationResult

namespace BatchProcessor.DI.Interfaces.Script
{
    /// <summary>
    /// Defines the contract for validating a script file.
    /// This interface combines the concerns of verifying the script path and ensuring the script type is correct.
    /// </summary>
    public interface IScriptValidator
    {
        /// <summary>
        /// Validates the specified script file by checking that:
        /// 1. The script path is valid and accessible.
        /// 2. The script file's type (as determined by its extension) matches the expected script type.
        /// </summary>
        /// <param name="scriptPath">The full path to the script file.</param>
        /// <param name="expectedType">The expected type of the script (e.g. Python, Grasshopper, etc.).</param>
        /// <returns>
        /// A task that returns a <see cref="ValidationResult"/> indicating the success of the validation and any error messages.
        /// </returns>
        Task<ValidationResult> ValidateScript(string scriptPath, ScriptType expectedType);
    }
}
