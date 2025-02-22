using System.Collections.Generic;

namespace Commons
{
    public class ValidationResults
    {
        public string Message { get; }
        public IList<string> Errors { get; } // Changed to IList<string>

        public ValidationResults(string message, IList<string> errors) // Changed parameter type
        {
            Message = message;
            Errors = errors;
        }

        public ValidationResults(string message, string[] errors)
            : this(message, errors != null ? (IList<string>)new List<string>(errors) : new List<string>()) // Explicit cast and creation
        {
        }
    }
}