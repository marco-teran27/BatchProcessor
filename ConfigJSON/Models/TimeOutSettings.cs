using System;
using System.Text.Json.Serialization;
using Commons;
using BatchProcessor.DI.Interfaces.Config;
using BatchProcessor.Core.Config.Validation;

/// <summary>
/// Represents timeout settings for batch processing.
/// 
/// Summary:
/// This model contains the timeout value (in minutes) for batch processing.
/// Validation is handled through the TimeOutValidator in the validation modules.
/// </summary>
namespace BatchProcessor.ConfigJSON.Models
{
    public class TimeOutSettings
    {
        /// <summary>
        /// Timeout value in minutes.
        /// </summary>
        [JsonPropertyName("minutes")]
        public int Minutes { get; set; } = 8;

        private readonly ITimeOutValidator _validator;

        /// <summary>
        /// Parameterless constructor for deserialization.
        /// </summary>
        public TimeOutSettings() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of TimeOutSettings with an optional validator.
        /// </summary>
        /// <param name="validator">
        /// An optional ITimeOutValidator. If not provided, a default TimeOutValidator is used.
        /// </param>
        public TimeOutSettings(ITimeOutValidator? validator = null)
        {
            _validator = validator ?? new TimeOutValidator();
        }

        /// <summary>
        /// Validates the timeout settings by ensuring the 'Minutes' value is positive.
        /// </summary>
        /// <returns>
        /// A ValidationResult indicating whether the timeout settings are valid,
        /// including any error messages.
        /// </returns>
        public ValidationResult Validate()
        {
            return _validator.ValidateTimeout(Minutes);
        }
    }
}
