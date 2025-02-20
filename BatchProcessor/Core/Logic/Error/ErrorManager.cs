using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BatchProcessor.DI.Interfaces.AbsRhino;    // For ICommLineOut, etc.
using BatchProcessor.DI.Interfaces.Error;      // Uses the interface’s ErrorSeverity (Low, Medium, High, Critical)
using Microsoft.Extensions.Logging;

namespace BatchProcessor.Core.Logic.Error
{
    /// <summary>
    /// Enhanced error management with categorization, tracking, and recovery strategies.
    /// </summary>
    public class ErrorManager
    {
        private readonly ICommLineOut _output;
        private readonly ILogger<ErrorManager> _logger;
        private readonly ConcurrentDictionary<string, List<ErrorInfo>> _errorHistory;
        private readonly ConcurrentDictionary<string, ErrorStats> _errorStats;
        private readonly ErrorRecoveryStrategy _recoveryStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorManager"/> class.
        /// </summary>
        /// <param name="output">The command-line output interface.</param>
        /// <param name="logger">The logger instance for diagnostic messages.</param>
        public ErrorManager(ICommLineOut output, ILogger<ErrorManager> logger)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _errorHistory = new ConcurrentDictionary<string, List<ErrorInfo>>();
            _errorStats = new ConcurrentDictionary<string, ErrorStats>();
            _recoveryStrategy = new ErrorRecoveryStrategy(logger);
        }

        /// <summary>
        /// Logs and handles an error during processing.
        /// </summary>
        /// <param name="fileName">The name of the file being processed.</param>
        /// <param name="ex">The exception that occurred.</param>
        /// <param name="severity">The severity of the error.</param>
        /// <returns>
        /// A task returning an <see cref="ErrorResponse"/> that encapsulates the error details and the recommended recovery action.
        /// </returns>
        public async Task<ErrorResponse> HandleError(string fileName, Exception ex, ErrorSeverity severity)
        {
            await Task.CompletedTask;

            var errorInfo = new ErrorInfo
            {
                Timestamp = DateTime.Now,
                Severity = severity,
                Message = ex.Message,
                Exception = ex,
                Category = CategorizeError(ex)
            };

            // Add to history
            _errorHistory.AddOrUpdate(
                fileName,
                new List<ErrorInfo> { errorInfo },
                (_, list) =>
                {
                    list.Add(errorInfo);
                    return list;
                }
            );

            // Update statistics
            UpdateErrorStats(fileName, errorInfo);

            // Log error
            _logger.LogError(ex, $"Error processing {fileName}: {ex.Message}");
            _output.ShowError($"{severity}: {ex.Message}");

            // Determine recovery action
            var recoveryAction = await _recoveryStrategy.DetermineRecoveryAction(fileName, errorInfo);

            return new ErrorResponse
            {
                ErrorInfo = errorInfo,
                RecoveryAction = recoveryAction,
                ShouldRetry = recoveryAction.ShouldRetry,
                RetryDelay = recoveryAction.RetryDelay
            };
        }

        /// <summary>
        /// Categorizes an error based on its type and message.
        /// </summary>
        /// <param name="ex">The exception to categorize.</param>
        /// <returns>An <see cref="ErrorCategory"/> indicating the error category.</returns>
        private ErrorCategory CategorizeError(Exception ex)
        {
            return ex switch
            {
                IOException _ => ErrorCategory.IO,
                OutOfMemoryException _ => ErrorCategory.Resource,
                TimeoutException _ => ErrorCategory.Timeout,
                UnauthorizedAccessException _ => ErrorCategory.Permission,
                _ => ErrorCategory.General
            };
        }

        /// <summary>
        /// Updates error statistics for a file.
        /// </summary>
        /// <param name="fileName">The file name to update statistics for.</param>
        /// <param name="error">The error information to record.</param>
        private void UpdateErrorStats(string fileName, ErrorInfo error)
        {
            _errorStats.AddOrUpdate(
                fileName,
                new ErrorStats { LastError = error },
                (_, stats) =>
                {
                    stats.ErrorCount++;
                    stats.LastError = error;
                    stats.UpdateErrorRate();
                    return stats;
                }
            );
        }

        /// <summary>
        /// Gets error statistics for a given file.
        /// </summary>
        /// <param name="fileName">The file name to retrieve statistics for.</param>
        /// <returns>An <see cref="ErrorStats"/> instance representing the error statistics.</returns>
        public ErrorStats GetErrorStats(string fileName)
        {
            return _errorStats.GetValueOrDefault(fileName) ?? new ErrorStats();
        }

        /// <summary>
        /// Gets the complete error history for a given file.
        /// </summary>
        /// <param name="fileName">The file name to retrieve history for.</param>
        /// <returns>A read-only list of <see cref="ErrorInfo"/> instances.</returns>
        public IReadOnlyList<ErrorInfo> GetErrorHistory(string fileName)
        {
            if (_errorHistory.TryGetValue(fileName, out var list))
            {
                return list.AsReadOnly();
            }
            else
            {
                return new List<ErrorInfo>().AsReadOnly();
            }
        }

        /// <summary>
        /// Clears the error history and statistics for a given file.
        /// </summary>
        /// <param name="fileName">The file name to clear history for.</param>
        public void ClearErrorHistory(string fileName)
        {
            _errorHistory.TryRemove(fileName, out _);
            _errorStats.TryRemove(fileName, out _);
        }

