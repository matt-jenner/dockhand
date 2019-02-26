using System;
using Dockhand.Exceptions;
using FluentAssertions;
using NUnit.Framework;

namespace Dockhand.Test.Exceptions
{
    [TestFixture]
    public class DockerContainerNotFoundExceptionTests
    {
        private const string ExpectedId = "testid";

        [Test]
        public void IdExceptionOnly_MessageContainsImageId()
        {
            // Act
            var sut = new DockerContainerNotFoundException(ExpectedId, new Exception());

            // Assert
            sut.Message.Should().Contain(ExpectedId);
        }

        [Test]
        public void IdExceptionOnly_InnerExceptionSet()
        {
            // Arrange
            var expectedException = new Exception();
            // Act
            var sut = new DockerImageNotFoundException(ExpectedId, expectedException);

            // Assert
            sut.InnerException.Should().Be(expectedException);
        }
    }
}
