// File: DI\DIContainerConfig.cs
using Microsoft.Extensions.DependencyInjection;
using Batch.Core; // Updated from BatchProcessor.Batch
using ConfigJSON;
using Commons.Interfaces; // Updated from BatchProcessor.DI.Interfaces

namespace DI
{
    /// <summary>
    /// Configures dependency injection for the batch processor.
    /// </summary>
    public static class DIContainerConfig
    {
        /// <summary>
        /// Configures services for the application.
        /// </summary>
        /// <param name="useRhino">True to allow Rhino-specific implementations; false for no-op.</param>
        /// <returns>The configured service provider.</returns>
        public static IServiceProvider ConfigureServices(bool useRhino = true)
        {
            var services = new ServiceCollection();

            // Config pipeline
            services.AddSingleton<ConfigSelector>();
            services.AddSingleton<ITheOrchestrator, TheOrchestrator>();

            // Rhino communication output
            services.AddSingleton<IRhinoCommOut, NoOpRhinoCommOut>();

            return services.BuildServiceProvider();
        }
    }

    /// <summary>
    /// No-op implementation of IRhinoCommOut for non-Rhino environments.
    /// </summary>
    internal class NoOpRhinoCommOut : IRhinoCommOut
    {
        public bool RunScript(string script, bool echo) => true;
        public void ShowMessage(string message) { }
        public void ShowError(string message) { }
    }
}