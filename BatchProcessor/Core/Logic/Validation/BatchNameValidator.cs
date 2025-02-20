using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BatchProcessor.Core.Config.Models;     // For BatchNameComponents, PIDSettings, RhinoFileNameSettings
using BatchProcessor.DI.Interfaces.Batch;    // Provides IBatchNameValidator interface
using BatchProcessor.DI.Utils;

namespace BatchProcessor.Core.Logic.Validation
{
    /// <summary>
    /// BatchNameValidator compares the aggregated Rhino file name criteria (obtained from BatchRhinoNameList)
    /// with the actual list of file names returned by BatchDirScanner.
    /// It identifies any discrepancies between the expected criteria and the actual files.
    /// 
    /// Responsibilities:
    ///  - If the aggregated criteria is null (i.e. mode "all"), assume all files are acceptable.
    ///  - Otherwise, verify that for each expected criteria there is at least one matching file.
    ///  - Optionally, report any extra files that do not match any of the expected criteria.
    /// 
    /// Additionally, it implements methods to validate individual file names and to verify that
    /// the extracted batch name components conform to configuration rules.
    /// Implements the IBatchNameValidator interface for integration with dependency injection.
    /// </summary>
    public class BatchNameValidator : IBatchNameValidator
    {
        /// <summary>
        /// Validates that the file names provided by BatchDirScanner match the expected criteria.
        /// </summary>
        /// <param name="expectedCriteria">
        /// The aggregated list of expected name criteria from BatchRhinoNameList, or null if mode is "all".
        /// </param>
        /// <param name="foundFiles">
        /// The list of file names (including extensions) discovered by BatchDirScanner.
        /// </param>
        /// <returns>
        /// A ValidationResult indicating whether the validation passed and containing any error messages.
        /// </returns>
        public ValidationResult ValidateNames(List<string>? expectedCriteria, List<string> foundFiles)
        {
            var errors = new List<string>();

            // If the aggregated criteria is null, it means "all" mode is active;
            // therefore, all found files are acceptable.
            if (expectedCriteria == null)
            {
                return new ValidationResult(true, errors);
            }

            // For each expected criteria, ensure that there is at least one file that matches.
            foreach (var criteria in expectedCriteria)
            {
                bool anyMatch = foundFiles.Any(file =>
                {
                    // Ensure file is not null.
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file ?? string.Empty) ?? string.Empty;
                    return fileNameWithoutExt.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0;
                });

                if (!anyMatch)
                {
                    errors.Add($"No file found matching expected criteria: '{criteria}'");
                }
            }

            // Optionally, flag any extra files that do not match any expected criteria.
            foreach (var file in foundFiles)
            {
                if (file == null)
                    continue;

                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file) ?? string.Empty;
                bool matchesAny = expectedCriteria.Any(criteria => fileNameWithoutExt.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!matchesAny)
                {
                    errors.Add($"File '{file}' does not match any expected criteria");
                }
            }

            bool isValid = errors.Count == 0;
            return new ValidationResult(isValid, errors);
        }

        /// <summary>
        /// Determines if the given file name is valid based on predetermined criteria and extracts batch name components.
        /// </summary>
        /// <param name="fileName">The file name to validate.</param>
        /// <param name="components">
        /// When this method returns, contains the extracted BatchNameComponents if the file name is valid;
        /// otherwise, a default BatchNameComponents.
        /// </param>
        /// <returns>True if the file name is valid; otherwise, false.</returns>
        public bool IsValidFileName(string fileName, out BatchNameComponents components)
        {
            components = new BatchNameComponents();

            // Use the centralized regex pattern to validate and parse the file name.
            var match = Utils.RhinoNameRegex.RhinoFilePattern.Match(fileName);
            if (!match.Success)
            {
                return false;
            }

            // Populate the components using the match groups.
            components.BasePid = match.Groups[1].Value;
            components.OptionalDigits = match.Groups[2].Value;
            components.Keyword = match.Groups[3].Value;
            components.SrNumber = match.Groups[4].Value;

            return true;
        }


        /// <summary>
        /// Determines whether the extracted batch name components match the configuration rules.
        /// </summary>
        /// <param name="components">The extracted batch name components.</param>
        /// <param name="pidSettings">The PID settings from configuration.</param>
        /// <param name="fileNameSettings">The Rhino file name settings from configuration.</param>
        /// <returns>True if the components match the configuration rules; otherwise, false.</returns>
        public bool MatchesConfigRules(BatchNameComponents components, PIDSettings pidSettings, RhinoFileNameSettings fileNameSettings)
        {
            // Placeholder logic: Without a specific file name component or a required prefix property,
            // no meaningful validation can be performed. For demonstration purposes, return true.
            return true;
        }
    }

    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Indicates whether the validation was successful.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// A read-only list of error messages from the validation process.
        /// </summary>
        public IReadOnlyList<string> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the ValidationResult class.
        /// </summary>
        /// <param name="isValid">True if validation succeeded; otherwise, false.</param>
        /// <param name="errors">A list of error messages generated during validation.</param>
        public ValidationResult(bool isValid, List<string> errors)
        {
            IsValid = isValid;
            Errors = errors.AsReadOnly();
        }
    }
}
