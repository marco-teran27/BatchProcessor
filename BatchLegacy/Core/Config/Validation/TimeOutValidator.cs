using System;
using System.Collections.Generic;
using BatchProcessor.DI.Interfaces.Config;

/*
File: BatchProcessor\Core\Config\Validation\TimeOutValidator.cs
Summary: Implements ITimeOutValidator to validate that the timeout value (in minutes)
         is a positive integer. Returns a ValidationResult without directly using ICommLineOut.
*/

namespace BatchProcessor.Core.Config.Validation
{
    /// <summary>
    /// Provides validation for the numeric "minutes" field in timeout settings.
    /// </summary>
    public class TimeOutValidator : ITimeOutValidator
    {
        /// <summary>
        /// Validates that the specified timeout in minutes is a positive integer.
        /// </summary>
        /// <param name="timeoutMinutes">The timeout value to validate.</param>
        /// <returns>A ValidationResult indicating whether the timeout value is valid.</returns>
        public ValidationResult ValidateTimeout(int timeoutMinutes)
        {
            var errors = new List<string>();

            if (timeoutMinutes <= 0)
            {
                string msg = "Configuration Error: 'minutes' in 'timeout_settings' must be a positive integer.";
                errors.Add(msg);
            }

            return new ValidationResult(errors.Count == 0, errors);
        }
    }
}
