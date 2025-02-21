using System.Threading;
using System.Threading.Tasks;
using BatchProcessor.Core.Config.Models;
using BatchProcessor.Core.Models;

namespace BatchProcessor.DI.Interfaces.Batch
{
    /// <summary>
    /// Defines the contract for the batch processing service.
    /// 
    /// Updated to accept only DirectorySettings, ScriptSettings, ReprocessSettings, and TimeOutSettings.
    /// This interface no longer accepts PIDSettings or RhinoFileNameSettings.
    /// The batch service is responsible for scanning the input directory,
    /// processing matching Rhino files, and returning the overall batch processing results.
    /// </summary>
    public interface IBatchService
    {
        /// <summary>
        /// Processes Rhino files in a batch operation.
        /// </summary>
        /// <param name="directorySettings">Directory settings specifying file input directory and related paths.</param>
        /// <param name="scriptSettings">Script settings specifying the script to run on each Rhino file.</param>
        /// <param name="reprocessSettings">Reprocess settings specifying rules for reprocessing files.</param>
        /// <param name="timeoutSettings">Timeout settings to govern processing timeouts.</param>
        /// <param name="cancellationToken">Token to observe cancellation requests.</param>
        /// <returns>A BatchResults object encapsulating processing statistics and outcomes.</returns>
        Task<BatchResults> ProcessFilesAsync(
            DirectorySettings directorySettings,
            ScriptSettings scriptSettings,
            ReprocessSettings reprocessSettings,
            TimeOutSettings timeoutSettings,
            CancellationToken cancellationToken);
    }
}
