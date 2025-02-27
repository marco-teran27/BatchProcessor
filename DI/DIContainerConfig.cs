// File: DI\DIContainerConfig.cs
using Microsoft.Extensions.DependencyInjection;
using System;
using Batch.Core; // For TheOrchestrator
using Commons.Interfaces; // For ITheOrchestrator, IRhinoCommOut
using ConfigJSON; // For ConfigSelector, ConfigParser

namespace DI
{
    public static class DIContainerConfig
    {
#pragma warning disable IDE0060 // Suppress unused parameter warning
        public static IServiceProvider ConfigureServices(bool useRhino = true)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ConfigSelector>();
            services.AddSingleton<ConfigParser>();
            services.AddSingleton<ITheOrchestrator, TheOrchestrator>();
            services.AddSingleton<IRhinoCommOut, NoOpRhinoCommOut>(); // Default, overridden in RhinoCore
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