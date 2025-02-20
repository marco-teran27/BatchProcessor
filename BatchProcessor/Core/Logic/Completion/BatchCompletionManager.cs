using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BatchProcessor.DI.Interfaces.Batch;           // If IBatchCompletionManager is defined here
using BatchProcessor.DI.Interfaces.AbsRhino;    // For ICommLineOut
using BatchProcessor.Core.Logic.Timeout;         // For TimeoutManager

namespace BatchProcessor.Core.Logic.Completion
{
    /// <summary>
    /// Manages completion detection and timeout handling for batch processes.
    /// </summary>
    public class BatchCompletionManager : IBatchCompletionManager
    {
        private readonly string _completionDir;
        private readonly string _projectName;
        private readonly ICommLineOut _output;
        private readonly TimeoutManager _timeoutManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchCompletionManager"/> class.
        /// </summary>
        /// <param name="outputDir">Base directory for completion files.</param>
        /// <param name="projectName">Project name for completion file naming.</param>
        /// <param name="timeoutMinutes">Initial timeout duration in minutes.</param>
        /// <param name="output">Command line output interface.</param>
        public BatchCompletionManager(
            string outputDir,
            string projectName,
            int timeoutMinutes,
            ICommLineOut output)
        {
            _completionDir = Path.Combine(outputDir, "completion");
            _projectName = projectName;
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _timeoutManager = new TimeoutManager(timeoutMinutes);

            Directory.CreateDirectory(_completionDir);
        }

        /// <summary>
        /// Waits for completion of a file processing operation.
        /// </summary>
        /// <param name="filePath">Path to the file being processed.</param>
        /// <param name="scriptPath">Path to the script processing the file.</param>
        /// <returns>
        /// A tuple indicating whether the completion was successful and details about the completion.
        /// </returns>
        public async Task<(bool success, string details)> WaitForCompletion(string filePath, string scriptPath)
        {
            var startTime = DateTime.Now;
            var timeout = _timeoutManager.CalculateTimeout(filePath);
            var fileName = Path.GetFileName(filePath);

            while (DateTime.Now - startTime < timeout)
            {
                var result = await CheckForCompletion(fileName);
                if (result.HasValue)
                {
                    if (result.Value.success)
                    {
                        _timeoutManager.UpdateHistory(filePath, DateTime.Now - startTime);
                    }
                    return result.Value;
                }

                await Task.Delay(100); // Short delay between checks
            }

            return (false, $"Operation timed out after {timeout.TotalMinutes:F1} minutes");
        }

        /// <summary>
        /// Checks for the existence and content of completion files.
        /// </summary>
        /// <param name="fileName">Name of the file to check completion for.</param>
        /// <returns>
        /// A tuple with completion status and details if a completion file is found; otherwise, null.
        /// </returns>
        private async Task<(bool success, string details)?> CheckForCompletion(string fileName)
        {
            var passFile = GetCompletionPath(fileName, "PASS");
            var failFile = GetCompletionPath(fileName, "FAIL");

            try
            {
                if (File.Exists(passFile))
                {
                    var result = await ReadCompletionFile(passFile);
                    File.Delete(passFile);
                    return result;
                }

                if (File.Exists(failFile))
                {
                    var result = await ReadCompletionFile(failFile);
                    File.Delete(failFile);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _output.ShowError($"Error checking completion: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Reads and parses a completion file.
        /// </summary>
        /// <param name="path">Path to the completion file.</param>
        /// <returns>
        /// A tuple with completion status and details as parsed from the file.
        /// </returns>
        private async Task<(bool success, string details)> ReadCompletionFile(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                var completion = await JsonSerializer.DeserializeAsync<CompletionInfo>(stream);
                return completion != null
                    ? (completion.Success, completion.Details ?? string.Empty)
                    : (false, "Invalid completion file format");
            }
            catch (Exception ex)
            {
                return (false, $"Error reading completion file: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates the full path for a completion file.
        /// </summary>
        /// <param name="fileName">Base file name.</param>
        /// <param name="status">Completion status (PASS/FAIL).</param>
        /// <returns>Full path to the completion file.</returns>
        private string GetCompletionPath(string fileName, string status)
        {
            return Path.Combine(
                _completionDir,
                $"{fileName}_{_projectName}_{status}.json"
            );
        }

        /// <summary>
        /// Checks whether the given file has a valid completion file.
        /// 
        /// This method searches for a PASS or FAIL completion file in the completion directory.
        /// </summary>
        /// <param name="fileName">The name of the file to check.</param>
        /// <returns>True if a valid completion file exists; otherwise, false.</returns>
        public bool HasValidCompletionFiles(string fileName)
        {
            var passFile = GetCompletionPath(fileName, "PASS");
            var failFile = GetCompletionPath(fileName, "FAIL");
            return File.Exists(passFile) || File.Exists(failFile);
        }

        /// <summary>
        /// Represents the model for completion file content.
        /// </summary>
        private class CompletionInfo
        {
            public bool Success { get; set; }
            public string? Details { get; set; }
        }
    }
}
