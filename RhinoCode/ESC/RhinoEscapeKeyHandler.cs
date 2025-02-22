using System;
using RhinoCode.Utils;

namespace RhinoCode.ESC
{
    /// <summary>
    /// Handles the processing of the Escape key event in Rhino.
    /// This legacy module now delegates key-handling operations to a helper class.
    /// Implements IDisposable to ensure proper cleanup of any resources.
    /// </summary>
    public class RhinoEscapeKeyHandler : IDisposable
    {
        // Private helper instance to provide key handling functionality.
        private readonly RhinoEscapeKeyHelper _keyHelper;

        // Flag to detect redundant dispose calls.
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RhinoEscapeKeyHandler"/> class.
        /// </summary>
        public RhinoEscapeKeyHandler()
        {
            _keyHelper = new RhinoEscapeKeyHelper();
        }

        /// <summary>
        /// Starts the escape key monitoring.
        /// Delegates the start operation to the helper.
        /// </summary>
        public void Start()
        {
            _keyHelper.Start();
        }

        /// <summary>
        /// Stops the escape key monitoring.
        /// Delegates the stop operation to the helper.
        /// </summary>
        public void Stop()
        {
            _keyHelper.Stop();
        }

        /// <summary>
        /// Gets a value indicating whether the escape key was pressed.
        /// Delegates the check to the helper.
        /// </summary>
        /// <returns><c>true</c> if the escape key was pressed; otherwise, <c>false</c>.</returns>
        public bool WasEscapePressed()
        {
            return _keyHelper.WasEscapePressed();
        }

        /// <summary>
        /// Disposes managed and unmanaged resources used by the RhinoEscapeKeyHandler.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs the actual disposal of resources.
        /// </summary>
        /// <param name="disposing">If true, managed resources are disposed; otherwise, only unmanaged resources are released.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Call Stop to ensure any active hooks or timers are stopped.
                    _keyHelper.Stop();

                    // If _keyHelper itself implemented IDisposable, you would call:
                    // _keyHelper.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer for RhinoEscapeKeyHandler.
        /// </summary>
        ~RhinoEscapeKeyHandler()
        {
            Dispose(false);
        }
    }
}
