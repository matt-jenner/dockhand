using System;
using System.IO;
using System.Threading.Tasks;
using Dockhand.Client;
using Dockhand.Exceptions;
using Dockhand.Interfaces;
using Dockhand.Test.Builders;
using FluentAssertions;
using NUnit.Framework;

namespace Dockhand.Test.DockerClient
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

            var sut = new Client.DockerClient(_workingDirectory, mockCommandFactory);

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

            var sut = new Client.DockerClient(_workingDirectory, mockCommandFactory);

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

            var sut = new Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var exception = Assert.CatchAsync(async () => await sut.ImageExistsAsync("testId"));

            // Assert
            exception.Should().BeOfType<DockerCommandException>();
        }

        [Test]
        public void CommandIsNotSuccessfulExceptionContainsOutput()
        {
            // Arrange
            var commandOutput = new[] {"command error","some other message"};
            var mockCommandFactory = BuildMockCommandFactoryForScenario(false, commandOutput);

            var sut = new Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var exception = Assert.CatchAsync<DockerCommandException>(async () => await sut.ImageExistsAsync("testId"));

            // Assert
            exception.Message.Should().Contain(string.Join(Environment.NewLine, commandOutput));
        }

        private IRunCommands BuildMockCommandFactoryForScenario(bool success, string[] commandOutput)
        {
            var commandString = DockerCommands.Image.ListIds;
            var command = success
                ? MotherFor.CommandWrapper.ThatSucceeds().WithOutput(commandOutput).Build()
                : MotherFor.CommandWrapper.ThatFails().WithOutput(commandOutput).Build();

            var mockCommandFactory =
                MotherFor.CommandFactory
                    .ForWorkingDirectory(_workingDirectory)
                    .ForCommandReturn(commandString, command)
                    .Build();

            return mockCommandFactory;





            //var workingDirectory = Directory.GetCurrentDirectory();

            //var listContainersCommandResult = Substitute.For<ICommandResult>();
            //listContainersCommandResult.Success.Returns(success);

            //var listContainersCommand = Substitute.For<ICommandWrapper>();
            //listContainersCommand
            //    .Result
            //    .Returns(listContainersCommandResult);


            //listContainersCommand
            //    .GetOutputAndErrorLines()
            //    .Returns(commandOutput);

            //var mockCommandFactory = Substitute.For<IRunCommands>();
            //mockCommandFactory
            //    .RunCommand(DockerCommands.Container.ListIds, workingDirectory, Arg.Any<CancellationToken?>())
            //    .Returns(listContainersCommand);

            //return mockCommandFactory;
        }
    }
}
