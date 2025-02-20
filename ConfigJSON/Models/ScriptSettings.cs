using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using BatchProcessor.Core.Config.Validation;

/// <summary>
/// Holds the script settings for batch processing.
/// 
/// Summary:
/// This model contains the script file name and type. Its validation is handled by
/// the ScriptSettingsValidator in the validation modules.
/// </summary>
namespace ConfigJSON
{
    public class ScriptSettings
    {
        /// <summary>
        /// The name of the script file (without extension).
        /// </summary>
        [JsonPropertyName("script_name")]
        public string ScriptName { get; set; } = string.Empty;

        /// <summary>
        /// The type of the script (e.g., Python, Grasshopper, etc.).
        /// </summary>
        [JsonPropertyName("script_type")]
        public ScriptType ScriptType { get; set; } = ScriptType.Python;
    }
}
