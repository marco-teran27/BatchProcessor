using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BatchProcessor.Core.Config.Models;
using BatchProcessor.DI.Interfaces.AbsRhino;
using BatchProcessor.DI.Interfaces.Config;

namespace BatchProcessor.Core.IO.Config
{
    /// <summary>
    /// =============================================================================
    /// SUMMARY:
    ///   ConfigParser is responsible for asynchronously reading and deserializing a JSON
    ///   configuration file into a config_Structure object. It reports high-level errors such
    ///   as "file not found", "invalid JSON format", or unexpected exceptions during parsing.
    ///   Basic structural checks (e.g. ensuring sub–objects exist) are not performed here,
    ///   since those checks are handled later in the configuration validation pipeline.
    /// =============================================================================
    /// </summary>
    public class ConfigParser : IConfigParser
    {
        private readonly ICommLineOut _output;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of ConfigParser.
        /// </summary>
        /// <param name="output">
        /// An optional ICommLineOut instance used for reporting high-level errors during parsing.
        /// If none is provided, a no-op implementation is used.
        /// </param>
        public ConfigParser(ICommLineOut? output = null)
        {
            _output = output ?? new NoOpCommLineOut();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        /// <summary>
        /// Asynchronously reads and deserializes the JSON configuration file.
        /// Reports errors if the file is not found, if deserialization fails, or if an exception occurs.
        /// </summary>
        /// <param name="filePath">The full path to the configuration file.</param>
        /// <returns>
        /// A tuple containing the deserialized config_Structure (or null on error) and a list of error messages.
        /// </returns>
        public async Task<(config_Structure?, List<string>)> ParseConfigFileAsync(string filePath)
        {
            var errors = new List<string>();
            config_Structure? configRoot = null;

            try
            {
                // Check if the file exists.
                if (!File.Exists(filePath))
                {
                    var msg = $"Config file not found: {filePath}";
                    _output.ShowError(msg);
                    errors.Add(msg);
                    return (null, errors);
                }

                // Read the entire file asynchronously.
                var jsonString = await File.ReadAllTextAsync(filePath);

                // Deserialize the JSON into a config_Structure object.
                configRoot = JsonSerializer.Deserialize<config_Structure>(jsonString, _jsonOptions);
                if (configRoot == null)
                {
                    var msg = "Deserialization returned null (invalid JSON format?).";
                    _output.ShowError(msg);
                    errors.Add(msg);
                    return (null, errors);
                }

                // Note: Basic structural checks are omitted because detailed validation is handled later.
                return (configRoot, errors);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error parsing config file: {ex.Message}";
                _output.ShowError(errorMsg);
                errors.Add(errorMsg);
                return (null, errors);
            }
        }

        /// <summary>
        /// Synchronously reads and deserializes the JSON configuration file.
        /// This method wraps the asynchronous version and blocks until completion.
        /// </summary>
        /// <param name="filePath">The full path to the configuration file.</param>
        /// <returns>
        /// A tuple containing the deserialized config_Structure (or null on error) and a list of error messages.
        /// </returns>
        public (config_Structure?, List<string>) ParseConfigFile(string filePath)
        {
            return ParseConfigFileAsync(filePath).GetAwaiter().GetResult();
        }

        /// <summary>
        /// A no-op implementation of ICommLineOut used when none is provided.
        /// This implementation does nothing.
        /// </summary>
        private class NoOpCommLineOut : ICommLineOut
        {
            public void ShowDependencyStatus(IEnumerable<(string Item, string Status)> deps) { }
            public void UpdateProgress(int current, int total, string currentFile, TimeSpan eta) { }
            public void ShowError(string message) { }
            public void ShowMessage(string message) { }
            public void Clear() { }
        }
    }
}
