using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dockhand.Client;
using Dockhand.Dtos;
using Dockhand.Exceptions;
using Dockhand.Interfaces;
using Dockhand.Models;
using Dockhand.Test.Builders;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Dockhand.Test.Models.DockerImageTests
{
    [TestFixture]
    public class StartContainerAsyncTests
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
        public async Task WhenNotDeleted_CallsStartContainer()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatSucceeds().WithOutput("twelvecharac").Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, null), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            await sut.StartContainerAsync();

            // Assert
            mockCommandFactory.Received(1).RunCommand(DockerCommands.Image.RunContainer(ExpectedId, null), _workingDirectory, Arg.Any<CancellationToken?>());
        }

        [Test]
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("\t")]
        [TestCase("idisshorten")]
        public void WhenCommandSucceeds_ButNoValidContainerIdInResponse(params string[] output)
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatSucceeds().WithOutput(output).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, null), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            var exception = Assert.CatchAsync(async () => await sut.StartContainerAsync());

            // Assert
            exception.Should().BeOfType<DockerCommandUnexpectedOutputException>();
        }

        [Test]
        public async Task WhenCommandSucceeds_ReturnsDockerContainer()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper
                .ThatSucceeds()
                .WithOutput("thisisacontaineridthatisover12chars")
                .Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, null), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            var result = await sut.StartContainerAsync();

            // Assert
            result.Should().BeOfType<DockerContainer>();
        }

        [Test]
        public async Task WhenCommandSucceeds_ResultContainsTruncatedContainerId()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);
            var fullContainerId = "thisislongerthantwelvecharacters";

            var mockCommand = MotherFor.CommandWrapper
                .ThatSucceeds()
                .WithOutput(fullContainerId)
                .Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, null), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var expectedResult = new DockerContainer(mockDockerClient, mockCommandFactory, "thisislonger", new DockerPortMapping[0]);

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            var result = await sut.StartContainerAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task WhenCommandSucceeds_DoesNotCallImageExists()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper
                .ThatSucceeds()
                .WithOutput("avalidcontainerid")
                .Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, null), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            await sut.StartContainerAsync();

            // Assert
            await mockDockerClient.DidNotReceive().ImageExistsAsync(Arg.Any<string>());
        }

        [Test]
        public async Task WhenCommandSucceeds_ItDoesNotChangeDeletedState()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper
                .ThatSucceeds()
                .WithOutput("avalidcontainerid")
                .Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, null), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);
            var initialDeletedState = sut.Deleted;

            // Act
            await sut.StartContainerAsync();

            // Assert
            sut.Deleted.Should().Be(initialDeletedState);
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(3)]
        public void WhenContainerMemoryLimitSpecifiedIsTooLow(int memoryLimitMb)
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, new StartContainerOptions().WithMemoryLimit(memoryLimitMb)), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            var exception = Assert.CatchAsync(async () => await sut.StartContainerAsync(o => o.WithMemoryLimit(memoryLimitMb)));

            // Assert
            exception.Should().BeOfType<ArgumentException>();
        }

        [Test]
        public void WhenContainerCpuLimitSpecifiedIsTooHigh()
        {
            // Arrange
            var aboveMaxCpuCount = Environment.ProcessorCount + 0.1m;

            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, new StartContainerOptions().WithCpuLimit(aboveMaxCpuCount)), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            var exception = Assert.CatchAsync(async () => await sut.StartContainerAsync(o => o.WithCpuLimit(aboveMaxCpuCount)));

            // Assert
            exception.Should().BeOfType<ArgumentException>();
        }

        [Test]
        public async Task WhenCommandFails_CallsImageExists()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, null), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            Assert.CatchAsync(async () => await sut.StartContainerAsync());

            // Assert
            await mockDockerClient.Received(1).ImageExistsAsync(Arg.Any<string>());
        }

        [Test]
        public void WhenCommandFails_ThrowsDockerCommandException()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, null), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            var exception = Assert.CatchAsync(async () => await sut.StartContainerAsync());

            // Assert
            exception.Should().BeOfType<DockerCommandException>();
        }
        
        [Test]
        public void WhenImageExistsSucceeds_ItSetsDeletedState()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, false);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, null), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            Assert.CatchAsync(async () => await sut.StartContainerAsync());

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
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, null), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            Assert.CatchAsync(async () => await sut.StartContainerAsync());

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

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage)
            {
                Deleted = true
            };

            // Act
            var exception = Assert.CatchAsync(async () => await sut.StartContainerAsync());

            // Assert
            exception.Should().BeOfType<DockerImageDeletedException>();
        }

        [Test]
        public void WhenDeleted_DoesNotCallStartContainerCommand()
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

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage)
            {
                Deleted = true
            };

            // Act
            Assert.CatchAsync(async () => await sut.StartContainerAsync());

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

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage)
            {
                Deleted = true
            };

            // Act
            Assert.CatchAsync(async () => await sut.StartContainerAsync());

            // Assert
            mockDockerClient.DidNotReceive().ImageExistsAsync(Arg.Any<string>());
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
