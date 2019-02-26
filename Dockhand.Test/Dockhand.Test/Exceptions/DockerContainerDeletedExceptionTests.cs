using System;
using Dockhand.Exceptions;
using FluentAssertions;
using NUnit.Framework;

namespace Dockhand.Test.Exceptions
{
    [TestFixture]
    public class DockerContainerDeletedExceptionTests
    {
        [Test]
        [TestCase("testsomeid")]
        [TestCase("  testid")]
        public void MessageContainsId(string id)
        {
            // Act
            var sut = new DockerContainerDeletedException(id);

            // Assert
            sut.Message.Should().Contain(id);
        }
    }
}
