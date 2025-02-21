// File: ConfigJSON\Models\PIDSettings.cs
using System.Text.Json.Serialization;

namespace ConfigJSON.Models
{
    public class PIDSettings
    {
        [JsonPropertyName("pid_pattern")]
        public string PIDPattern { get; set; } = string.Empty;
    }
}