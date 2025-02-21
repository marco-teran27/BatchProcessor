// File: ConfigJSON\ConfigParser.cs
using System;
using System.IO;
using System.Text.Json;
using ConfigJSON.Models;

namespace BatchProcessor.ConfigJSON
{
    /// <summary>
    /// Parses a JSON configuration file into a ConfigStructure object.
    /// </summary>
    public class ConfigParser
    {
        /// <summary>
        /// Parses the JSON configuration file at the specified path.
        /// </summary>
        /// <param name="filePath">Path to the JSON file.</param>
        /// <returns>The parsed ConfigStructure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if filePath is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
        /// <exception cref="JsonException">Thrown if JSON parsing fails.</exception>
        public ConfigStructure ParseConfig(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath), "Configuration file path cannot be null or empty.");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Configuration file not found: {filePath}", filePath);

            string json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var config = JsonSerializer.Deserialize<ConfigStructure>(json, options)
                ?? throw new JsonException($"Failed to parse configuration from {filePath}.");

            config.FilePath = filePath; // Store for later use
            return config;
        }
    }
}