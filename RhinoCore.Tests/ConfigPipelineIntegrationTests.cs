using NUnit.Framework;
using Rhino.Testing;
using DI.Interfaces;
using RhinoCore.Plugin;

namespace BatchProcessorRhino.Tests
{
    [TestFixture]
    public class ConfigPipelineIntegrationTests
    {
        [Test]
        public void Command_InvokesOrchestrator()
        {
            using var rhino = RhinoRuntime.Host;

            var orchestrator = BatchProcessorPlugin.ServiceProvider.GetService<ITheOrchestrator>();
            Assert.IsNotNull(orchestrator, "Orchestrator should be resolved.");

            // Simulate command (manual test in Rhino for now)
            // Future: Use Rhino.Testing to simulate command input
        }
    }
}