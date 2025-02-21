// File: RhinoCore\Plugin\BatchProcessorPlugin.cs
using Rhino.PlugIns;
using Microsoft.Extensions.DependencyInjection;
using DI; // Updated from BatchProcessor.DI
using Commons.Interfaces; // Updated from BatchProcessor.DI.Interfaces
using RhinoCore.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;
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
            ServiceProvider = ConfigureServicesWithRhino();
        }

        /// <summary>
        /// Gets the singleton instance of the plugin.
        /// </summary>
        public static BatchProcessorPlugin Instance { get; private set; } = null!;

        private static IServiceProvider ConfigureServicesWithRhino()
        {
            var baseServices = DIContainerConfig.ConfigureServices(useRhino: true);
            var services = new ServiceCollection();
            foreach (var descriptor in baseServices.GetServiceDescriptors())
            {
                services.Add(descriptor);
            }
            services.AddSingleton<IRhinoCommOut, RhinoCommOut>();
            return services.BuildServiceProvider();
        }
    }

    internal static class ServiceProviderExtensions
    {
        public static IEnumerable<ServiceDescriptor> GetServiceDescriptors(this IServiceProvider provider)
        {
            var collection = new ServiceCollection();
            var servicesField = provider.GetType().GetField("_services", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (servicesField != null)
            {
                var descriptors = servicesField.GetValue(provider) as IEnumerable<ServiceDescriptor>;
                if (descriptors != null)
                {
                    foreach (var descriptor in descriptors)
                    {
                        collection.Add(descriptor);
                    }
                }
            }
            return collection;
        }
    }
}