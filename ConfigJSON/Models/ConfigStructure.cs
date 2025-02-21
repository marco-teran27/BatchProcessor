using System.Text.Json.Serialization;

namespace ConfigJSON.Models
{
    public class ConfigStructure
    {
        [JsonPropertyName("projectName")]
        public string ProjectName { get; set; } = string.Empty;

        [JsonPropertyName("directories")]
        public DirectorySettings Directories { get; set; } = new();

        [JsonPropertyName("script_settings")]
        public ScriptSettings ScriptSettings { get; set; } = new();

        [JsonPropertyName("rhino_file_name_settings")]
        public RhinoFileNameSettings RhinoFileNameSettings { get; set; } = new();

        [JsonPropertyName("pid_settings")]
        public PIDSettings PidSettings { get; set; } = new();

        [JsonPropertyName("timeout_settings")]
        public TimeOutSettings TimeoutMinutes { get; set; } = new();

        [JsonPropertyName("reprocess_settings")]
        public ReprocessSettings ReprocessSettings { get; set; } = new();

        [JsonIgnore]
        public string FilePath { get; set; } = string.Empty;
    }
}