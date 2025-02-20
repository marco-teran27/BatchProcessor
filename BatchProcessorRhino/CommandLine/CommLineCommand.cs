using Rhino;
using Rhino.Commands;
using System;
using System.Threading;
using BatchProcessor.DI.Interfaces.AbsRhino;
using BatchProcessor.Core.Services;
using BatchProcessorRhino.Plugin;
using Microsoft.Extensions.DependencyInjection;

namespace BatchProcessorRhino.CommandLine
{
    public class CommLineCommand : Command
    {
        public override string EnglishName => "BatchProcessor";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                var orchestrator = BatchProcessorPlugin.ServiceProvider.GetService<TheOrchestrator>();
                if (orchestrator == null)
                {
                    RhinoApp.WriteLine("Failed to initialize batch orchestrator.");
                    return Result.Failure;
                }

                // Run the orchestrator with a cancellation token
                using var cts = new CancellationTokenSource();
                bool success = orchestrator.RunFullBatchProcessAsync(cts.Token).GetAwaiter().GetResult();
                return success ? Result.Success : Result.Cancel;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Command execution failed: {ex.Message}");
                return Result.Failure;
            }
        }
    }
}