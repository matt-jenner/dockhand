using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dockhand.Dtos;
using Dockhand.Exceptions;
using Dockhand.Interfaces;
using Dockhand.Test.Builders;
using ExpectedObjects;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Dockhand.Test.Client.DockerClient
{
    [TestFixture]
    public class GetImageAsyncTests
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
        public async Task CommandsIsSuccessful_ImageFound()
        {
            // Arrange
            var imageOutput = new[]
            {
                new DockerImageResult { Id = ExpectedImageId1, Repository = ExpectedRepository1, Tag = ExpectedTag1 },
                new DockerImageResult { Id = ExpectedImageId2, Repository = ExpectedRepository2, Tag = ExpectedTag2 }
            };
            var mockCommandFactory = BuildMockCommandFactoryForScenario(true, imageOutput);
            var expectedResult = new { Id = ExpectedImageId1, Repository = ExpectedRepository1, Tag = ExpectedTag1, Deleted = false }.ToExpectedObject();

            var sut = new Dockhand.Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var result = await sut.GetImageAsync(ExpectedRepository1, ExpectedTag1);

            // Assert
            expectedResult.ShouldMatch(result);
        }

        [Test]
        public void CommandsIsSuccessful_ImageNotFound_ExceptionType()
        {
            // Arrange

            var imageOutput = new[]
            {
                new DockerImageResult { Id = ExpectedImageId1, Repository = ExpectedRepository1, Tag = ExpectedTag1 },
                new DockerImageResult { Id = ExpectedImageId2, Repository = ExpectedRepository2, Tag = ExpectedTag2 }
            };
            var mockCommandFactory = BuildMockCommandFactoryForScenario(true, imageOutput);

            var sut = new Dockhand.Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var exception = Assert.CatchAsync(async () => await sut.GetImageAsync("wontfindthisrepo", ExpectedTag1));

            // Assert
            exception.Should().BeOfType<DockerImageNotFoundException>();
        }

        [Test]
        public void CommandIsNotSuccessfulExceptionThrown()
        {
            // Arrange
            var mockCommandFactory = BuildMockCommandFactoryForScenario(false, new DockerImageResult[0]);

            var sut = new Dockhand.Client.DockerClient(_workingDirectory, mockCommandFactory);

            // Act
            var exception = Assert.CatchAsync(async () => await sut.GetImageAsync(ExpectedRepository1, ExpectedTag1));

            // Assert
            exception.Should().BeOfType<DockerCommandException>();
        }

        private IRunCommands BuildMockCommandFactoryForScenario(bool getImagesSucceeds, DockerImageResult[] getImagesOutput)
        {
            return BuildMockCommandFactoryForScenario(getImagesSucceeds, getImagesOutput.Select(JsonConvert.SerializeObject).ToArray());
        }

        private IRunCommands BuildMockCommandFactoryForScenario(bool getImagesSucceeds, string[] output)
        {
           
            var commandString = Dockhand.Client.DockerCommands.Image.List();

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
