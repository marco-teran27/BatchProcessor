using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BatchProcessor.Core.Config.Models;
using BatchProcessor.Core.Logic.Utils;

namespace BatchProcessor.Core.Logic.Services
{
    /// <summary>
    /// BatchRhinoNameList consolidates configuration parameters from PidSettings and RhinoFileNameSettings
    /// to produce an aggregated list of expected Rhino file naming criteria. It also provides methods for:
    /// 
    ///  1) Aggregation of Configuration Criteria:
    ///     - For <c>PidSettings</c>:
    ///         • Mode "list": Uses the provided list of PIDs.
    ///         • Mode "all": All Rhino files are acceptable (PID filtering is not applied).
    ///     - For <c>RhinoFileNameSettings</c>:
    ///         • Mode "list": Uses the provided list of keywords.
    ///         • Mode "all": All Rhino files are acceptable (keyword filtering is not applied).
    /// 
    ///     The aggregator follows these rules:
    ///       • If both settings are in "all" mode, the aggregated criteria is <c>null</c>,
    ///         meaning every Rhino file is acceptable.
    ///       • If one setting is in "all" mode and the other is in "list" mode, only the non-"all" setting's list is used.
    ///       • If both settings are in "list" mode, the module combines each PID with each keyword (for example, "300000L-foobar")
    ///         to form the expected criteria.
    /// 
    ///  2) Parsing a Rhino File Name:
    ///     Uses the centralized regex (from <c>RhinoNameRegex</c>) to extract file name components
    ///     (BasePid, OptionalDigits, Keyword, and SrNumber) from a given Rhino file name.
    /// 
    ///  3) Matching a File Name:
    ///     Determines if a given file name (without its extension) matches one or more of the expected criteria.
    ///     In "all" mode (aggregated criteria is null), every file is considered a match.
    /// 
    /// This module replaces the former responsibilities of BatchNameParser (for extraction)
    /// and parts of BatchNameValidator (for applying configuration rules) into a single consolidated service.
    /// </summary>
    public class BatchRhinoNameList
    {
        private readonly PIDSettings _pidSettings;
        private readonly RhinoFileNameSettings _rhinoFileNameSettings;

        /// <summary>
        /// Initializes a new instance of BatchRhinoNameList with the specified configuration settings.
        /// </summary>
        /// <param name="pidSettings">The PidSettings containing the mode and list of PIDs.</param>
        /// <param name="rhinoFileNameSettings">The RhinoFileNameSettings containing the mode and keywords.</param>
        public BatchRhinoNameList(PIDSettings pidSettings, RhinoFileNameSettings rhinoFileNameSettings)
        {
            _pidSettings = pidSettings ?? throw new ArgumentNullException(nameof(pidSettings));
            _rhinoFileNameSettings = rhinoFileNameSettings ?? throw new ArgumentNullException(nameof(rhinoFileNameSettings));
        }

        /// <summary>
        /// Retrieves the consolidated list of expected name criteria based on the configuration.
        /// 
        /// The rules are as follows:
        ///   - If both PidSettings and RhinoFileNameSettings are in "all" mode,
        ///     then the method returns <c>null</c> (meaning every file is acceptable).
        ///   - If one of the settings is in "all" mode and the other is in "list" mode,
        ///     then only the list from the "list" setting is used.
        ///   - If both settings are in "list" mode, then a combined list is produced by
        ///     concatenating each PID with each keyword (formatted as "PID-keyword").
        /// 
        /// If both lists are empty, it will also return <c>null</c>.
        /// </summary>
        /// <returns>
        /// A list of strings representing the expected PID-keyword combinations,
        /// or <c>null</c> if every file should be accepted.
        /// </returns>
        public List<string>? GetAggregatedNameCriteria()
        {
            bool pidAll = _pidSettings.Mode.Equals("all", StringComparison.OrdinalIgnoreCase);
            bool rhinoAll = _rhinoFileNameSettings.Mode.Equals("all", StringComparison.OrdinalIgnoreCase);

            // If both are "all", then every file is acceptable.
            if (pidAll && rhinoAll)
            {
                return null;
            }

            List<string> aggregated = new List<string>();

            // If PidSettings is in "list" mode, use its list; otherwise, treat it as empty.
            List<string> pidCriteria = pidAll ? new List<string>() : _pidSettings.Pids;
            // If RhinoFileNameSettings is in "list" mode, use its keywords; otherwise, treat it as empty.
            List<string> keywordCriteria = rhinoAll ? new List<string>() : _rhinoFileNameSettings.Keywords;

            // If both lists are available, combine them.
            if (pidCriteria.Any() && keywordCriteria.Any())
            {
                foreach (var pid in pidCriteria)
                {
                    foreach (var keyword in keywordCriteria)
                    {
                        aggregated.Add($"{pid}-{keyword}");
                    }
                }
            }
            else if (pidCriteria.Any())
            {
                aggregated.AddRange(pidCriteria);
            }
            else if (keywordCriteria.Any())
            {
                aggregated.AddRange(keywordCriteria);
            }
            else
            {
                // If both lists are empty, then treat it as "all" mode.
                return null;
            }

            return aggregated;
        }

        /// <summary>
        /// Parses a given Rhino file name (expected format: "300000L-foobar-S12345.3dm")
        /// and returns a <c>BatchNameComponents</c> object containing its constituent parts.
        /// </summary>
        /// <param name="fileName">The Rhino file name to parse.</param>
        /// <returns>
        /// A <c>BatchNameComponents</c> object with the extracted components if the file name matches the expected pattern;
        /// otherwise, <c>null</c>.
        /// </returns>
        public BatchNameComponents? ParseFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            // Use the centralized regex from RhinoNameRegex.
            var match = RhinoNameRegex.RhinoFilePattern.Match(fileName);
            if (!match.Success)
                return null;

            return new BatchNameComponents
            {
                BasePid = match.Groups[1].Value,
                OptionalDigits = match.Groups[2].Value,
                Keyword = match.Groups[3].Value,
                SrNumber = match.Groups[4].Value
            };
        }

        /// <summary>
        /// Determines if the given file name matches the aggregated naming criteria.
        /// 
        /// - If the aggregated criteria is <c>null</c>, then every file is considered a match (i.e. "all" mode).
        /// - Otherwise, the file name (without its extension) is checked to see if it contains any one of the expected criteria.
        /// </summary>
        /// <param name="fileName">The Rhino file name to validate.</param>
        /// <returns><c>true</c> if the file name matches one or more expected criteria; otherwise, <c>false</c>.</returns>
        public bool IsFileNameMatch(string fileName)
        {
            var aggregatedCriteria = GetAggregatedNameCriteria();
            if (aggregatedCriteria == null)
                return true; // "all" mode

            var fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(fileName);
            if (fileNameWithoutExt == null)
                return false;

            return aggregatedCriteria.Any(criteria =>
                fileNameWithoutExt.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }

    /// <summary>
    /// Represents the individual components extracted from a Rhino file name.
    /// For example, for "300000L-foobar-S12345.3dm":
    ///   - BasePid: "300000L"
    ///   - OptionalDigits: (empty if none)
    ///   - Keyword: "foobar"
    ///   - SrNumber: "S12345"
    /// </summary>
    public class BatchNameComponents
    {
        public string BasePid { get; set; } = string.Empty;
        public string OptionalDigits { get; set; } = string.Empty;
        public string Keyword { get; set; } = string.Empty;
        public string SrNumber { get; set; } = string.Empty;
    }
}
