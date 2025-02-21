using System.Threading;
using System.Threading.Tasks;

namespace Commons.Interfaces
{
    /// <summary>
    /// Defines the contract for orchestrating batch processing.
    /// </summary>
    public interface ITheOrchestrator
    {
        /// <summary>
        /// Runs the batch process asynchronously.
        /// </summary>
        /// <param name="configPath">Optional config file path; if null, prompts user.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>True if successful; false otherwise.</returns>
        Task<bool> RunBatchAsync(string? configPath, CancellationToken ct);
    }
}