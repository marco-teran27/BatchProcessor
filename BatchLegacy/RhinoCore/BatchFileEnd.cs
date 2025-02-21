using System;
using Rhino;
using BatchProcessor.Core.Interfaces.Command;

namespace BatchProcessorRhino.Logic
{
    /// <summary>
    /// Handles cleanup and closing of Rhino files after batch processing.
    /// </summary>
    public class BatchFileEnd
    {
        private readonly ICommLineOut _output;

        public BatchFileEnd(ICommLineOut output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        /// <summary>
        /// Closes a Rhino document without saving changes.
        /// </summary>
        /// <param name="doc">The RhinoDoc to close</param>
        /// <param name="fileName">Name of the file (for logging)</param>
        /// <returns>True if closed successfully</returns>
        public bool CloseFile(RhinoDoc doc, string fileName)
        {
            try
            {
                if (doc == null) return true; // Nothing to close

                // Ensure modified flag is false to prevent save prompt
                doc.Modified = false;

                // Close the document
                doc.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                _output.ShowError($"Error closing file {fileName}: {ex.Message}");
                return false;
            }
        }
    }
}