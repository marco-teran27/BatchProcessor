using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using Rhino;
using Rhino.Commands;
using RhinoCore.Plugin;
using Commons.Interfaces;

namespace RhinoCore.CommandLine
{
    public class CommLineCommand : Command
    {
        public override string EnglishName => "BatchProcessor";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                var orchestrator = BatchProcessorPlugin.ServiceProvider.GetService<ITheOrchestrator>()
                    ?? throw new InvalidOperationException("Failed to resolve ITheOrchestrator.");
                using var cts = new CancellationTokenSource();
                bool success = orchestrator.RunBatchAsync(null, cts.Token).GetAwaiter().GetResult(); // Sync call for now
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