// File: RhinoCore.Tests\ConfigPipelineIntegrationTests.cs
using NUnit.Framework;
using Rhino.Testing.Fixtures; // For RhinoTestFixture
using Microsoft.Extensions.DependencyInjection; // For GetRequiredService<T>
using Commons.Interfaces; // For ITheOrchestrator
using RhinoCore.Plugin;

namespace RhinoCore.Tests
{
    [TestFixture]
    public class ConfigPipelineIntegrationTests : RhinoTestFixture // Inherit from RhinoTestFixture
    {
        public ConfigPipelineIntegrationTests()
        {
            // RhinoTestFixture constructor initializes the Rhino runtime
        }

        [Test]
        public void Command_InvokesOrchestrator()
        {
            // Rhino runtime is already initialized by RhinoTestFixture
            var orchestrator = BatchProcessorPlugin.ServiceProvider.GetRequiredService<ITheOrchestrator>();
            Assert.That(orchestrator, Is.Not.Null, "Orchestrator should be resolved.");

            // Simulate command (manual test in Rhino for now)
            // Future: Use Rhino.Testing to simulate command input
        }
    }
}