// File: RhinoCode.Tests\ConfigPipelineIntegrationTests.cs
using NUnit.Framework;
using Rhino.Testing.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Commons;
using RhinoCode.Plugin;
using System;
using Rhino;
using Commons.Interfaces;

namespace RhinoCode.Tests
{
    [RhinoTestFixture]
    public class ConfigPipelineIntegrationTests
    {
        [Test]
        public void Command_InvokesOrchestrator()
        {
            RhinoApp.WriteLine("Test started: Command_InvokesOrchestrator");
            Console.WriteLine("Test started: Command_InvokesOrchestrator");
            try
            {
                var orchestrator = BatchProcessorPlugin.ServiceProvider.GetRequiredService<ITheOrchestrator>();
                RhinoApp.WriteLine("Orchestrator resolved: " + (orchestrator != null));
                Console.WriteLine("Orchestrator resolved: " + (orchestrator != null));
                Assert.That(orchestrator, Is.Not.Null, "Orchestrator should be resolved.");
                RhinoApp.WriteLine("Test passed");
                Console.WriteLine("Test passed");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Test failed: {ex.Message}");
                Console.WriteLine($"Test failed: {ex.Message}");
                throw;
            }
        }
    }
}