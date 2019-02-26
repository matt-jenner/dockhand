using Dockhand.Exceptions;
using FluentAssertions;
using NUnit.Framework;

namespace Dockhand.Test.Exceptions
{
    [TestFixture]
    public class DockerImageDeletedExceptionTests
    {
        [Test]
        [TestCase("testsomeid")]
        [TestCase("  testid")]
        public void MessageContainsId(string id)
        {
            // Act
            var sut = new DockerImageDeletedException(id);

            // Assert
            sut.Message.Should().Contain(id);
        }
    }
}
