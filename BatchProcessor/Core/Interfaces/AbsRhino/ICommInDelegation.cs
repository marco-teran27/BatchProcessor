using System.Threading.Tasks;


namespace BatchProcessor.DI.Interfaces.AbsRhino
{
    /// <summary>
    /// Handles command line input and initialization.
    /// Single responsibility: Command entry point and processing initiation.
    /// </summary>
    public interface ICommInDelegation
    {
        /// <summary>
        /// Initiates the batch processing command.
        /// </summary>
        /// <returns>True if command executed successfully, false if cancelled or failed.</returns>
        Task<bool> InitiateCommand();

        /// <summary>
        /// Handles cancellation of the current command.
        /// </summary>
        void HandleCancellation();

        /// <summary>
        /// Gets whether a command is currently active.
        /// </summary>
        bool IsCommandActive { get; }
    }
}