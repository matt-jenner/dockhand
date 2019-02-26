using System;
using Dockhand.Exceptions;
using FluentAssertions;
using NUnit.Framework;

namespace Dockhand.Test.Exceptions
{
    [TestFixture]
    public class DockerCommandExceptionTests
    {
        [Test]
        [TestCase("test command")]
        [TestCase("")]
        [TestCase((string)null)]
        public void MessageContainsCommand(string command)
        {
            // Act
            var sut = new DockerCommandException(command, new string[0]);

            // Assert
            sut.Message.Should().Contain($"Command: {command}{Environment.NewLine}");
        }

        [Test]
        public void MessageContainsOutput()
        {
            // Arrange
            var output = new[]
            {
                "output1",
                string.Empty,
                null,
                "/t",
                "output4"
            };

            // Act
            var sut = new DockerCommandException("test command", output);

            // Assert
            sut.Message.Should().Contain($"Process Output:{Environment.NewLine}{string.Join(Environment.NewLine, output)}");
        }
    }
}
