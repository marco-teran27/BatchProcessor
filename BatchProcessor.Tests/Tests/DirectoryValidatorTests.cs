using NUnit.Framework;
using BatchProcessor.Core.Config.Validation;
using System.IO;

namespace BatchProcessor.Tests
{
    [TestFixture]
    public class DirectoryValidatorTests
    {
        [Test]
        public void ValidateDirectories_EmptyPath_ReturnsError()
        {
            // Arrange
            var validator = new DirectoryValidator();
            string[] dirs = { string.Empty };

            // Act
            var result = validator.ValidateDirectories(dirs);

            // Assert
            Assert.IsFalse(result.IsValid, "Empty directory should be invalid");
            Assert.IsNotEmpty(result.Errors);
        }

        [Test]
        public void ValidateDirectories_NonExistentDirectory_ReturnsError()
        {
            // Arrange
            var validator = new DirectoryValidator();
            string fakeDir = Path.Combine(Path.GetTempPath(), "NonExistentDir12345");
            string[] dirs = { fakeDir };

            // Act
            var result = validator.ValidateDirectories(dirs);

            // Assert
            Assert.IsFalse(result.IsValid, "Non-existent directory should be invalid");
            StringAssert.Contains("Directory not found", result.Errors[0]);
        }

        [Test]
        public void ValidateDirectories_ExistingDirectory_ReturnsSuccess()
        {
            // Arrange
            var validator = new DirectoryValidator();
            string currentDir = Directory.GetCurrentDirectory();
            string[] dirs = { currentDir };

            // Act
            var result = validator.ValidateDirectories(dirs);

            // Assert
            Assert.IsTrue(result.IsValid, "Existing directory should be valid");
            Assert.IsEmpty(result.Errors);
        }
    }
}
