// File: RhinoCore.Tests\ConfigPipelineIntegrationTests.cs
using NUnit.Framework;
using RhinoInside; // Correct namespace
using Microsoft.Extensions.DependencyInjection;
using RhinoCode.Plugin;
using Commons.Interfaces;

namespace RhinoCode.Tests
{
    [TestFixture]
    public class ConfigPipelineIntegrationTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Resolver.UseLatest = true; // Target Rhino 8
            Resolver.Initialize(); // Start Rhino out-of-process
        }

        [Test]
        public void Command_InvokesOrchestrator()
        {
            var orchestrator = BatchProcessorPlugin.ServiceProvider.GetRequiredService<ITheOrchestrator>();
            Assert.That(orchestrator, Is.Not.Null, "Orchestrator should be resolved.");
        }
    }
}