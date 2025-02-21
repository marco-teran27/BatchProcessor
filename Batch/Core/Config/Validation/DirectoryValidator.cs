using System;
using System.Collections.Generic;
using System.IO;
using BatchProcessor.DI.Interfaces.Config;

/*
File: BatchProcessor\Core\Config\Validation\DirectoryValidator.cs
Summary: Implements IDirectoryValidator to verify that directory paths are valid and accessible.
         Returns a ValidationResult containing success flag and error messages.
*/

namespace BatchProcessor.Core.Config.Validation
{
    /// <summary>
    /// Validates directory existence and permissions.
    /// </summary>
    public class DirectoryValidator : IDirectoryValidator
    {
        /// <summary>
        /// Validates that each provided directory is non-empty and exists.
        /// </summary>
        /// <param name="directories">Array of directory paths.</param>
        /// <returns>A ValidationResult with overall status and error messages.</returns>
        public ValidationResult ValidateDirectories(string[] directories)
        {
            var errors = new List<string>();

            foreach (var directory in directories)
            {
                if (string.IsNullOrWhiteSpace(directory))
                {
                    errors.Add("Directory path cannot be empty");
                    continue;
                }

                try
                {
                    if (!Directory.Exists(directory))
                    {
                        errors.Add($"Directory not found: {directory}");
                        continue;
                    }
                    // Additional permission checks can be added here.
                }
                catch (UnauthorizedAccessException)
                {
                    errors.Add($"Access denied to directory: {directory}");
                }
                catch (Exception ex)
                {
                    errors.Add($"Error validating directory {directory}: {ex.Message}");
                }
            }

            return new ValidationResult(errors.Count == 0, errors);
        }
    }
}
