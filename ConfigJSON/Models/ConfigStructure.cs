using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace ConfigJSON.Models
{
    public class ConfigStructure
    {
        [JsonPropertyName("projectName")]
        public string ProjectName { get; set; } = string.Empty;

        [JsonPropertyName("directories")]
        public DirectorySettings Directories { get; set; } = new DirectorySettings();

        [JsonPropertyName("script_settings")]
        public ScriptSettings ScriptSettings { get; set; } = new ScriptSettings();

        [JsonPropertyName("rhino_file_name_settings")]
        public RhinoFileNameSettings RhinoFileNameSettings { get; set; } = new RhinoFileNameSettings();

        [JsonPropertyName("pid_settings")]
        public PIDSettings PidSettings { get; set; } = new PIDSettings();

        [JsonPropertyName("timeout_settings")]
        public TimeOutSettings TimeoutMinutes { get; set; } = new TimeOutSettings();

        [JsonPropertyName("reprocess_settings")]
        public ReprocessSettings ReprocessSettings { get; set; } = new ReprocessSettings();

        [JsonIgnore]
        public string FilePath { get; set; } = string.Empty;
    }
}