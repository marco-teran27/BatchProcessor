using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using BatchProcessor.Core.Config.Validation;

/// <summary>
/// Represents settings for filtering or processing Rhino file names.
/// 
/// Summary:
/// This model holds the configuration for Rhino file name filtering (mode and keywords).
/// All validation should be performed via the RhinoFileNameValidator in the validation modules.
/// </summary>
namespace BatchProcessor.Core.Config.Models
{
    public class RhinoFileNameSettings
    {
        /// <summary>
        /// The mode for processing file names ("list" or "all").
        /// </summary>
        [JsonPropertyName("mode")]
        public string Mode { get; set; } = string.Empty;

        /// <summary>
        /// List of keywords used when mode is "list" to filter file names.
        /// </summary>
        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; } = new();
    }
}
