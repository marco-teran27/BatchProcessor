using System;
using System.Collections.Generic;
using System.IO;
using BatchProcessor.Core.Config.Models;
using BatchProcessor.DI.Interfaces.Config;

namespace BatchProcessor.Core.Config.Validation
{
    /// <summary>
    /// Validates the reprocessing settings configuration.
    /// 
    /// In this updated version, errors are collected into a ValidationResult.
    /// No direct command-line output is performed. This enables all reporting to be handled
    /// by the centralized validation results pipeline.
    /// </summary>
    public class ReprocessValidator : IReprocessValidator
    {
        // ICommLineOut dependency removed; errors are now added to the ValidationResult only.

        /// <summary>
        /// Validates the provided reprocessing settings.
        /// </summary>
        /// <param name="settings">The reprocessing settings to validate.</param>
        /// <returns>A ValidationResult indicating whether the settings are valid and containing any error messages.</returns>
        public ValidationResult ValidateConfig(ReprocessSettings settings)
        {
            var errors = new List<string>();

            // Validate mode is not empty.
            if (string.IsNullOrWhiteSpace(settings.Mode))
            {
                errors.Add("Reprocess mode cannot be empty");
                return new ValidationResult(false, errors);
            }

            // Validate that the mode is one of the allowed values.
            var validModes = new[] { "ALL", "PASS", "FAIL", "RESUME" };
            if (!Array.Exists(validModes, mode => mode.Equals(settings.Mode.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add($"Invalid reprocess mode: {settings.Mode}. Must be one of: {string.Join(", ", validModes)}");
            }

            // For non-ALL modes, ensure ReferenceLog is provided and exists.
            if (!settings.Mode.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(settings.ReferenceLog))
                {
                    errors.Add("Reference log path required for non-ALL modes");
                }
                else if (!File.Exists(settings.ReferenceLog))
                {
                    errors.Add($"Reference log not found: {settings.ReferenceLog}");
                }
            }

            return new ValidationResult(errors.Count == 0, errors);
        }
    }
}
