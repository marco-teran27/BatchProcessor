// File: ConfigJSON\Models\RhinoFileNameSettings.cs
using System.Text.Json.Serialization;

namespace ConfigJSON.Models
{
    public class RhinoFileNameSettings
    {
        [JsonPropertyName("file_name_pattern")]
        public string FileNamePattern { get; set; } = string.Empty;
    }
}