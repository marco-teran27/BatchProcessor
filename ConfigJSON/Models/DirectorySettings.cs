using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using BatchProcessor.DI.Interfaces.Config;
using BatchProcessor.Core.Config.Validation;

namespace ConfigJSON.Models
{
    /// <summary>
    /// Represents directory-related configuration settings.
    /// </summary>
    public class DirectorySettings
    {
        /// <summary>
        /// Path to the directory containing input files.
        /// </summary>
        [JsonPropertyName("file_dir")]
        public string FileDir { get; set; } = string.Empty;

        /// <summary>
        /// Path to the directory for output files.
        /// </summary>
        [JsonPropertyName("output_dir")]
        public string OutputDir { get; set; } = string.Empty;

        /// <summary>
        /// Path to the directory containing scripts.
        /// </summary>
        [JsonPropertyName("script_dir")]
        public string ScriptDir { get; set; } = string.Empty;

        private readonly IDirectoryValidator _validator;

        // Add an explicit parameterless constructor for deserialization.
        public DirectorySettings() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of DirectorySettings with an optional validator.
        /// </summary>
        /// <param name="validator">An optional IDirectoryValidator; if null, a default DirectoryValidator is used.</param>
        public DirectorySettings(IDirectoryValidator? validator = null)
        {
            _validator = validator ?? new DirectoryValidator();
        }

        /// <summary>
        /// Validates that the directory paths are non-empty and exist.
        /// </summary>
        /// <returns>A ValidationResult with the validation outcome and error messages.</returns>
        public ValidationResult ValidatePermissions()
        {
            return _validator.ValidateDirectories(new[] { FileDir, OutputDir, ScriptDir });
        }
    }
}
