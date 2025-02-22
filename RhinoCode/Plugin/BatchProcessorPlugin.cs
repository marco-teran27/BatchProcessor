// File: RhinoCore\Plugin\BatchProcessorPlugin.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using Rhino.PlugIns;
using DI; // For DIContainerConfig
using Commons.Interfaces; // For IRhinoCommOut
using RhinoCode.Services; // For RhinoCommOut

namespace RhinoCode.Plugin
{
    public class BatchProcessorPlugin : PlugIn
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        public BatchProcessorPlugin()
        {
            Instance = this;
            ServiceProvider = ConfigureServicesWithRhino();
        }

        public static BatchProcessorPlugin Instance { get; private set; } = null!;

        private static IServiceProvider ConfigureServicesWithRhino()
        {
            var baseServices = DIContainerConfig.ConfigureServices(useRhino: true);
            var services = new ServiceCollection();
            foreach (var descriptor in baseServices.GetServiceDescriptors())
            {
                services.Add(descriptor);
            }
            services.AddSingleton<IRhinoCommOut, RhinoCommOut>(); // Override NoOpRhinoCommOut
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