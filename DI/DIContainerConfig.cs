// File: DI\DIContainerConfig.cs
using Microsoft.Extensions.DependencyInjection;
using Batch.Core; // For TheOrchestrator
using Commons.Interfaces; // For ITheOrchestrator, IRhinoCommOut
using ConfigJSON; // For ConfigSelector, ConfigParser

namespace DI
{
    public static class DIContainerConfig
    {
        public static IServiceProvider ConfigureServices(bool useRhino = true)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ConfigSelector>();
            services.AddSingleton<ConfigParser>(); // Added back
            services.AddSingleton<ITheOrchestrator, TheOrchestrator>();
            services.AddSingleton<IRhinoCommOut, NoOpRhinoCommOut>();
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