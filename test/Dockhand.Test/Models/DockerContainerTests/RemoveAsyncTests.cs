using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dockhand.Client;
using Dockhand.Exceptions;
using Dockhand.Interfaces;
using Dockhand.Models;
using Dockhand.Test.Builders;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Dockhand.Test.Models.DockerContainerTests
{
    [TestFixture]
    public class RemoveAsyncTests
    {
        private string _workingDirectory;
        private const string ExpectedId = "testId";

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
                .ForCommandReturn(DockerCommands.Container.Remove(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            await sut.RemoveAsync();

            // Assert
            mockCommandFactory.Received(1).RunCommand(DockerCommands.Container.Remove(ExpectedId), _workingDirectory, Arg.Any<CancellationToken?>());
        }

        [Test]
        public async Task WhenRemoveSucceeds_DoesNotCallContainerExists()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatSucceeds().Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Container.Remove(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            await sut.RemoveAsync();

            // Assert
            await mockDockerClient.DidNotReceive().ContainerExistsAsync(Arg.Any<string>());
        }

        [Test]
        public async Task WhenRemoveSucceeds_ItSetsDeleted()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatSucceeds().Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Container.Remove(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            await sut.RemoveAsync();

            // Assert
            sut.Deleted.Should().BeTrue();
        }

        [Test]
        public async Task WhenRemoveFails_CallsContainerExists()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Container.Remove(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            Assert.CatchAsync(async () => await sut.RemoveAsync());

            // Assert
            await mockDockerClient.Received(1).ContainerExistsAsync(Arg.Any<string>());
        }

        [Test]
        public void WhenRemoveFails_ThrowsDockerCommandException()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Container.Remove(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

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
                .ForCommandReturn(DockerCommands.Container.Remove(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            Assert.CatchAsync(async () => await sut.RemoveAsync());

            // Assert
            sut.Deleted.Should().BeFalse();
        }

        [Test]
        public void WhenContainerExistsSucceeds_ItSetsDeletedState()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, false);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Container.Remove(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            Assert.CatchAsync(async () => await sut.RemoveAsync());

            // Assert
            sut.Deleted.Should().BeTrue();
        }

        [Test]
        public void WhenContainerExistsFails_ItDoesNotSetDeletedState()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(false, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Container.Remove(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            Assert.CatchAsync(async () => await sut.RemoveAsync());

            // Assert
            sut.Deleted.Should().BeFalse();
        }

        [Test]
        public void WhenDeleted_ThrowsDockerContainerDeletedException()
        {
            // Arrange
            var mockDockerClient = Substitute.For<IDockerClient>();
            var mockCommandFactory = Substitute.For<IRunCommands>();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0])
            {
                Deleted = true
            };

            // Act
            var exception = Assert.CatchAsync(async () => await sut.RemoveAsync());

            // Assert
            exception.Should().BeOfType<DockerContainerDeletedException>();
        }

        [Test]
        public void WhenDeleted_DoesNotCallRemoveCommand()
        {
            // Arrange
            var mockDockerClient = Substitute.For<IDockerClient>();
            var mockCommandFactory = Substitute.For<IRunCommands>();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0])
            {
                Deleted = true
            };

            // Act
            Assert.CatchAsync(async () => await sut.RemoveAsync());

            // Assert
            mockCommandFactory.DidNotReceive().RunCommand(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken?>());
        }

        [Test]
        public void WhenDeleted_DoesNotCallContainerExists()
        {
            // Arrange
            var mockDockerClient = Substitute.For<IDockerClient>();
            var mockCommandFactory = Substitute.For<IRunCommands>();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0])
            {
                Deleted = true
            };

            // Act
            Assert.CatchAsync(async () => await sut.RemoveAsync());

            // Assert
            mockDockerClient.DidNotReceive().ContainerExistsAsync(Arg.Any<string>());
        }

        private IRunCommands BuildMockCommandFactoryForScenario(bool removeSucceeds, string[] removeOutput)
        {

            var removeCommandString = DockerCommands.Container.Remove(ExpectedId);
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

        private IDockerClient BuildMockDockerClient(bool containerExistsSucceeds, bool containerExists)
        {
            var mockDockerClient = Substitute.For<IDockerClient>();
            if (containerExistsSucceeds)
            {
                mockDockerClient.ContainerExistsAsync(ExpectedId).Returns(Task.FromResult(containerExists));
            }
            else
            {
                mockDockerClient.ContainerExistsAsync(ExpectedId).Throws(new DockerCommandException("testcommand", new string[0]));
            }

            mockDockerClient.WorkingDirectory.Returns(_workingDirectory);

            return mockDockerClient;
        }
    }
}
