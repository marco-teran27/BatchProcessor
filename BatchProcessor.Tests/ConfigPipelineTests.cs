// File: BatchProcessor.Tests\ConfigPipelineTests.cs
using NUnit.Framework;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Batch.Core; // For TheOrchestrator
using Commons.Interfaces; // For ITheOrchestrator, IRhinoCommOut
using ConfigJSON; // For ConfigSelector, ConfigParser
using ConfigJSON.Models; // For ConfigStructure

namespace BatchProcessor.Tests
{
    [TestFixture]
    public class ConfigPipelineTests
    {
        [Test]
        public async Task Orchestrator_ValidPath_ParsesConfig()
        {
            // Arrange
            var selectorMock = new Mock<ConfigSelector>();
            selectorMock.Setup(s => s.SelectConfigFile()).Returns("test.json");

            var parserMock = new Mock<ConfigParser>();
            var config = new ConfigStructure { ProjectName = "Test" };
            parserMock.Setup(p => p.ParseConfigAsync("test.json")).ReturnsAsync(config);

            var rhinoMock = new Mock<IRhinoCommOut>();
            var orchestrator = new TheOrchestrator(selectorMock.Object, parserMock.Object, rhinoMock.Object);

            // Act
            bool result = await orchestrator.RunBatchAsync(null, CancellationToken.None);

            // Assert
            Assert.That(result, Is.True, "Should succeed with valid config path.");
            rhinoMock.Verify(r => r.ShowMessage(It.Is<string>(m => m.Contains("parsed"))), Times.Once());
        }

        [Test]
        public async Task Orchestrator_Canceled_ReturnsFalse()
        {
            // Arrange
            var selectorMock = new Mock<ConfigSelector>();
            selectorMock.Setup(s => s.SelectConfigFile()).Returns((string?)null);

            var parserMock = new Mock<ConfigParser>(); // Added
            var rhinoMock = new Mock<IRhinoCommOut>();
            var orchestrator = new TheOrchestrator(selectorMock.Object, parserMock.Object, rhinoMock.Object);

            // Act
            bool result = await orchestrator.RunBatchAsync(null, CancellationToken.None);

            // Assert
            Assert.That(result, Is.False, "Should fail if config selection canceled.");
            rhinoMock.Verify(r => r.ShowError(It.IsAny<string>()), Times.Once());
        }
    }
}