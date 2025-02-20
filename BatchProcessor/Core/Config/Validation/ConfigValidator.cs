using System;
using System.Collections.Generic;
using BatchProcessor.Core.Config.Models;
using BatchProcessor.Core.Config.Validation;
using BatchProcessor.DI.Interfaces.Config;
using BatchProcessor.DI.Interfaces.AbsRhino;

/*
File: BatchProcessor\Core\Config\Validation\ConfigValidator.cs
Summary: Aggregates validations for all major configuration components including project name,
         directories, PID settings, Rhino file name settings, script settings, reprocessing settings,
         and timeout settings. Returns a single ValidationResult aggregating all error messages.
*/

namespace BatchProcessor.Core.Config.Validation
{
    /// <summary>
    /// Aggregates validation results from individual configuration validators.
    /// </summary>
    public class ConfigValidator : IConfigValidator
    {
        private readonly IDirectoryValidator _directoryValidator;
        private readonly IRhinoFileNameValidator _rhinoFileNameValidator;
        private readonly IPIDValidator _pidValidator;
        private readonly IScriptSettingsValidator _scriptValidator;
        private readonly IReprocessValidator _reprocessValidator;
        private readonly ICommLineOut _output;
        private readonly IProjectNameValidator _projectNameValidator;

        /// <summary>
        /// Initializes a new instance of ConfigValidator with optional validators.
        /// </summary>
        public ConfigValidator(
            IDirectoryValidator? directoryValidator = null,
            IRhinoFileNameValidator? rhinoFileNameValidator = null,
            IPIDValidator? pidValidator = null,
            IScriptSettingsValidator? scriptValidator = null,
            IReprocessValidator? reprocessValidator = null,
            IProjectNameValidator? projectNameValidator = null,
            ICommLineOut? output = null)
        {
            _directoryValidator = directoryValidator ?? new DirectoryValidator();
            _rhinoFileNameValidator = rhinoFileNameValidator ?? new RhinoFileNameValidator();
            _pidValidator = pidValidator ?? new PIDValidator();
            _scriptValidator = scriptValidator ?? new ScriptSettingsValidator();
            _reprocessValidator = reprocessValidator ?? new ReprocessValidator();
            _projectNameValidator = projectNameValidator ?? new ProjectNameValidator();
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        /// <summary>
        /// Validates the overall configuration by aggregating individual validation results.
        /// </summary>
        /// <param name="projectName">Project name settings.</param>
        /// <param name="directorySettings">Directory settings.</param>
        /// <param name="pidSettings">PID settings.</param>
        /// <param name="fileNameSettings">Rhino file name settings.</param>
        /// <param name="scriptSettings">Script settings.</param>
        /// <param name="reprocessSettings">Reprocessing settings.</param>
        /// <param name="timeoutSettings">Timeout settings.</param>
        /// <returns>A ValidationResult representing the overall configuration validity.</returns>
        public ValidationResult ValidateConfig(
            ProjectName projectName,
            DirectorySettings directorySettings,
            PIDSettings pidSettings,
            RhinoFileNameSettings fileNameSettings,
            ScriptSettings scriptSettings,
            ReprocessSettings reprocessSettings,
            TimeOutSettings timeoutSettings)
        {
            var errorList = new List<string>();

            // Validate project name.
            var projectNameResult = _projectNameValidator.ValidateConfig(projectName);
            errorList.AddRange(projectNameResult.Errors);

            // Validate directories.
            var dirResult = _directoryValidator.ValidateDirectories(new[]
            {
                directorySettings.FileDir,
                directorySettings.OutputDir,
                directorySettings.ScriptDir
            });
            errorList.AddRange(dirResult.Errors);

            // Validate PID settings.
            var pidResult = _pidValidator.ValidateConfig(pidSettings);
            errorList.AddRange(pidResult.Errors);

            // Validate Rhino file name settings.
            var fileNameResult = _rhinoFileNameValidator.ValidateConfig(fileNameSettings);
            errorList.AddRange(fileNameResult.Errors);

            // Validate script settings.
            var scriptResult = _scriptValidator.ValidateConfig(scriptSettings, directorySettings.ScriptDir);
            errorList.AddRange(scriptResult.Errors);

            // Validate reprocessing settings.
            var reprocessResult = _reprocessValidator.ValidateConfig(reprocessSettings);
            errorList.AddRange(reprocessResult.Errors);

            // Validate timeout settings.
            var timeoutResult = timeoutSettings.Validate();
            errorList.AddRange(timeoutResult.Errors);

            // Optionally log all errors.
            foreach (var error in errorList)
            {
                _output.ShowError($"Configuration validation error: {error}");
            }

            return new ValidationResult(errorList.Count == 0, errorList);
        }
    }
}
