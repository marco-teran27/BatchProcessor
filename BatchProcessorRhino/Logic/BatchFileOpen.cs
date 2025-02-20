using System;
using System.IO;
using Rhino;
using BatchProcessor.Core.Interfaces.Command;
using BatchProcessor.Core.Config.Models;

namespace BatchProcessorRhino.Logic
{
    /// <summary>
    /// Handles opening Rhino files for batch processing.
    /// </summary>
    public class BatchFileOpen
    {
        private readonly ICommLineOut _output;

        public BatchFileOpen(ICommLineOut output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        /// <summary>
        /// Opens a Rhino file for batch processing.
        /// </summary>
        /// <param name="fileName">Name of the file to open</param>
        /// <param name="directorySettings">Directory settings containing file paths</param>
        /// <returns>Tuple containing (success status, RhinoDoc if successful, error message if failed)</returns>
        public (bool success, RhinoDoc doc, string message) OpenFile(
            string fileName, 
            DirectorySettings directorySettings)
        {
            try
            {
                // Construct full file path
                var fullPath = Path.Combine(directorySettings.FileDir, fileName);

                // Verify file exists
                if (!File.Exists(fullPath))
                {
                    return (false, null, $"File not found: {fullPath}");
                }

                // Attempt to open the file
                var doc = RhinoDoc.Open(fullPath, out bool wasAlreadyOpen);
                
                if (doc == null)
                {
                    return (false, null, $"Failed to open file: {fileName}");
                }

                // Set modified flag to false to prevent save prompts
                doc.Modified = false;

                return (true, doc, $"Successfully opened {fileName}");
            }
            catch (Exception ex)
            {
                _output.ShowError($"Error opening file {fileName}: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
    }
}