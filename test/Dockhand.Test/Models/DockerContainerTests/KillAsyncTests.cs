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
    public class KillAsyncTests
    {
        private string _workingDirectory;
        private const string ExpectedId = "testId";

        [SetUp]
        public void SetUp()
        {
            _workingDirectory = Directory.GetCurrentDirectory();
        }

        [Test]
        public async Task WhenNotDeleted_CallsKillCommand()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatSucceeds().Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Container.Kill(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            await sut.KillAsync();

            // Assert
            mockCommandFactory.Received(1).RunCommand(DockerCommands.Container.Kill(ExpectedId), _workingDirectory, Arg.Any<CancellationToken?>());
        }

        [Test]
        public async Task WhenKillSucceeds_DoesNotCallContainerExists()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatSucceeds().Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Container.Kill(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            await sut.KillAsync();

            // Assert
            await mockDockerClient.DidNotReceive().ContainerExistsAsync(Arg.Any<string>());
        }

        [Test]
        public async Task WhenKillFails_CallsContainerExists()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Container.Kill(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            Assert.CatchAsync(async () => await sut.KillAsync());

            // Assert
            await mockDockerClient.Received(1).ContainerExistsAsync(Arg.Any<string>());
        }

        [Test]
        public void WhenKillFails_ThrowsDockerCommandException()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Container.Kill(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            var exception = Assert.CatchAsync(async () => await sut.KillAsync());

            // Assert
            exception.Should().BeOfType<DockerCommandException>();
        }

        [Test]
        public void WhenContainerExistsSucceeds_ItSetsDeletedState()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, false);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Container.Kill(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            Assert.CatchAsync(async () => await sut.KillAsync());

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
                .ForCommandReturn(DockerCommands.Container.Kill(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            Assert.CatchAsync(async () => await sut.KillAsync());

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
            var exception = Assert.CatchAsync(async () => await sut.KillAsync());

            // Assert
            exception.Should().BeOfType<DockerContainerDeletedException>();
        }

        [Test]
        public void WhenDeleted_DoesNotCallKillCommand()
        {
            // Arrange
            var mockDockerClient = Substitute.For<IDockerClient>();
            var mockCommandFactory = Substitute.For<IRunCommands>();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0])
            {
                Deleted = true
            };

            // Act
            Assert.CatchAsync(async () => await sut.KillAsync());

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
            Assert.CatchAsync(async () => await sut.KillAsync());

            // Assert
            mockDockerClient.DidNotReceive().ContainerExistsAsync(Arg.Any<string>());
        }

        private IRunCommands BuildMockCommandFactoryForScenario(bool killSucceeds, string[] killOutput)
        {

            var killCommandString = DockerCommands.Container.Kill(ExpectedId);
            var killCommand = killSucceeds
                ? MotherFor.CommandWrapper.ThatSucceeds().WithOutput(killOutput).Build()
                : MotherFor.CommandWrapper.ThatFails().WithOutput(killOutput).Build();


            var mockCommandFactory =
                MotherFor.CommandFactory
                    .ForWorkingDirectory(_workingDirectory)
                    .ForCommandReturn(killCommandString, killCommand)
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
