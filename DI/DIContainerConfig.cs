// File: DI\DIContainerConfig.cs
using Microsoft.Extensions.DependencyInjection;
using Batch.Core; // For TheOrchestrator
using Commons.Interfaces; // For ITheOrchestrator, IRhinoCommOut
using ConfigJSON; // For ConfigSelector, ConfigParser
using RhinoCore.Services; // For RhinoCommOut

namespace DI
{
    public static class DIContainerConfig
    {
        public static IServiceProvider ConfigureServices(bool useRhino = true)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ConfigSelector>();
            services.AddSingleton<ConfigParser>();
            services.AddSingleton<ITheOrchestrator, TheOrchestrator>();
            if (useRhino)
                services.AddSingleton<IRhinoCommOut, RhinoCommOut>(); // Use RhinoCommOut when true
            else
                services.AddSingleton<IRhinoCommOut, NoOpRhinoCommOut>(); // Fallback
            return services.BuildServiceProvider();
        }
    }

    internal class NoOpRhinoCommOut : IRhinoCommOut
    {
        public bool RunScript(string script, bool echo) => true;
        public void ShowMessage(string message) { }
        public void ShowError(string message) { }
    }
}