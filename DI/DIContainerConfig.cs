// File: BatchProcessor.DI\DIContainerConfig.cs
using Microsoft.Extensions.DependencyInjection;
using BatchProcessor.Core;
using BatchProcessor.DI.Interfaces;
using BatchProcessor.RhinoCore.Services;
using BatchProcessor.ConfigJSON;

namespace BatchProcessor.DI
{
    /// <summary>
    /// Configures dependency injection for the batch processor.
    /// </summary>
    public static class DIContainerConfig
    {
        /// <summary>
        /// Configures services for the application.
        /// </summary>
        /// <param name="useRhino">True to use Rhino integration; false for no-op.</param>
        public static IServiceProvider ConfigureServices(bool useRhino = true)
        {
            var services = new ServiceCollection();

            // Config pipeline
            services.AddSingleton<ConfigSelector>();
            services.AddSingleton<ConfigParser>();
            services.AddSingleton<ITheOrchestrator, TheOrchestrator>();

            // Rhino integration
            services.AddSingleton<IRhinoCommOut>(useRhino ? new RhinoCommOut() : new NoOpRhinoCommOut());

            return services.BuildServiceProvider();
        }
    }

    /// <summary>
    /// No-op implementation for non-Rhino environments.
    /// </summary>
    internal class NoOpRhinoCommOut : IRhinoCommOut
    {
        public bool RunScript(string script, bool echo) => true;
        public void ShowMessage(string message) { }
        public void ShowError(string message) { }
    }
}