using NUnit.Framework;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using BatchProcessor.Core;
using DI.Interfaces;
using ConfigJSON;
using ConfigJSON.Models;

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
            parserMock.Setup(p => p.ParseConfig("test.json")).Returns(new ConfigStructure { ProjectName = "Test" });
            var rhinoMock = new Mock<IRhinoIntegration>();
            var orchestrator = new TheOrchestrator(selectorMock.Object, parserMock.Object, rhinoMock.Object);

            // Act
            bool result = await orchestrator.RunBatchAsync(null, CancellationToken.None);

            // Assert
            Assert.IsTrue(result, "Should succeed with valid config path.");
            rhinoMock.Verify(r => r.ShowMessage(It.Is<string>(m => m.Contains("parsed")), Times.Once()));
        }

        [Test]
        public async Task Orchestrator_Canceled_ReturnsFalse()
        {
            // Arrange
            var selectorMock = new Mock<ConfigSelector>();
            selectorMock.Setup(s => s.SelectConfigFile()).Returns((string?)null);
            var parserMock = new Mock<ConfigParser>();
            var rhinoMock = new Mock<IRhinoIntegration>();
            var orchestrator = new TheOrchestrator(selectorMock.Object, parserMock.Object, rhinoMock.Object);

            // Act
            bool result = await orchestrator.RunBatchAsync(null, CancellationToken.None);

            // Assert
            Assert.IsFalse(result, "Should fail if config selection canceled.");
            rhinoMock.Verify(r => r.ShowError(It.IsAny<string>()), Times.Once());
        }
    }
}