using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dockhand.Client;
using Dockhand.Dtos;
using Dockhand.Exceptions;
using Dockhand.Interfaces;
using Dockhand.Test.Builders;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Dockhand.Test.Models.DockerImageTests
{
    [TestFixture]
    public class RemoveAsyncTests
    {
        private string _workingDirectory;
        private const string ExpectedId = "testId";
        private const string ExpectedRepository = "testRepository";
        private const string ExpectedTag = "testTag";

        [SetUp]
        public void SetUp()
        {
            _workingDirectory = Directory.GetCurrentDirectory();
        }

        [Test]
        public async Task WhenNotDeleted_CallsRemoveCommand()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatSucceeds().Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.Remove(ExpectedId), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new Dockhand.Models.DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            await sut.RemoveAsync();

            // Assert
            mockCommandFactory.Received(1).RunCommand(DockerCommands.Image.Remove(ExpectedId), _workingDirectory, Arg.Any<CancellationToken?>());
        }

        [Test]
        public async Task WhenRemoveSucceeds_DoesNotCallImageExists()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatSucceeds().Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.Remove(ExpectedId), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new Dockhand.Models.DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            await sut.RemoveAsync();

            // Assert
            await mockDockerClient.DidNotReceive().ImageExistsAsync(Arg.Any<string>());
        }

        [Test]
        public async Task WhenRemoveSucceeds_ItSetsDeleted()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatSucceeds().Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.Remove(ExpectedId), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new Dockhand.Models.DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            await sut.RemoveAsync();

            // Assert
            sut.Deleted.Should().BeTrue();
        }

        [Test]
        public async Task WhenRemoveFails_CallsImageExists()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.Remove(ExpectedId), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new Dockhand.Models.DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            Assert.CatchAsync(async () => await sut.RemoveAsync());

            // Assert
            await mockDockerClient.Received(1).ImageExistsAsync(Arg.Any<string>());
        }

        [Test]
        public void WhenRemoveFails_ThrowsDockerCommandException()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.Remove(ExpectedId), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new Dockhand.Models.DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            var exception = Assert.CatchAsync(async () => await sut.RemoveAsync());

            // Assert
            exception.Should().BeOfType<DockerCommandException>();
        }

        [Test]
        public void WhenRemoveFails_ItDoesNotSetDeletedState()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.Remove(ExpectedId), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new Dockhand.Models.DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            Assert.CatchAsync(async () => await sut.RemoveAsync());

            // Assert
            sut.Deleted.Should().BeFalse();
        }

        [Test]
        public void WhenImageExistsSucceeds_ItSetsDeletedState()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, false);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.Remove(ExpectedId), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new Dockhand.Models.DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            Assert.CatchAsync(async () => await sut.RemoveAsync());

            // Assert
            sut.Deleted.Should().BeTrue();
        }

        [Test]
        public void WhenImageExistsFails_ItDoesNotSetDeletedState()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(false, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.Remove(ExpectedId), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new Dockhand.Models.DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            Assert.CatchAsync(async () => await sut.RemoveAsync());

            // Assert
            sut.Deleted.Should().BeFalse();
        }

        [Test]
        public void WhenDeleted_ThrowsDockerImageDeletedException()
        {
            // Arrange
            var mockDockerClient = Substitute.For<IDockerClient>();
            var mockCommandFactory = Substitute.For<IRunCommands>();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new Dockhand.Models.DockerImage(mockDockerClient, mockCommandFactory, dockerImage)
            {
                Deleted = true
            };

            // Act
            var exception = Assert.CatchAsync(async () => await sut.RemoveAsync());

            // Assert
            exception.Should().BeOfType<DockerImageDeletedException>();
        }

        [Test]
        public void WhenDeleted_DoesNotCallRemoveCommand()
        {
            // Arrange
            var mockDockerClient = Substitute.For<IDockerClient>();
            var mockCommandFactory = Substitute.For<IRunCommands>();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new Dockhand.Models.DockerImage(mockDockerClient, mockCommandFactory, dockerImage)
            {
                Deleted = true
            };

            // Act
            Assert.CatchAsync(async () => await sut.RemoveAsync());

            // Assert
            mockCommandFactory.DidNotReceive().RunCommand(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken?>());
        }

        [Test]
        public void WhenDeleted_DoesNotCallImageExists()
        {
            // Arrange
            var mockDockerClient = Substitute.For<IDockerClient>();
            var mockCommandFactory = Substitute.For<IRunCommands>();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new Dockhand.Models.DockerImage(mockDockerClient, mockCommandFactory, dockerImage)
            {
                Deleted = true
            };

            // Act
            Assert.CatchAsync(async () => await sut.RemoveAsync());

            // Assert
            mockDockerClient.DidNotReceive().ImageExistsAsync(Arg.Any<string>());
        }

        private IRunCommands BuildMockCommandFactoryForScenario(bool removeSucceeds, string[] removeOutput)
        {

            var removeCommandString = DockerCommands.Image.Remove(ExpectedId);
            var removeCommand = removeSucceeds
                ? MotherFor.CommandWrapper.ThatSucceeds().WithOutput(removeOutput).Build()
                : MotherFor.CommandWrapper.ThatFails().WithOutput(removeOutput).Build();


            var mockCommandFactory =
                MotherFor.CommandFactory
                    .ForWorkingDirectory(_workingDirectory)
                    .ForCommandReturn(removeCommandString, removeCommand)
                    .Build();

            return mockCommandFactory;
        }

        private IDockerClient BuildMockDockerClient(bool imageExistsSucceeds, bool imageExists)
        {
            var mockDockerClient = Substitute.For<IDockerClient>();
            if (imageExistsSucceeds)
            {
                mockDockerClient.ImageExistsAsync(ExpectedId).Returns(Task.FromResult(imageExists));
            }
            else
            {
                mockDockerClient.ImageExistsAsync(ExpectedId).Throws(new DockerCommandException("testcommand", new string[0]));
            }

            mockDockerClient.WorkingDirectory.Returns(_workingDirectory);

            return mockDockerClient;
        }
    }
}
