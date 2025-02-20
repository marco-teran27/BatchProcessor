// NEW FILE: Core/IO/Logging/LogWriteException.cs
using System;

namespace BatchProcessor.Core.IO.Logging
{
    /// <summary>
    /// Exception thrown when log file writing fails.
    /// </summary>
    public class LogWriteException : Exception
    {
        public string LogPath { get; }
        public DateTime AttemptTime { get; }

        public LogWriteException(string message, string logPath) 
            : base(message)
        {
            LogPath = logPath;
            AttemptTime = DateTime.Now;
        }

        public LogWriteException(string message, string logPath, Exception innerException) 
            : base(message, innerException)
        {
            LogPath = logPath;
            AttemptTime = DateTime.Now;
        }
    }
}