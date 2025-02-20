using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BatchProcessor.Core.IO.Batch
{
    /// <summary>
    /// BatchDirScanner is responsible for scanning a given directory for Rhino (.3dm) files
    /// and filtering them based on the aggregated name criteria provided by BatchRhinoNameList.
    /// 
    /// Responsibilities:
    ///  - Read all .3dm files from the specified directory.
    ///  - If the aggregated criteria (from BatchRhinoNameList) is null (i.e. mode "all"),
    ///    then return every file.
    ///  - Otherwise, filter the list so that only files whose name (without extension) contains
    ///    one or more of the expected criteria are returned.
    /// </summary>
    public class BatchDirScanner
    {
        /// <summary>
        /// Scans the specified directory for Rhino (.3dm) files and returns those that match the
        /// aggregated naming criteria.
        /// </summary>
        /// <param name="directoryPath">
        /// The full path to the directory in which to scan for Rhino files.
        /// </param>
        /// <param name="aggregatedCriteria">
        /// The list of expected naming criteria obtained from BatchRhinoNameList.
        /// If null, it indicates that all files should be accepted.
        /// </param>
        /// <returns>
        /// A list of file names (with extension) that match the expected criteria.
        /// </returns>
        public List<string> ScanDirectory(string directoryPath, List<string>? aggregatedCriteria)
        {
            var matchingFiles = new List<string>();

            // Ensure the directory exists before scanning.
            if (!Directory.Exists(directoryPath))
            {
                // Optionally, the caller is responsible for logging this error.
                return matchingFiles;
            }

            // Retrieve all .3dm files in the specified directory.
            // Directory.GetFiles returns a non-null array of non-null strings,
            // but Path.GetFileName is annotated as returning a nullable string;
            // therefore, we use the null-forgiving operator (!) to indicate non-null.
            var allFiles = Directory.GetFiles(directoryPath, "*.3dm", SearchOption.TopDirectoryOnly)
                                    .Select(x => Path.GetFileName(x)!)
                                    .ToList();

            // If aggregatedCriteria is null, then "all" mode is active; return all found files.
            if (aggregatedCriteria == null)
            {
                matchingFiles.AddRange(allFiles);
            }
            else
            {
                // Filter files: include only those whose name (without extension) contains any expected criteria.
                foreach (var file in allFiles)
                {
                    // Even though file is expected non-null, we check for safety.
                    if (file == null)
                        continue;

                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file) ?? string.Empty;
                    if (aggregatedCriteria.Any(criteria => fileNameWithoutExt.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        matchingFiles.Add(file);
                    }
                }
            }

            return matchingFiles;
        }
    }
}
