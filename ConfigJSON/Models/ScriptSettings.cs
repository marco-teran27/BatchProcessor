// File: ConfigJSON\Models\ScriptSettings.cs
using System.Text.Json.Serialization;
using Commons; // For ScriptType

namespace ConfigJSON.Models
{
    public class ScriptSettings
    {
        [JsonPropertyName("script_path")]
        public string ScriptPath { get; set; } = string.Empty;

        [JsonPropertyName("script_type")]
        public ScriptType ScriptType { get; set; } = ScriptType.Python; // Default to Python
    }
}