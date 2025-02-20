using System;
using System.IO;
using BatchProcessor.Core.Config.Models;
using BatchProcessor.Core.Interfaces.Script;
using BatchProcessor.Core.Interfaces.AbsRhino;

namespace BatchProcessor.Core.IO.Script
{
    /// <summary>
    /// Handles script file location and validation.
    /// </summary>
    public class ScriptScanner : IScriptScanner
    {
        private readonly ICommLineOut _output;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptScanner"/> class.
        /// </summary>
        /// <param name="output">The command-line output interface used for reporting errors and messages.</param>
        public ScriptScanner(ICommLineOut output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        /// <summary>
        /// Finds a script in the specified directory.
        /// </summary>
        /// <param name="directory">Directory to search.</param>
        /// <param name="scriptName">Name of the script to find.</param>
        /// <param name="type">Type of script to look for.</param>
        /// <returns>Full path to the script if found; otherwise, null.</returns>
        public string? FindScript(string directory, string scriptName, ScriptType type)
        {
            var extension = GetScriptExtension(type);
            var fullPath = Path.Combine(directory, $"{scriptName}{extension}");

            if (!File.Exists(fullPath))
            {
                _output.ShowError($"Script not found: {fullPath}");
                return null;
            }

            return fullPath;
        }

        /// <summary>
        /// Verifies if a script exists and is valid.
        /// </summary>
        /// <param name="scriptPath">Full path to the script.</param>
        /// <param name="type">Expected script type.</param>
        /// <returns>True if the script exists and its extension matches the expected type; otherwise, false.</returns>
        public bool ValidateScript(string scriptPath, ScriptType type)
        {
            if (string.IsNullOrWhiteSpace(scriptPath))
            {
                _output.ShowError("Script path is null or empty.");
                return false;
            }

            if (!File.Exists(scriptPath))
            {
                _output.ShowError($"Script file does not exist: {scriptPath}");
                return false;
            }

            string expectedExtension = GetScriptExtension(type);
            string actualExtension = Path.GetExtension(scriptPath);
            if (!string.Equals(actualExtension, expectedExtension, StringComparison.OrdinalIgnoreCase))
            {
                _output.ShowError($"Script file {scriptPath} does not match expected extension {expectedExtension} for script type {type}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the expected file extension for a given script type.
        /// </summary>
        /// <param name="type">The script type.</param>
        /// <returns>The expected file extension (including the period).</returns>
        private string GetScriptExtension(ScriptType type) => type switch
        {
            ScriptType.Python => ".py",
            ScriptType.Grasshopper => ".gh",
            ScriptType.GrasshopperXml => ".ghx",
            _ => throw new ArgumentException($"Unsupported script type: {type}")
        };
    }
}
