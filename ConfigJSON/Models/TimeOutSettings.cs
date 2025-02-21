// File: ConfigJSON\Models\TimeOutSettings.cs
using System.Text.Json.Serialization;

namespace ConfigJSON.Models
{
    public class TimeOutSettings
    {
        [JsonPropertyName("timeout_minutes")]
        public int Minutes { get; set; } = 5; // Default value as placeholder
    }
}