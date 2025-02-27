﻿namespace Commons.Interfaces
{
    /// <summary>
    /// Defines minimal Rhino command-line output interactions.
    /// </summary>
    public interface IRhinoCommOut
    {
        /// <summary>
        /// Shows a message in Rhino’s command line.
        /// </summary>
        void ShowMessage(string message);

        /// <summary>
        /// Shows an error in Rhino’s command line.
        /// </summary>
        void ShowError(string message);

        /// <summary>
        /// Runs a script in Rhino (placeholder for later).
        /// </summary>
        bool RunScript(string script, bool echo);
    }
}