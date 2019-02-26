using System;
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
    public class GetContainerStatsTests
    {
        private string _workingDirectory;
        private const string ExpectedId = "testId";

        [SetUp]
        public void SetUp()
        {
            _workingDirectory = Directory.GetCurrentDirectory();
        }

        [Test]
        public async Task WhenNotDeleted_CallsMonitorStatsForCommand()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper
                .ThatSucceeds()
                .WithOutput(MotherFor.ContainerStatsJson(1m, 1m).Build())
                .Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Container.GetStats(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            await sut.MonitorStatsFor(TimeSpan.Zero);

            // Assert
            mockCommandFactory.Received().RunCommand(DockerCommands.Container.GetStats(ExpectedId), _workingDirectory, Arg.Any<CancellationToken?>());
        }

        [Test]
        public async Task WhenMonitorStatsForSucceeds_DoesNotCallContainerExists()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatSucceeds()
                .WithOutput(MotherFor.ContainerStatsJson(1m,1m).Build())
                .Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Container.GetStats(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            await sut.MonitorStatsFor(TimeSpan.Zero);

            // Assert
            await mockDockerClient.DidNotReceive().ContainerExistsAsync(Arg.Any<string>());
        }

        [Test]
        public async Task WhenMonitorStatsForFails_CallsContainerExists()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Container.GetStats(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            Assert.CatchAsync(async () => await sut.MonitorStatsFor(TimeSpan.Zero));

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
                .ForCommandReturn(DockerCommands.Container.GetStats(ExpectedId), mockCommand)
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
                .ForCommandReturn(DockerCommands.Container.GetStats(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            Assert.CatchAsync(async () => await sut.MonitorStatsFor(TimeSpan.Zero));

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
                .ForCommandReturn(DockerCommands.Container.GetStats(ExpectedId), mockCommand)
                .Build();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0]);

            // Act
            Assert.CatchAsync(async () => await sut.MonitorStatsFor(TimeSpan.Zero));

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
            var exception = Assert.CatchAsync(async () => await sut.MonitorStatsFor(TimeSpan.Zero));

            // Assert
            exception.Should().BeOfType<DockerContainerDeletedException>();
        }

        [Test]
        public void WhenDeleted_DoesNotCallMonitorStatsForCommand()
        {
            // Arrange
            var mockDockerClient = Substitute.For<IDockerClient>();
            var mockCommandFactory = Substitute.For<IRunCommands>();

            var sut = new DockerContainer(mockDockerClient, mockCommandFactory, ExpectedId, new DockerPortMapping[0])
            {
                Deleted = true
            };

            // Act
            Assert.CatchAsync(async () => await sut.MonitorStatsFor(TimeSpan.Zero));

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
            Assert.CatchAsync(async () => await sut.MonitorStatsFor(TimeSpan.Zero));

            // Assert
            mockDockerClient.DidNotReceive().ContainerExistsAsync(Arg.Any<string>());
        }

        private IRunCommands BuildMockCommandFactoryForScenario(bool getStatsSucceeds, string[] statsOutput)
        {

            var getStatsCommandString = DockerCommands.Container.GetStats(ExpectedId);
            var getStatsCommand = getStatsSucceeds
                ? MotherFor.CommandWrapper.ThatSucceeds().WithOutput(statsOutput).Build()
                : MotherFor.CommandWrapper.ThatFails().WithOutput(statsOutput).Build();


            var mockCommandFactory =
                MotherFor.CommandFactory
                    .ForWorkingDirectory(_workingDirectory)
                    .ForCommandReturn(getStatsCommandString, getStatsCommand)
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
