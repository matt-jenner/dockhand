using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dockhand.Client;
using Dockhand.Dtos;
using Dockhand.Exceptions;
using Dockhand.Interfaces;
using Dockhand.Models;
using Dockhand.Test.Builders;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Dockhand.Test.DockerClient
{
    [TestFixture]
    public class BuildImageAsyncTests
    {
        private string _workingDirectory;
        private const string ExpectedImageId = "imageId";
        private const string ExpectedDockerFile = "testdockerfile";
        private const string ExpectedTarget = "testTarget";
        private const string ExpectedRepository = "testRepository";
        private const string ExpectedTag = "testTag";

        [SetUp]
        public void SetUp()
        {
            _workingDirectory = Directory.GetCurrentDirectory();
        }

        [Test]
        public async Task BothCommandsAreSuccessfulImageCreated_ReturnsDockerImage()
        {
            // Arrange
            var imageOutput = new[] {new DockerImageResult {Id = ExpectedImageId, Repository = ExpectedRepository, Tag = ExpectedTag} };
            var buildOutput = new[] {"buildOutput"};
            var mockCommandFactory = BuildMockCommandFactoryForScenario(true, buildOutput, true, imageOutput);

            var sut = new Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var result = await sut.BuildImageAsync(ExpectedDockerFile, ExpectedTarget, ExpectedRepository, ExpectedTag);

            // Assert
            result.Should().BeOfType<DockerImage>();
        }

        [Test]
        public async Task BothCommandsAreSuccessfulImageCreated_DockerImageHasCorrectId()
        {
            // Arrange
            var imageOutput = new[] { new DockerImageResult { Id = ExpectedImageId, Repository = ExpectedRepository, Tag = ExpectedTag } };
            var buildOutput = new[] { "buildOutput" };
            var mockCommandFactory = BuildMockCommandFactoryForScenario(true, buildOutput, true, imageOutput);

            var sut = new Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var result = await sut.BuildImageAsync(ExpectedDockerFile, ExpectedTarget, ExpectedRepository, ExpectedTag);

            // Assert
            result.Id.Should().Be(ExpectedImageId);
        }

        [Test]
        public async Task BothCommandsAreSuccessfulImageCreated_DockerImageHasCorrectRepository()
        {
            // Arrange
            var imageOutput = new[] { new DockerImageResult { Id = ExpectedImageId, Repository = ExpectedRepository, Tag = ExpectedTag } };
            var buildOutput = new[] { "buildOutput" };
            var mockCommandFactory = BuildMockCommandFactoryForScenario(true, buildOutput, true, imageOutput);

            var sut = new Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var result = await sut.BuildImageAsync(ExpectedDockerFile, ExpectedTarget, ExpectedRepository, ExpectedTag);

            // Assert
            result.Repository.Should().Be(ExpectedRepository);
        }

        [Test]
        public async Task BothCommandsAreSuccessfulImageCreated_DockerImageHasCorrectTag()
        {
            // Arrange
            var imageOutput = new[] { new DockerImageResult { Id = ExpectedImageId, Repository = ExpectedRepository, Tag = ExpectedTag } };
            var buildOutput = new[] { "buildOutput" };
            var mockCommandFactory = BuildMockCommandFactoryForScenario(true, buildOutput, true, imageOutput);

            var sut = new Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var result = await sut.BuildImageAsync(ExpectedDockerFile, ExpectedTarget, ExpectedRepository, ExpectedTag);

            // Assert
            result.Tag.Should().Be(ExpectedTag);
        }

        [Test]
        public void BuildCommandIsSuccessfulContainerNotExist()
        {
            // Arrange
            var mockCommandFactory = BuildMockCommandFactoryForScenario(true, new string[0], true, new DockerImageResult[0]);

            var sut = new Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var exception = Assert.CatchAsync(async() => await sut.BuildImageAsync(ExpectedDockerFile, ExpectedTarget, ExpectedRepository, ExpectedTag));

            // Assert
            exception.Should().BeOfType<DockerImageNotFoundException>();
        }

        [Test]
        public void BuildCommandIsSuccessfulGetImageFails()
        {
            // Arrange
            var mockCommandFactory = BuildMockCommandFactoryForScenario(true, new string[0], false, new DockerImageResult[0]);

            var sut = new Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var exception = Assert.CatchAsync(async () => await sut.BuildImageAsync(ExpectedDockerFile, ExpectedTarget, ExpectedRepository, ExpectedTag));

            // Assert
            exception.Should().BeOfType<DockerCommandException>();
        }

        [Test]
        public void BuildCommandIsNotSuccessfulExceptionThrown()
        {
            // Arrange
            var mockCommandFactory = BuildMockCommandFactoryForScenario(false, new string[0], true, new DockerImageResult[0]);

            var sut = new Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var exception = Assert.CatchAsync(async () => await sut.BuildImageAsync(ExpectedDockerFile, ExpectedTarget, ExpectedRepository, ExpectedTag));

            // Assert
            exception.Should().BeOfType<DockerCommandException>();
        }

        private IRunCommands BuildMockCommandFactoryForScenario(bool buildSucceeds, string[] buildOutput, bool listImagesSucceeds, DockerImageResult[] listImagesOutput)
        {

            var buildCommandString = DockerCommands.Image.Build(ExpectedDockerFile, ExpectedTarget, ExpectedRepository, ExpectedTag);
            var buildCommand = buildSucceeds
                ? MotherFor.CommandWrapper.ThatSucceeds().WithOutput(buildOutput).Build()
                : MotherFor.CommandWrapper.ThatFails().WithOutput(buildOutput).Build();
            
            var listImagesCommandString = DockerCommands.Image.List();

            var listImagesCommand = listImagesSucceeds ? 
                MotherFor.CommandWrapper.ThatSucceeds().WithOutput(listImagesOutput.Select(JsonConvert.SerializeObject)).Build() : 
                MotherFor.CommandWrapper.ThatFails().WithOutput(listImagesOutput.Select(JsonConvert.SerializeObject)).Build();
            
            var mockCommandFactory =
                MotherFor.CommandFactory
                    .ForWorkingDirectory(_workingDirectory)
                    .ForCommandReturn(buildCommandString, buildCommand)
                    .ForCommandReturn(listImagesCommandString, listImagesCommand)
                    .Build();

            return mockCommandFactory;
        }
    }
}
