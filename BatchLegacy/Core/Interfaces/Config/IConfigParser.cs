using System.Collections.Generic;
using BatchProcessor.Core.Config.Models;

namespace BatchProcessor.DI.Interfaces.Config
{
    /// <summary>
    /// Defines contract for parsing the config file in one pass,
    /// returning a tuple of (config_Structure?, List of parse errors).
    /// </summary>
    public interface IConfigParser
    {
        /// <summary>
        /// Reads the given JSON file path and returns:
        ///   1) A config_Structure instance if successful (or null if it fails),
        ///   2) A list of parse error messages, if any.
        /// </summary>
        /// <param name="filePath">Full path to the config file</param>
        /// <returns>(config_Structure? config, List of error strings)</returns>
        (config_Structure? configRoot, List<string> parseErrors) ParseConfigFile(string filePath);
    }
}
