using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BatchProcessor.Core.Config.Models;               // For ScriptType
using BatchProcessor.Core.Config.Validation;            // For ValidationResult
using BatchProcessor.DI.Interfaces.AbsRhino;           // For ICommLineOut
using BatchProcessor.DI.Interfaces.Script;

namespace BatchProcessor.Core.Logic.Script.Validation
{
    /// <summary>
    /// ScriptValidationManager implements the IScriptValidator interface.
    /// It validates a script by checking that the script path exists and that its file extension
    /// matches the expected script type.
    /// </summary>
    public class ScriptValidationManager : IScriptValidator
    {
        private readonly IScriptCompletionValidator _completionValidator;
        private readonly IScriptTypeValidator _typeValidator;
        private readonly IScriptPathValidator _pathValidator;
        private readonly ICommLineOut _output;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptValidationManager"/> class.
        /// </summary>
        /// <param name="completionValidator">The script completion validator (reserved for future use).</param>
        /// <param name="typeValidator">The validator for verifying the script type based on file extension.</param>
        /// <param name="pathValidator">The validator for checking the existence and accessibility of the script file.</param>
        /// <param name="output">The command-line output interface used for reporting validation errors.</param>
        public ScriptValidationManager(
            IScriptCompletionValidator completionValidator,
            IScriptTypeValidator typeValidator,
            IScriptPathValidator pathValidator,
            ICommLineOut output)
        {
            _completionValidator = completionValidator ?? throw new ArgumentNullException(nameof(completionValidator));
            _typeValidator = typeValidator ?? throw new ArgumentNullException(nameof(typeValidator));
            _pathValidator = pathValidator ?? throw new ArgumentNullException(nameof(pathValidator));
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        /// <summary>
        /// Validates the script at the specified path against the expected script type.
        /// Checks that the path is valid and that the file extension matches the expected type.
        /// </summary>
        /// <param name="scriptPath">The full path to the script file.</param>
        /// <param name="expectedType">The expected script type (e.g. Python, Grasshopper, or GrasshopperXml).</param>
        /// <returns>
        /// A task that returns a <see cref="BatchProcessor.Core.Config.Validation.ValidationResult"/>
        /// indicating whether the script is valid and, if not, the list of error messages.
        /// </returns>
        public Task<ValidationResult> ValidateScript(string scriptPath, ScriptType expectedType)
        {
            var errors = new List<string>();

            // Validate the script path using the path validator.
            if (!_pathValidator.ValidatePath(scriptPath))
            {
                errors.Add("Invalid script path");
                return Task.FromResult(new ValidationResult(errors.Count == 0, errors));
            }

            // Validate that the script's file extension matches the expected type.
            if (!_typeValidator.IsValidScriptType(scriptPath, expectedType))
            {
                errors.Add("Invalid script type");
                return Task.FromResult(new ValidationResult(errors.Count == 0, errors));
            }

            return Task.FromResult(new ValidationResult(errors.Count == 0, errors));
        }
    }
}
