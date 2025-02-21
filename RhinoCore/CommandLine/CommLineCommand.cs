using Rhino.Commands;
using System;
using System.Threading;
using BatchProcessor.DI.Interfaces;
using BatchProcessor.RhinoCore.Plugin;
using Rhino;

namespace BatchProcessor.RhinoCore.CommandLine
{
    /// <summary>
    /// Rhino command to initiate batch processing via "BatchProcessor".
    /// </summary>
    public class CommLineCommand : Command
    {
        /// <summary>
        /// Gets the command name as it appears in Rhino.
        /// </summary>
        public override string EnglishName => "BatchProcessor";

        /// <summary>
        /// Executes the batch processing command.
        /// </summary>
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                var orchestrator = BatchProcessorPlugin.ServiceProvider.GetService<ITheOrchestrator>()
                    ?? throw new InvalidOperationException("Failed to resolve IBatchOrchestrator.");
                using var cts = new CancellationTokenSource();
                bool success = orchestrator.RunBatchAsync(null, cts.Token).GetAwaiter().GetResult();
                return success ? Result.Success : Result.Failure;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"BatchProcessor failed: {ex.Message}");
                return Result.Failure;
            }
        }
    }
}