// File: ConfigJSON\Models\LoggingConfig.cs
using System.Text.Json.Serialization;

namespace ConfigJSON.Models
{
    public class LoggingConfig
    {
        [JsonPropertyName("log_path")]
        public string LogPath { get; set; } = string.Empty;
    }
}