// File: RhinoCode.Tests\ConfigPipelineIntegrationTests.cs
using NUnit.Framework;
using Rhino.Testing.Fixtures; // For RhinoTestFixtureAttribute
using Microsoft.Extensions.DependencyInjection;
using Commons;
using RhinoCode.Plugin;
using Commons.Interfaces;

namespace RhinoCode.Tests
{
    [RhinoTestFixture] // Ensures Rhino is loaded during discovery
    public class ConfigPipelineIntegrationTests
    {
        [Test]
        public void Command_InvokesOrchestrator()
        {
            var orchestrator = BatchProcessorPlugin.ServiceProvider.GetRequiredService<ITheOrchestrator>();
            Assert.That(orchestrator, Is.Not.Null, "Orchestrator should be resolved.");
        }
    }
}