using System;
using System.Threading.Tasks;
using BatchProcessor.DI.Interfaces.Services;

namespace BatchProcessor.Core.Services
{
    /// <summary>
    /// Provides functionality to clean up resources used during batch processing.
    /// This service is responsible for disposing of temporary files, releasing unmanaged resources,
    /// and performing any necessary cleanup operations to ensure optimal resource usage.
    /// </summary>
    public class ResourceCleanupService : IResourceCleanUpService, IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceCleanupService"/> class.
        /// </summary>
        public ResourceCleanupService()
        {
            // Initialization logic here if required.
        }

        /// <summary>
        /// Performs synchronous cleanup of resources.
        /// </summary>
        public void CleanUp()
        {
            // Insert resource cleanup logic here.
            // For example: delete temporary files, clear caches, etc.
        }

        /// <summary>
        /// Performs asynchronous cleanup of resources.
        /// </summary>
        /// <returns>A task representing the asynchronous cleanup operation.</returns>
        public async Task CleanUpAsync()
        {
            // Optionally perform asynchronous operations here.
            // For now, we simply call the synchronous cleanup method.
            CleanUp();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="ResourceCleanupService"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources and optionally the managed resources.
        /// </summary>
        /// <param name="disposing">If true, releases both managed and unmanaged resources; otherwise, only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources if any.
                }
                // Free unmanaged resources if any.
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer for the <see cref="ResourceCleanupService"/>.
        /// </summary>
        ~ResourceCleanupService()
        {
            Dispose(false);
        }
    }
}
