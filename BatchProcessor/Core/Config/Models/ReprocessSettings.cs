using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using BatchProcessor.Core.Config.Validation;

/// <summary>
/// Represents settings for reprocessing operations.
/// 
/// Summary:
/// This model holds the reprocessing configuration (mode and reference log path).
/// It no longer contains its own validation logic; use ReprocessValidator for validation.
/// </summary>
namespace BatchProcessor.Core.Config.Models
{
    public class ReprocessSettings
    {
        /// <summary>
        /// Mode for reprocessing ("ALL", "PASS", "FAIL", or "RESUME").
        /// </summary>
        [JsonPropertyName("mode")]
        public string Mode { get; set; } = "ALL";

        /// <summary>
        /// Path to the reference log file used for reprocessing.
        /// </summary>
        [JsonPropertyName("reference_log")]
        public string ReferenceLog { get; set; } = string.Empty;
    }
}
