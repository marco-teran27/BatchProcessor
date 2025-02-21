using System.Text.Json.Serialization;

/*
Summary: Represents the top‐level JSON structure for the configuration file.
         Consolidates all sub‐models (directories, script settings, etc.) for one‐pass JSON parsing.
*/

namespace ConfigJSON.Models
{
    /// <summary>
    /// Represents the complete configuration structure for batch processing.
    /// </summary>
    public class ConfigStructure
    {
        /// <summary>
        /// Project name specified in the JSON config.
        /// </summary>
        [JsonPropertyName("projectName")]
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// Directory settings for file, output, and script paths.
        /// </summary>
        [JsonPropertyName("directories")]
        public DirectorySettings Directories { get; set; } = new();

        /// <summary>
        /// Script settings including script name and type.
        /// </summary>
        [JsonPropertyName("script_settings")]
        public ScriptSettings Script_Settings { get; set; } = new();

        /// <summary>
        /// Rhino file name settings for filtering file names.
        /// </summary>
        [JsonPropertyName("rhino_file_name_settings")]
        public RhinoFileNameSettings Rhino_File_Name_Settings { get; set; } = new();

        /// <summary>
        /// PID settings for selecting specific files.
        /// </summary>
        [JsonPropertyName("pid_settings")]
        public PIDSettings Pid_Settings { get; set; } = new();

        /// <summary>
        /// Timeout settings for batch processing.
        /// </summary>
        [JsonPropertyName("timeout_settings")]
        public TimeOutSettings Timeout_Minutes { get; set; } = new();

        /// <summary>
        /// Reprocessing settings for retrying failed operations.
        /// </summary>
        [JsonPropertyName("reprocess_settings")]
        public ReprocessSettings Reprocess_Settings { get; set; } = new();
    }
}
