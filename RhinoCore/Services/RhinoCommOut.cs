// File: BatchProcessorRhino\Services\RhinoAppWrapper.cs
using Rhino;
using DI.Interfaces;

namespace RhinoCore.Services
{
    /// <summary>
    /// Implements Rhino interactions for batch processing.
    /// </summary>
    public class RhinoCommOut : IRhinoCommOut
    {
        /// <summary>
        /// Runs a script in Rhino (placeholder for later).
        /// </summary>
        public bool RunScript(string script, bool echo)
        {
            return RhinoApp.RunScript(script, echo);
        }

        /// <summary>
        /// Displays a message in Rhino’s command line.
        /// </summary>
        public void ShowMessage(string message)
        {
            RhinoApp.WriteLine(message);
        }

        /// <summary>
        /// Displays an error in Rhino’s command line.
        /// </summary>
        public void ShowError(string message)
        {
            RhinoApp.WriteLine($"Error: {message}");
        }
    }
}