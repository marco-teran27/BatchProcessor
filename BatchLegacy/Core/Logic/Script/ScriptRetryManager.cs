using System;
using System.Threading.Tasks;
using BatchProcessor.DI.Interfaces.AbsRhino;
using BatchProcessor.DI.Interfaces.Logging;
using BatchProcessor.DI.Interfaces.Monitoring;
using System.IO;

namespace BatchProcessor.Core.Logic.Script
{
    /// <summary>
    /// Manages retry logic for operations such as script execution.
    /// This class provides methods to retry Rhino script execution and system operations with delays and jitter.
    /// It leverages the command-line output, logging, and system monitoring interfaces.
    /// </summary>
    public class ScriptRetryManager
    {
        private readonly ICommLineOut _output;
        private readonly IBatchLogger _logger;
        private readonly ISystemsMonitor _monitor;
        private const int BASE_DELAY_MS = 1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptRetryManager"/> class.
        /// </summary>
        /// <param name="output">Interface for command-line output.</param>
        /// <param name="logger">Logger for diagnostic messages.</param>
        /// <param name="monitor">System monitor for capturing resource metrics.</param>
        public ScriptRetryManager(ICommLineOut output, IBatchLogger logger, ISystemsMonitor monitor)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        }

        /// <summary>
        /// Retries Rhino script execution once asynchronously.
        /// </summary>
        /// <param name="rhinoApp">The Rhino application wrapper.</param>
        /// <param name="command">The script command to execute.</param>
        /// <param name="fileName">The name of the file being processed.</param>
        /// <returns>A task that returns true if the script execution succeeds on retry; otherwise, false.</returns>
        public async Task<bool> RetryRhinoScript(IRhinoApp rhinoApp, string command, string fileName)
        {
            try
            {
                if (!rhinoApp.RunScript(command, false))
                {
                    await Task.Delay(BASE_DELAY_MS);
                    var retrySuccess = rhinoApp.RunScript(command, false);
                    if (!retrySuccess)
                    {
                        _output.ShowError($"Failed to start script for {fileName} after retry");
                    }
                    return retrySuccess;
                }
                return true;
            }
            catch (Exception ex)
            {
                _output.ShowError($"Error executing script: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retries a system operation asynchronously with up to 2 attempts.
        /// </summary>
        /// <param name="fileName">The name of the file for which the operation is attempted.</param>
        /// <param name="operation">A function representing the asynchronous operation to attempt.</param>
        /// <returns>A task that returns true if the operation succeeds; otherwise, false.</returns>
        public async Task<bool> RetrySystemOperation(string fileName, Func<Task<bool>> operation)
        {
            const int MAX_ATTEMPTS = 2;

            for (int attempt = 0; attempt <= MAX_ATTEMPTS; attempt++)
            {
                try
                {
                    if (await operation())
                        return true;

                    if (attempt < MAX_ATTEMPTS)
                    {
                        var message = $"System resource issue for {fileName}. Retry attempt {attempt + 1} of {MAX_ATTEMPTS}";
                        _output.ShowMessage(message);
                        // Await the asynchronous logging call so that execution waits for it to complete.
                        await _logger.LogProcessingStatus(fileName, BatchProcessor.Core.Models.BatchStatus.Running, message, null, null, null);
                        await Task.Delay(BASE_DELAY_MS * (attempt + 1));
                    }
                }
                catch (Exception ex)
                {
                    if (attempt == MAX_ATTEMPTS)
                    {
                        var message = $"System resource operation failed for {fileName} after {MAX_ATTEMPTS} retries: {ex.Message}";
                        _output.ShowError(message);
                        await _logger.LogProcessingStatus(fileName, BatchProcessor.Core.Models.BatchStatus.Fail, message, null, null, null);
                        return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks for a completion file asynchronously with system metric and timeout considerations.
        /// </summary>
        /// <param name="fileName">The name of the file being processed.</param>
        /// <param name="completionPath">The full path to the expected completion file.</param>
        /// <param name="timeout">The timeout duration for the operation.</param>
        /// <param name="startTime">The start time of the operation.</param>
        /// <returns>
        /// A task that returns a tuple (bool found, string details) where <c>found</c> indicates whether the completion file was found.
        /// </returns>
        public async Task<(bool found, string details)> CheckCompletionWithMetrics(
            string fileName,
            string completionPath,
            TimeSpan timeout,
            DateTime startTime)
        {
            var metrics = _monitor.CaptureMetrics();
            var elapsed = DateTime.Now - startTime;

            if (metrics.CpuPercentage < 10 && metrics.MemoryUsageMB < 100)
            {
                if (!File.Exists(completionPath))
                {
                    if (elapsed < timeout)
                    {
                        await Task.Delay(BASE_DELAY_MS);
                        if (File.Exists(completionPath))
                            return (true, "Found after metrics trigger retry");
                    }
                    return (false, "No completion file after metrics trigger and retry");
                }
                return (true, "Found after metrics trigger");
            }

            if (elapsed > timeout)
            {
                await Task.Delay(BASE_DELAY_MS);
                if (!File.Exists(completionPath))
                    return (false, "Timeout expired, no completion file after retry");
            }

            return (File.Exists(completionPath), "Standard completion check");
        }

        /// <summary>
        /// Handles locked files by retrying once asynchronously.
        /// </summary>
        /// <param name="fileName">The name of the locked file.</param>
        /// <param name="filePath">The full file path to the locked file.</param>
        /// <param name="isDependency">Indicates if the file is a dependency file.</param>
        /// <returns>
        /// A task that returns a tuple (bool success, string details) indicating whether the file is accessible.
        /// </returns>
        public async Task<(bool success, string details)> HandleLockedFile(string fileName, string filePath, bool isDependency = false)
        {
            try
            {
                using (File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    return (true, "File accessible");
                }
            }
            catch (IOException)
            {
                await Task.Delay(BASE_DELAY_MS);
                try
                {
                    using (File.Open(filePath, FileMode.Open, FileAccess.Read))
                    {
                        return (true, "File accessible after retry");
                    }
                }
                catch (IOException)
                {
                    var message = isDependency ?
                        $"Dependency file locked: {fileName}" :
                        $"MISSING: File locked: {fileName}";
                    _output.ShowMessage(message);
                    return (false, message);
                }
            }
        }

        /// <summary>
        /// Retries reading a completion file up to 3 times asynchronously.
        /// </summary>
        /// <param name="fileName">The name of the file whose completion file is to be read.</param>
        /// <param name="completionPath">The full path to the completion file.</param>
        /// <returns>
        /// A task that returns a tuple (bool success, string content) where <c>success</c> indicates whether the file was read successfully.
        /// </returns>
        public async Task<(bool success, string content)> ReadCompletionFile(string fileName, string completionPath)
        {
            const int MAX_READ_ATTEMPTS = 3;

            for (int attempt = 0; attempt < MAX_READ_ATTEMPTS; attempt++)
            {
                try
                {
                    var content = await File.ReadAllTextAsync(completionPath);
                    return (true, content);
                }
                catch (Exception ex)
                {
                    if (attempt == MAX_READ_ATTEMPTS - 1)
                    {
                        var message = $"CANNOT READ completion file for {fileName}";
                        _output.ShowError(message);
                        await _logger.LogProcessingStatus(fileName, BatchProcessor.Core.Models.BatchStatus.Fail, $"{message}: {ex.Message}", null, null, null);
                        return (false, message);
                    }
                    await Task.Delay(BASE_DELAY_MS * (attempt + 1));
                }
            }

            return (false, "CANNOT READ completion file after retries");
        }
    }
}
