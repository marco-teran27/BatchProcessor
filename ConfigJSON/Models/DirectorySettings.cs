// File: ConfigJSON\Models\DirectorySettings.cs
using System.Text.Json.Serialization;

namespace ConfigJSON.Models
{
    public class DirectorySettings
    {
        [JsonPropertyName("file_dir")]
        public string FileDir { get; set; } = string.Empty;

        [JsonPropertyName("output_dir")]
        public string OutputDir { get; set; } = string.Empty;
    }
}