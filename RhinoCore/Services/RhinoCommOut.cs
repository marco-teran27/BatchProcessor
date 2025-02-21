// File: RhinoCore\Services\RhinoCommOut.cs
using Rhino;
using Commons.Interfaces; // Updated from BatchProcessor.DI.Interfaces

namespace RhinoCore.Services
{
    /// <summary>
    /// Implements Rhino command-line output interactions.
    /// </summary>
    public class RhinoCommOut : IRhinoCommOut
    {
        public bool RunScript(string script, bool echo)
        {
            return RhinoApp.RunScript(script, echo);
        }

        public void ShowMessage(string message)
        {
            RhinoApp.WriteLine(message);
        }

        public void ShowError(string message)
        {
            RhinoApp.WriteLine($"Error: {message}");
        }
    }
}