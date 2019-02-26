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
        [TestCase(null, null, null, null)]
        [TestCase(1, 80, null, null)]
        [TestCase(5, 30, 3, 14)]
        public async Task WhenNotDeleted_CallStartContainer(int? internalPort1, int? externalPort1, int? internalPort2, int? externalPort2)
        {
            // Arrange
            var expectedPortMappings = new List<DockerPortMapping>();
            if (internalPort1.HasValue && externalPort1.HasValue)
            {
                expectedPortMappings.Add(new DockerPortMapping(internalPort1.Value, externalPort1.Value));
            }

            if (internalPort2.HasValue && externalPort2.HasValue)
            {
                expectedPortMappings.Add(new DockerPortMapping(internalPort2.Value, externalPort2.Value));
            }

            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatSucceeds().WithOutput("twelvecharac").Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, expectedPortMappings), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            await sut.StartContainerAsync(expectedPortMappings.ToArray());

            // Assert
            mockCommandFactory.Received(1).RunCommand(DockerCommands.Image.RunContainer(ExpectedId, expectedPortMappings), _workingDirectory, Arg.Any<CancellationToken?>());
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
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, new DockerPortMapping[0]), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            var exception = Assert.CatchAsync(async () => await sut.StartContainerAsync(new DockerPortMapping[0]));

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
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, new DockerPortMapping[0]), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            var result = await sut.StartContainerAsync(new DockerPortMapping[0]);

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
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, new DockerPortMapping[0]), mockCommand)
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
            var result = await sut.StartContainerAsync(new DockerPortMapping[0]);

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
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, new DockerPortMapping[0]), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            await sut.StartContainerAsync(new DockerPortMapping[0]);

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
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, new DockerPortMapping[0]), mockCommand)
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
            await sut.StartContainerAsync(new DockerPortMapping[0]);

            // Assert
            sut.Deleted.Should().Be(initialDeletedState);
        }

        [Test]
        public async Task WhenStartContainerFails_CallsImageExists()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, new DockerPortMapping[0]), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            Assert.CatchAsync(async () => await sut.StartContainerAsync(new DockerPortMapping[0]));

            // Assert
            await mockDockerClient.Received(1).ImageExistsAsync(Arg.Any<string>());
        }

        [Test]
        public void WhenStartContainerFails_ThrowsDockerCommandException()
        {
            // Arrange
            var mockDockerClient = BuildMockDockerClient(true, true);

            var mockCommand = MotherFor.CommandWrapper.ThatFails().WithExitCode(-1).Build();

            var mockCommandFactory = MotherFor.CommandFactory
                .ForWorkingDirectory(_workingDirectory)
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, new DockerPortMapping[0]), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            var exception = Assert.CatchAsync(async () => await sut.StartContainerAsync(new DockerPortMapping[0]));

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
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, new DockerPortMapping[0]), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            Assert.CatchAsync(async () => await sut.StartContainerAsync(new DockerPortMapping[0]));

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
                .ForCommandReturn(DockerCommands.Image.RunContainer(ExpectedId, new DockerPortMapping[0]), mockCommand)
                .Build();

            var dockerImage = new DockerImageResult
            {
                Id = ExpectedId,
                Repository = ExpectedRepository,
                Tag = ExpectedTag
            };

            var sut = new DockerImage(mockDockerClient, mockCommandFactory, dockerImage);

            // Act
            Assert.CatchAsync(async () => await sut.StartContainerAsync(new DockerPortMapping[0]));

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
            var exception = Assert.CatchAsync(async () => await sut.StartContainerAsync( new DockerPortMapping[0]));

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
            Assert.CatchAsync(async () => await sut.StartContainerAsync(new DockerPortMapping[0]));

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
            Assert.CatchAsync(async () => await sut.StartContainerAsync(new DockerPortMapping[0]));

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
