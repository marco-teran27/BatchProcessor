// File: RhinoCore.Tests\ConfigPipelineIntegrationTests.cs
using NUnit.Framework;
using Rhino.Runtime.InProcess; // For out-of-process Rhino
using Microsoft.Extensions.DependencyInjection;
using Commons;
using RhinoCore.Plugin;
using Commons.Interfaces;

namespace RhinoCore.Tests
{
    [TestFixture]
    public class ConfigPipelineIntegrationTests
    {
        [SetUp]
        public void Setup()
        {
            RhinoInsideResolver.Initialize(); // Start Rhino out-of-process
        }

        [Test]
        public void Command_InvokesOrchestrator()
        {
            var orchestrator = BatchProcessorPlugin.ServiceProvider.GetRequiredService<ITheOrchestrator>();
            Assert.That(orchestrator, Is.Not.Null, "Orchestrator should be resolved.");
            // Simulate command with Rhino.Inside
        }

        [TearDown]
        public void TearDown()
        {
            RhinoInsideResolver.Shutdown();
        }
    }
}