        /// <summary>
        /// Gets an overall analysis of errors across the batch.
        /// </summary>
        /// <returns>A <see cref="BatchErrorAnalysis"/> containing aggregated error information.</returns>
        public BatchErrorAnalysis GetBatchErrorAnalysis()
        {
            var analysis = new BatchErrorAnalysis();

            foreach (var stats in _errorStats.Values)
            {
                analysis.TotalErrors += stats.ErrorCount;
                if (analysis.ErrorsByCategory.ContainsKey(stats.LastError.Category))
                {
                    analysis.ErrorsByCategory[stats.LastError.Category]++;
                }
                else
                {
                    analysis.ErrorsByCategory[stats.LastError.Category] = 1;
                }

                if (stats.ErrorRate > analysis.HighestErrorRate)
                {
                    analysis.HighestErrorRate = stats.ErrorRate;
                }
            }

            return analysis;
        }
    }

    /// <summary>
    /// Represents error statistics for a specific file.
    /// </summary>
    public class ErrorStats
    {
        public int ErrorCount { get; set; }
        public ErrorInfo LastError { get; set; } = new ErrorInfo();
        public double ErrorRate { get; private set; }
        private DateTime _firstErrorTime = DateTime.Now;

        /// <summary>
        /// Updates the error rate based on the duration since the first error.
        /// </summary>
        public void UpdateErrorRate()
        {
            var duration = DateTime.Now - _firstErrorTime;
            ErrorRate = duration.TotalHours > 0 ? ErrorCount / duration.TotalHours : ErrorCount;
        }
    }

    /// <summary>
    /// Provides an analysis summary of errors for the entire batch.
    /// </summary>
    public class BatchErrorAnalysis
    {
        public int TotalErrors { get; set; }
        public Dictionary<ErrorCategory, int> ErrorsByCategory { get; } = new Dictionary<ErrorCategory, int>();
        public double HighestErrorRate { get; set; }
    }

    /// <summary>
    /// Enumerates possible error categories.
    /// </summary>
    public enum ErrorCategory
    {
        General,
        IO,
        Resource,
        Timeout,
        Permission
    }

    /// <summary>
    /// Represents the response from an error handling operation,
    /// including details about the error and a recommended recovery action.
    /// </summary>
    public class ErrorResponse
    {
        public ErrorInfo ErrorInfo { get; set; } = new ErrorInfo();
        public RecoveryAction RecoveryAction { get; set; } = new RecoveryAction();
        public bool ShouldRetry { get; set; }
        public TimeSpan RetryDelay { get; set; }
    }

    /// <summary>
    /// Represents a recommended recovery action for an error.
    /// </summary>
    public class RecoveryAction
    {
        public bool ShouldRetry { get; set; }
        public TimeSpan RetryDelay { get; set; }
        public string RecoverySteps { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents detailed information about an error.
    /// </summary>
    public class ErrorInfo
    {
        public DateTime Timestamp { get; set; }
        public ErrorSeverity Severity { get; set; }  // Uses the interface’s ErrorSeverity (Low, Medium, High, Critical)
        public string Message { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
        /// <summary>
        /// The category of the error. This property is now added to allow categorization of errors.
        /// </summary>
        public ErrorCategory Category { get; set; }
    }

    /// <summary>
    /// Provides a recovery strategy for errors.
    /// </summary>
    public class ErrorRecoveryStrategy
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorRecoveryStrategy"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public ErrorRecoveryStrategy(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Determines the appropriate recovery action for an error.
        /// </summary>
        /// <param name="fileName">The file for which the error occurred.</param>
        /// <param name="error">The error information.</param>
        /// <returns>A task that returns a <see cref="RecoveryAction"/> indicating how to proceed.</returns>
        public async Task<RecoveryAction> DetermineRecoveryAction(string fileName, ErrorInfo error)
        {
            await Task.CompletedTask;

            var action = new RecoveryAction();

            switch (error.Category)
            {
                case ErrorCategory.IO:
                    action.ShouldRetry = true;
                    action.RetryDelay = TimeSpan.FromSeconds(5);
                    action.RecoverySteps = "Wait for file system availability";
                    break;
                case ErrorCategory.Resource:
                    action.ShouldRetry = true;
                    action.RetryDelay = TimeSpan.FromMinutes(1);
                    action.RecoverySteps = "Wait for resource availability";
                    break;
                case ErrorCategory.Timeout:
                    action.ShouldRetry = true;
                    action.RetryDelay = TimeSpan.FromSeconds(30);
                    action.RecoverySteps = "Retry with increased timeout";
                    break;
                case ErrorCategory.Permission:
                    action.ShouldRetry = false;
                    action.RecoverySteps = "Check file permissions";
                    break;
                default:
                    action.ShouldRetry = error.Severity != BatchProcessor.Core.Interfaces.Error.ErrorSeverity.Critical;
                    action.RetryDelay = TimeSpan.FromSeconds(10);
                    action.RecoverySteps = "Standard retry";
                    break;
            }

            _logger.LogInformation($"Recovery action for {fileName}: {action.RecoverySteps}");

            return action;
        }
    }
}
