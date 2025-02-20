using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BatchProcessor.DI.Interfaces.AbsRhino;
using BatchProcessor.DI.Interfaces.Batch;
using BatchProcessor.DI.Interfaces.Config;
using BatchProcessor.DI.Interfaces.Logging;
using BatchProcessor.DI.Interfaces.Monitoring;
using BatchProcessor.DI.Interfaces.Services;
using BatchProcessor.DI.Interfaces.Script;
using BatchProcessor.Di.Interfaces.AbsRhino;

namespace BatchProcessor.DI
{
    public static class DIContainerConfig
    {
        public static IServiceProvider ConfigureServices(IServiceCollection services, bool useRhino = true)
        {
            // Add logging
            services.AddLogging(configure => configure.AddConsole());

            // Register no-op implementations as defaults (overridden if useRhino = false)
            services.AddSingleton<IRhinoApp, NoOpRhinoApp>();
            services.AddSingleton<ICommLineOut, NoOpCommLineOut>();
            services.AddSingleton<ICommInDelegation, NoOpCommInDelegation>();

            if (!useRhino)
            {
                // Ensure no-op implementations are used in non-Rhino mode
                services.AddSingleton<IRhinoApp, NoOpRhinoApp>();
                services.AddSingleton<ICommLineOut, NoOpCommLineOut>();
                services.AddSingleton<ICommInDelegation, NoOpCommInDelegation>();
            }

            // Caller (e.g., BatchProcessorPlugin) will register specific implementations
            return services.BuildServiceProvider();
        }
    }

    public class NoOpCommLineOut : ICommLineOut
    {
        public void ShowDependencyStatus(IEnumerable<(string Item, string Status)> dependencies) { }
        public void UpdateProgress(int current, int total, string currentFile, TimeSpan estimatedTimeRemaining) { }
        public void ShowError(string message) { }
        public void ShowMessage(string message) { }
        public void Clear() { }
    }

    public class NoOpCommInDelegation : ICommInDelegation
    {
        public Task<bool> InitiateCommand() => Task.FromResult(true);
        public void HandleCancellation() { }
        public bool IsCommandActive => false;
    }

    public class NoOpRhinoApp : IRhinoApp
    {
        public bool RunScript(string command, bool echo) => false;
    }
}