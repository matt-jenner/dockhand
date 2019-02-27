using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dockhand.Client;
using Dockhand.Dtos;
using Dockhand.Exceptions;
using Dockhand.Interfaces;
using Dockhand.Test.Builders;
using ExpectedObjects;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Dockhand.Test.DockerClient
{
    [TestFixture]
    public class GetImagesAsyncTests
    {
        private string _workingDirectory;
        private const string ExpectedImageId1 = "imageId1";
        private const string ExpectedRepository1 = "testRepository1";
        private const string ExpectedTag1 = "testTag1";
        private const string ExpectedImageId2 = "imageId2";
        private const string ExpectedRepository2 = "testRepository2";
        private const string ExpectedTag2 = "testTag2";

        [SetUp]
        public void SetUp()
        {
            _workingDirectory = Directory.GetCurrentDirectory();
        }

        [Test]
        public async Task CommandsIsSuccessful_ReturnsDockerImages()
        {
            // Arrange
            var imageOutput = new[]
            {
                new DockerImageResult { Id = ExpectedImageId1, Repository = ExpectedRepository1, Tag = ExpectedTag1 },
                new DockerImageResult { Id = ExpectedImageId2, Repository = ExpectedRepository2, Tag = ExpectedTag2 }
            };
            var mockCommandFactory = BuildMockCommandFactoryForScenario(true, imageOutput);

            var sut = new Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var result = await sut.GetImagesAsync();

            // Assert
            var expectedResult = new [] 
            {
                new { Id = ExpectedImageId1, Repository = ExpectedRepository1, Tag = ExpectedTag1, Deleted = false },
                new { Id = ExpectedImageId2, Repository = ExpectedRepository2, Tag = ExpectedTag2, Deleted = false }
            }.ToExpectedObject();

            expectedResult.ShouldMatch(result);
        }

        [Test]
        public async Task CommandsIsSuccessful_NoDockerImages()
        {
            // Arrange
            var imageOutput = new DockerImageResult[0];
            var mockCommandFactory = BuildMockCommandFactoryForScenario(true, imageOutput);

            var sut = new Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var result = await sut.GetImagesAsync();

            // Assert
            result.Count().Should().Be(0);
        }

        [Test]
        public void CommandIsNotSuccessfulExceptionThrown()
        {
            // Arrange
            var mockCommandFactory = BuildMockCommandFactoryForScenario(false, new DockerImageResult[0]);

            var sut = new Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var exception = Assert.CatchAsync(async () => await sut.GetImagesAsync());

            // Assert
            exception.Should().BeOfType<DockerCommandException>();
        }

        private IRunCommands BuildMockCommandFactoryForScenario(bool getImagesSucceeds, DockerImageResult[] getImagesOutput)
        {
            return BuildMockCommandFactoryForScenario(getImagesSucceeds, getImagesOutput.Select(JsonConvert.SerializeObject).ToArray());
        }

        private IRunCommands BuildMockCommandFactoryForScenario(bool getImagesSucceeds, string[] output)
        {
           
            var commandString = DockerCommands.Image.List();

            var command = getImagesSucceeds ? 
                MotherFor.CommandWrapper.ThatSucceeds().WithOutput(output).Build() : 
                MotherFor.CommandWrapper.ThatFails().WithOutput(output).Build();
            
            var mockCommandFactory =
                MotherFor.CommandFactory
                    .ForWorkingDirectory(_workingDirectory)
                    .ForCommandReturn(commandString, command)
                    .Build();

            return mockCommandFactory;
        }
    }
}
