using System;
using System.Runtime.InteropServices;

namespace BatchProcessorRhino.Utils
{
    public class RhinoEscapeKeyHelper
    {
        private bool _escapePressed;

        public RhinoEscapeKeyHelper()
        {
            _escapePressed = false;
        }

        public void Start()
        {
            _escapePressed = false;
            Console.WriteLine("Escape key monitoring started.");
        }

        public void Stop()
        {
            Console.WriteLine("Escape key monitoring stopped.");
        }

        public bool WasEscapePressed()
        {
            return _escapePressed;
        }
    }
}