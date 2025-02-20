using System.Text.Json.Serialization;

/*
File: BatchProcessor\Core\Config\Models\ProjectName.cs
Summary: Holds the project name as defined in the JSON config and the actual configuration file name.
         Used for validating that the file name embeds the correct project name.
*/

namespace BatchProcessor.Core.Config.Models
{
    /// <summary>
    /// Represents the project name configuration.
    /// </summary>
    public class ProjectName
    {
        /// <summary>
        /// The project name as defined in the JSON configuration.
        /// </summary>
        [JsonPropertyName("projectName")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The actual configuration file name (populated at runtime).
        /// </summary>
        [JsonIgnore]
        public string ActualConfigFileName { get; set; } = string.Empty;
    }
}
