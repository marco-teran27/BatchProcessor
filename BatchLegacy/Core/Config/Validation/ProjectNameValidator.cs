using System;
using System.Collections.Generic;
using BatchProcessor.Core.Config.Models;
using BatchProcessor.DI.Interfaces.Config;

/*
File: BatchProcessor\Core\Config\Validation\ProjectNameValidator.cs
Summary: Implements IProjectNameValidator to verify that the project name provided in the JSON config
         is non-empty and matches the substring extracted from the actual configuration file name.
         This module returns a ValidationResult to report validation status and errors.
*/

namespace BatchProcessor.Core.Config.Validation
{
    /// <summary>
    /// Validates the project name by ensuring that:
    /// 1) The JSON project name is not empty.
    /// 2) The actual config file name is provided and follows the naming convention "config-{ProjectName}.json".
    /// 3) The project name extracted from the file name matches the JSON project name.
    /// </summary>
    public class ProjectNameValidator : IProjectNameValidator
    {
        /// <summary>
        /// Validates the given <see cref="ProjectName"/> instance.
        /// </summary>
        /// <param name="projectName">
        /// The <see cref="ProjectName"/> object containing the JSON project name and the actual config file name.
        /// </param>
        /// <returns>
        /// A <see cref="ValidationResult"/> indicating whether the project name is valid, and containing any error messages.
        /// </returns>
        public ValidationResult ValidateConfig(ProjectName projectName)
        {
            var errors = new List<string>();

            // 1) Check that the JSON project name is provided.
            if (string.IsNullOrWhiteSpace(projectName.Name))
            {
                errors.Add("projectName cannot be empty in the JSON config.");
            }

            // 2) Ensure that the actual config file name is provided.
            if (string.IsNullOrWhiteSpace(projectName.ActualConfigFileName))
            {
                errors.Add("Actual config file name is missing (could not validate).");
            }
            else
            {
                // 3) Confirm the file name follows the "config-{ProjectName}.json" pattern.
                var fileNameLower = projectName.ActualConfigFileName.ToLowerInvariant();
                if (!fileNameLower.StartsWith("config-") || !fileNameLower.EndsWith(".json"))
                {
                    errors.Add($"Config file '{projectName.ActualConfigFileName}' does not follow 'config-{{ProjectName}}.json' naming.");
                }
                else
                {
                    // Extract the substring between "config-" and ".json".
                    var expectedName = projectName.ActualConfigFileName
                        .Replace("config-", "", StringComparison.OrdinalIgnoreCase)
                        .Replace(".json", "", StringComparison.OrdinalIgnoreCase);

                    if (!string.Equals(projectName.Name, expectedName, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add($"Mismatch: JSON projectName = '{projectName.Name}', file name substring = '{expectedName}'.");
                    }
                }
            }

            return new ValidationResult(errors.Count == 0, errors);
        }
    }
}
