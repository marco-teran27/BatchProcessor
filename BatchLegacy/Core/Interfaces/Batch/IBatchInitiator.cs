using System.Threading.Tasks;

/// <summary>
/// Handles the command line entry point for the batch processor.
/// Single responsibility: Command initialization and handoff to processing pipeline.
/// </summary>
public interface IBatchInitiator
{
    /// <summary>
    /// Initializes the batch processing command.
    /// Returns false if initialization fails or user cancels.
    /// </summary>
    Task<bool> Initialize();

    /// <summary>
    /// Cancels the current batch processing operation if one is running.
    /// </summary>
    void Cancel();
}