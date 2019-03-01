using System;
using System.IO;
using Dockhand.Interfaces;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Dockhand.Test.Client.DockerClient
{
    [TestFixture]
    public class ForDirectoryTests
    {
        [Test]
        [TestCase("\\thisshouldntexist")]
        [TestCase("")]
        [TestCase((string)null)]
        public void ConstructorWithInvalidDirectory(string testPath)
        {
            // Act
            var exception = Assert.Catch(() => Dockhand.Client.DockerClient.ForDirectory(testPath));

            // Assert
            exception.Should().BeOfType<ArgumentException>();
        }

        [Test]
        public void ConstructorWithValidDirectory()
        { 
            // Act
            var sut = Dockhand.Client.DockerClient.ForDirectory(Directory.GetCurrentDirectory());

            // Assert
            sut.Should().BeOfType<Dockhand.Client.DockerClient>();
        }

        [Test]
        public void WorkingDirectoryIsSet()
        {
            // Arrange
            var mockCommandFactory = Substitute.For<IRunCommands>();
            var validDirectory = Directory.GetCurrentDirectory();
            // Act
            var sut = new Dockhand.Client.DockerClient(validDirectory, mockCommandFactory);

            // Assert
            sut.WorkingDirectory.Should().Be(validDirectory);
        }
    }
}
