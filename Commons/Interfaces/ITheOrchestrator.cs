using System.Threading;
using System.Threading.Tasks;

namespace Commons.Interfaces
{
    public interface ITheOrchestrator
    {
        Task<bool> RunBatchAsync(string? configPath, CancellationToken ct); // Restored async
    }
}