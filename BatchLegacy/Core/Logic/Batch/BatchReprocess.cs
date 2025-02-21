using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BatchProcessor.Core.Models;
using BatchProcessor.Core.Config.Models; // Contains ReprocessSettings, BatchResults, FileProcessingResult, BatchStatus
using BatchProcessor.DI.Interfaces.AbsRhino; // For ICommLineOut
using BatchProcessor.DI.Interfaces.Logging; // For IBatchLogger

/// <summary>
/// The BatchReprocess module manages reprocessing of files based on previous processing results.
/// It loads a reference log file (which must follow a consistent format for resuming runs) and filters out
/// file names (PIDs) that have already been processed. This enables resume or re‑process functionality.
/// </summary>
namespace BatchProcessor.Core.Logic.Batch
{
    public class BatchReprocess
    {
        private readonly ICommLineOut _output;
        private readonly IBatchLogger _logger;
        private readonly HashSet<string> _processedFiles;

        /// <summary>
        /// Initializes a new instance of BatchReprocess.
        /// </summary>
        /// <param name="output">Interface for sending messages to the Rhino command line.</param>
        /// <param name="logger">Interface for logging batch events.</param>
        public BatchReprocess(ICommLineOut output, IBatchLogger logger)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the list of files to reprocess based on the reprocess settings.
        /// If the mode is "ALL", returns all files. Otherwise, loads the reference log file,
        /// deserializes it into BatchResults, and filters out files that were already processed.
        /// </summary>
        /// <param name="settings">Reprocess settings containing mode and reference log file path.</param>
        /// <param name="allFiles">List of all file names found in the batch directory.</param>
        /// <returns>A task that returns a list of file names to reprocess.</returns>
        public async Task<List<string>> GetFilesToReprocess(
            ReprocessSettings settings,
            List<string> allFiles)
        {
            if (settings.Mode.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                return allFiles;
            }

            if (string.IsNullOrEmpty(settings.ReferenceLog) ||
                !File.Exists(settings.ReferenceLog))
            {
                _output.ShowError("Reference log file not found");
                return new List<string>();
            }

            var previousResults = await LoadPreviousResults(settings.ReferenceLog);

            return settings.Mode.ToUpperInvariant() switch
            {
                "RESUME" => GetResumeFiles(previousResults, allFiles),
                "PASS" => GetPassedFiles(previousResults, allFiles),
                "FAIL" => GetFailedFiles(previousResults, allFiles),
                _ => throw new ArgumentException($"Invalid reprocess mode: {settings.Mode}")
            };
        }

        /// <summary>
        /// Loads previous BatchResults from the specified reference log file.
        /// The log file must be in a consistent format for deserialization.
        /// </summary>
        /// <param name="logPath">Path to the reference log file.</param>
        /// <returns>A task that returns BatchResults.</returns>
        private async Task<BatchResults> LoadPreviousResults(string logPath)
        {
            try
            {
                var jsonString = await File.ReadAllTextAsync(logPath);
                var results = JsonSerializer.Deserialize<BatchResults>(jsonString);
                if (results == null)
                {
                    throw new JsonException("Failed to deserialize log file");
                }
                return results;
            }
            catch (Exception ex)
            {
                _output.ShowError($"Error loading previous results: {ex.Message}");
                return new BatchResults
                {
                    FileResults = new List<FileProcessingResult>(),
                    Status = BatchStatus.Fail,
                    Details = "Failed to load previous results"
                };
            }
        }

        /// <summary>
        /// For RESUME mode: Returns files that were not previously processed.
        /// </summary>
        /// <param name="previousResults">The BatchResults from a previous run.</param>
        /// <param name="allFiles">List of all file names from the batch directory.</param>
        /// <returns>A list of file names to reprocess.</returns>
        private List<string> GetResumeFiles(BatchResults previousResults, List<string> allFiles)
        {
            foreach (var result in previousResults.FileResults)
            {
                if (result.Status == BatchStatus.Pass ||
                    result.Status == BatchStatus.Fail)
                {
                    _processedFiles.Add(result.FileName);
                }
            }

            return allFiles.Where(f => !_processedFiles.Contains(f)).ToList();
        }

        /// <summary>
        /// For PASS mode: Returns only the file names that previously processed with a pass status.
        /// </summary>
        /// <param name="previousResults">The BatchResults from a previous run.</param>
        /// <param name="allFiles">List of all file names from the batch directory.</param>
        /// <returns>A list of file names that previously passed processing.</returns>
        private List<string> GetPassedFiles(BatchResults previousResults, List<string> allFiles)
        {
            return previousResults.FileResults
                .Where(r => r.Status == BatchStatus.Pass)
                .Select(r => r.FileName)
                .Intersect(allFiles)
                .ToList();
        }

        /// <summary>
        /// For FAIL mode: Returns only the file names that previously processed with a fail status.
        /// </summary>
        /// <param name="previousResults">The BatchResults from a previous run.</param>
        /// <param name="allFiles">List of all file names from the batch directory.</param>
        /// <returns>A list of file names that previously failed processing.</returns>
        private List<string> GetFailedFiles(BatchResults previousResults, List<string> allFiles)
        {
            return previousResults.FileResults
                .Where(r => r.Status == BatchStatus.Fail)
                .Select(r => r.FileName)
                .Intersect(allFiles)
                .ToList();
        }

        /// <summary>
        /// Checks whether a specific file was previously processed.
        /// </summary>
        /// <param name="fileName">The file name to check.</param>
        /// <returns>True if the file was previously processed; otherwise, false.</returns>
        public bool WasPreviouslyProcessed(string fileName)
        {
            return _processedFiles.Contains(fileName);
        }
    }
}
