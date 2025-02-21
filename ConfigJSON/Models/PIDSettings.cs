using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BatchProcessor.Core.Config.Validation;

/// <summary>
/// Represents configuration for PID‐based file selection.
/// 
/// Summary:
/// This model holds the PID selection settings. It no longer performs its own validation;
/// all validation is delegated to the corresponding validation module (PIDValidator).
/// </summary>
namespace BatchProcessor.ConfigJSON.Models
{
    public class PIDSettings
    {
        /// <summary>
        /// Mode for PID processing ("list" or "all").
        /// </summary>
        [JsonPropertyName("mode")]
        public string Mode { get; set; } = string.Empty;

        /// <summary>
        /// List of specific PIDs to process when mode is "list".
        /// </summary>
        [JsonPropertyName("pids")]
        public List<string> Pids { get; set; } = new();
    }
}
