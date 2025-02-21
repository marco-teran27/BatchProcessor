// File: ConfigJSON\Models\ReprocessSettings.cs
using System.Text.Json.Serialization;

namespace ConfigJSON.Models
{
    public class ReprocessSettings
    {
        [JsonPropertyName("reprocess_enabled")]
        public bool ReprocessEnabled { get; set; } = false;
    }
}