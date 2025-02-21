using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BatchProcessor.Core.Config.Models;
using BatchProcessor.DI.Interfaces.Config;
using BatchProcessor.DI.Utils;

/*
File: BatchProcessor\Core\Config\Validation\RhinoFileNameValidator.cs
Summary: Implements IRhinoFileNameValidator to validate Rhino file name settings and individual file names.
         Returns a ValidationResult for configuration validation.
*/

namespace BatchProcessor.Core.Config.Validation
{
    /// <summary>
    /// Validates Rhino file name settings and individual file names.
    /// </summary>
    public class RhinoFileNameValidator : IRhinoFileNameValidator
    {
        private static readonly Regex ConfigFilePattern = new Regex(
            @"^config-(.+)\.json$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        /// <summary>
        /// Checks if a config file name is valid and extracts the project name.
        /// </summary>
        /// <param name="fileName">The configuration file name.</param>
        /// <param name="projectName">Output project name if valid.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        public bool IsValidConfigFileName(string fileName, out string? projectName)
        {
            projectName = null;
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var match = ConfigFilePattern.Match(fileName);
            if (!match.Success)
                return false;

            projectName = match.Groups[1].Value;
            return true;
        }

        /// <summary>
        /// Checks if a Rhino file name matches the expected pattern.
        /// </summary>
        /// <param name="fileName">The file name to validate.</param>
        /// <param name="parts">Output tuple containing (pid, keyword, srNumber) if valid.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        public bool IsValidRhinoFileName(string fileName, out (string pid, string keyword, string srNumber) parts)
        {
            parts = default;
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var match = RhinoNameRegex.RhinoFilePattern.Match(fileName);
            if (!match.Success)
                return false;

            parts = (
                pid: match.Groups[1].Value + match.Groups[2].Value,
                keyword: match.Groups[3].Value,
                srNumber: match.Groups[4].Value
            );
            return true;
        }

        /// <summary>
        /// Validates the Rhino file name settings.
        /// </summary>
        /// <param name="settings">The Rhino file name settings to validate.</param>
        /// <returns>A ValidationResult indicating whether the settings are valid.</returns>
        public ValidationResult ValidateConfig(RhinoFileNameSettings settings)
        {
            var errors = new List<string>();

            if (settings == null)
            {
                errors.Add("RhinoFileNameSettings is null.");
                return new ValidationResult(false, errors);
            }

            if (string.IsNullOrWhiteSpace(settings.Mode))
            {
                errors.Add("File name mode cannot be empty.");
            }
            else if (!IsValidMode(settings.Mode))
            {
                errors.Add($"Invalid file name mode: {settings.Mode}. Must be 'list' or 'all'.");
            }

            if (settings.Mode.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                if (settings.Keywords == null || settings.Keywords.Count == 0)
                {
                    errors.Add("Keywords list cannot be empty when mode is 'list'.");
                }
            }

            return new ValidationResult(errors.Count == 0, errors);
        }

        /// <summary>
        /// Checks if the file name mode is valid.
        /// </summary>
        /// <param name="mode">The mode string.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        private bool IsValidMode(string mode)
        {
            return mode.Equals("list", StringComparison.OrdinalIgnoreCase) ||
                   mode.Equals("all", StringComparison.OrdinalIgnoreCase);
        }
    }
}
