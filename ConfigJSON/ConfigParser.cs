// File: ConfigJSON\ConfigParser.cs
using System;
using System.IO;
using System.Threading.Tasks; // Added for async
using System.Text.Json;
using ConfigJSON.Models;

namespace ConfigJSON
{
    public class ConfigParser
    {
        public async Task<ConfigStructure> ParseConfigAsync(string filePath) // Changed to async
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath), "Configuration file path cannot be null or empty.");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Configuration file not found: {filePath}", filePath);

            string json = await File.ReadAllTextAsync(filePath); // Async file read
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var config = JsonSerializer.Deserialize<ConfigStructure>(json, options)
                ?? throw new JsonException($"Failed to parse configuration from {filePath}");

            config.FilePath = filePath;
            return config;
        }
    }
}