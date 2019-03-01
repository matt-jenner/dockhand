using System.IO;
using System.Threading.Tasks;
using Dockhand.Exceptions;
using Dockhand.Interfaces;
using Dockhand.Test.Builders;
using FluentAssertions;
using NUnit.Framework;

namespace Dockhand.Test.Client.DockerClient
{
    [TestFixture]
    public class ImageExistsAsyncTests
    {
        private string _workingDirectory;

        [SetUp]
        public void SetUp()
        {
            _workingDirectory = Directory.GetCurrentDirectory();
        }

        [Test]
        [TestCase("testId", "otherId, testId, thingyId")]
        [TestCase("testId", "testId")]
        public async Task CommandIsSuccessfulImageExists(string imageId, string results)
        {
            // Arrange
            var imageIds = results.Split(',');
            var mockCommandFactory = BuildMockCommandFactoryForScenario(true, imageIds);

            var sut = new Dockhand.Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var result = await sut.ImageExistsAsync("testId");

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        [TestCase("testId", "otherId, thingyId")]
        [TestCase("testId", "thingyId")]
        [TestCase("testId", "")]
        public async Task CommandIsSuccessfulImageNotExist(string imageId, string results)
        {
            // Arrange
            var containerIds = results.Split(',');
            var mockCommandFactory = BuildMockCommandFactoryForScenario(true, containerIds);

            var sut = new Dockhand.Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var result = await sut.ImageExistsAsync("testId");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void CommandIsNotSuccessfulExceptionThrown()
        {
            // Arrange
            var mockCommandFactory = BuildMockCommandFactoryForScenario(false, new []{ "command error" });

            var sut = new Dockhand.Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var exception = Assert.CatchAsync(async () => await sut.ImageExistsAsync("testId"));

            // Assert
            exception.Should().BeOfType<DockerCommandException>();
        }

        private IRunCommands BuildMockCommandFactoryForScenario(bool success, string[] commandOutput)
        {
            var commandString = Dockhand.Client.DockerCommands.Image.ListIds;
            var command = success
                ? MotherFor.CommandWrapper.ThatSucceeds().WithOutput(commandOutput).Build()
                : MotherFor.CommandWrapper.ThatFails().WithOutput(commandOutput).Build();

            var mockCommandFactory =
                MotherFor.CommandFactory
                    .ForWorkingDirectory(_workingDirectory)
                    .ForCommandReturn(commandString, command)
                    .Build();

            return mockCommandFactory;
        }
    }
}
