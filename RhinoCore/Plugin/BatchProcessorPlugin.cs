using Rhino.PlugIns;
using Microsoft.Extensions.DependencyInjection;
using BatchProcessor.DI;
using System;

namespace RhinoCore.Plugin
{
    /// <summary>
    /// Rhino plugin for batch processing.
    /// </summary>
    public class BatchProcessorPlugin : PlugIn
    {
        /// <summary>
        /// Gets the DI service provider.
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        /// <summary>
        /// Initializes the plugin and configures DI.
        /// </summary>
        public BatchProcessorPlugin()
        {
            Instance = this;
            ServiceProvider = DIContainerConfig.ConfigureServices();
        }

        /// <summary>
        /// Gets the singleton instance of the plugin.
        /// </summary>
        public static BatchProcessorPlugin Instance { get; private set; } = null!;
    }
}