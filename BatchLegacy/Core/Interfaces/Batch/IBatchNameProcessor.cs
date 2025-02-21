
using BatchProcessor.Core.Config.Models;

namespace BatchProcessor.DI.Interfaces.Batch
{
    /// <summary>
    /// Provides a contract for determining whether a file should be processed based on configuration settings.
    /// </summary>
    public interface IBatchNameProcessor
{
    bool ShouldProcessFile(
        string fileName, 
        DirectorySettings directorySettings,
        PIDSettings pidSettings,
        RhinoFileNameSettings fileNameSettings);
}
}
