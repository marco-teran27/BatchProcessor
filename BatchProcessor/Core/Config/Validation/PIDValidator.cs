using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BatchProcessor.Core.Config.Models;
using BatchProcessor.DI.Interfaces.Config;

namespace BatchProcessor.Core.Config.Validation
{
    /// <summary>
    /// Validates the PID settings configuration.
    /// 
    /// In this updated version, all error messages are added to a ValidationResult.
    /// No direct command-line output is performed, and the dependency on ICommLineOut has been removed.
    /// This makes error reporting consistent through the ValidationResult pipeline.
    /// </summary>
    public class PIDValidator : IPIDValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PidValidator"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor does not require an ICommLineOut parameter because all output is handled
        /// via the ValidationResult. This change is in line with the new design where error reporting
        /// is centralized.
        /// </remarks>
        public PIDValidator()
        {
            // No external output dependency is required.
        }

        /// <summary>
        /// Validates the provided PID settings.
        /// </summary>
        /// <param name="settings">The PID settings to validate.</param>
        /// <returns>
        /// A <see cref="ValidationResult"/> indicating whether the PID settings are valid along with any error messages.
        /// </returns>
        public ValidationResult ValidateConfig(PIDSettings settings)
        {
            var errors = new List<string>();

            // Ensure the mode is provided.
            if (string.IsNullOrWhiteSpace(settings.Mode))
            {
                errors.Add("PID mode cannot be empty.");
                return new ValidationResult(false, errors);
            }

            // Validate that the mode is either 'list' or 'all'.
            if (!IsValidMode(settings.Mode))
            {
                errors.Add($"Invalid PID mode: {settings.Mode}. Must be either 'list' or 'all'.");
            }

            // If mode is "list", ensure that the PIDs list is not empty and each PID has the correct format.
            if (settings.Mode.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                if (settings.Pids == null || !settings.Pids.Any())
                {
                    errors.Add("PIDs list cannot be empty when mode is 'list'.");
                }
                else
                {
                    foreach (var pid in settings.Pids)
                    {
                        if (!Regex.IsMatch(pid, @"^\d{6}[LR]-[SR]\d{5}$", RegexOptions.IgnoreCase))
                        {
                            errors.Add($"Invalid PID format: {pid}");
                        }
                    }
                }
            }

            return new ValidationResult(errors.Count == 0, errors);
        }

        /// <summary>
        /// Determines whether the provided mode is valid.
        /// </summary>
        /// <param name="mode">The PID mode to validate.</param>
        /// <returns>True if mode is either 'list' or 'all'; otherwise, false.</returns>
        private bool IsValidMode(string mode)
        {
            return mode.Equals("list", StringComparison.OrdinalIgnoreCase) ||
                   mode.Equals("all", StringComparison.OrdinalIgnoreCase);
        }
    }
}
