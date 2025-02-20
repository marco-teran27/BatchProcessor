//---------------------------------------------------------------------------------------------------------------------
// Summary:
//   This interface provides an abstraction layer around RhinoApp functionality to enable easier testing and mocking. 
//   It allows code to interact with Rhino's scripting engine without directly depending on the static RhinoApp class.
//
// Dependencies:
//   - RhinoApp (part of the RhinoCommon SDK): The actual functionality that executes Rhino commands comes from 
//     the RhinoApp class in RhinoCommon. Implementations of this interface are expected to wrap calls to RhinoApp.
//
// Usage:
//   Implement this interface to run Rhino commands without calling RhinoApp directly, facilitating unit testing 
//   and reducing coupling to Rhino-specific APIs.
//---------------------------------------------------------------------------------------------------------------------

namespace BatchProcessor.Di.Interfaces.AbsRhino
{
    /// <summary>
    /// Interface for wrapping RhinoApp functionality to enable testing.
    /// Implementers of this interface should call RhinoApp methods internally 
    /// to execute commands within a Rhino environment, allowing for testable, 
    /// decoupled code.
    /// </summary>
    public interface IRhinoApp
    {
        /// <summary>
        /// Executes a Rhino script command.
        /// This method is intended to forward the provided command string 
        /// to RhinoApp.RunScript internally, enabling scripted interactions 
        /// with the Rhino document.
        /// </summary>
        /// <param name="command">The script command to execute. For example: "_Line", "_Circle", etc.</param>
        /// <param name="echo">
        /// Whether to echo the command in Rhino's command line. 
        /// If true, the command is displayed in Rhino's interface; if false, it remains silent.
        /// </param>
        /// <returns>
        /// True if the command executed successfully; false if it failed or if RhinoApp was not ready 
        /// to handle the command.
        /// </returns>
        bool RunScript(string command, bool echo);
    }
}
