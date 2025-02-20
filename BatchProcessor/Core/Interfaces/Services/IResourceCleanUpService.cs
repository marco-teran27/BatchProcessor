using System.Threading.Tasks;

namespace BatchProcessor.DI.Interfaces.Services
{
    /// <summary>
    /// Defines the contract for a service that cleans up resources used by the batch processor.
    /// Implementations of this interface are responsible for releasing temporary files,
    /// unmanaged resources, or any other resources that require explicit cleanup.
    /// </summary>
    public interface IResourceCleanUpService
    {
        /// <summary>
        /// Performs synchronous cleanup of resources.
        /// </summary>
        void CleanUp();

        /// <summary>
        /// Performs asynchronous cleanup of resources.
        /// </summary>
        /// <returns>A task that represents the asynchronous cleanup operation.</returns>
        Task CleanUpAsync();
    }
}
