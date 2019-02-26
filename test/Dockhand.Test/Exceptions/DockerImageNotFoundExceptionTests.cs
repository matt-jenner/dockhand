using System;
using Dockhand.Exceptions;
using FluentAssertions;
using NUnit.Framework;

namespace Dockhand.Test.Exceptions
{
    [TestFixture]
    public class DockerImageNotFoundExceptionTests
    {
        private const string ExpectedId = "testid";
        private const string ExpectedTag = "testtag";
        private const string ExpectedRepository = "testrepo";


        [Test]
        public void IdExceptionOnly_MessageContainsImageId()
        {
            // Act
            var sut = new DockerImageNotFoundException(ExpectedId, new Exception());

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

        [Test]
        public void RepoTag_MessageContainsImageRepository()
        {
            // Act
            var sut = new DockerImageNotFoundException(ExpectedRepository, ExpectedTag);

            // Assert
            sut.Message.Should().Contain(ExpectedRepository);
        }

        [Test]
        public void RepoTag_MessageContainsImageTag()
        {
            // Act
            var sut = new DockerImageNotFoundException(ExpectedRepository, ExpectedTag);

            // Assert
            sut.Message.Should().Contain(ExpectedTag);
        }

        [Test]
        public void RepoTagException_MessageContainsImageRepository()
        {
            // Act
            var sut = new DockerImageNotFoundException(ExpectedRepository, ExpectedTag, new Exception());

            // Assert
            sut.Message.Should().Contain(ExpectedRepository);
        }

        [Test]
        public void RepoTagException_MessageContainsImageTag()
        {
            // Act
            var sut = new DockerImageNotFoundException(ExpectedRepository, ExpectedTag, new Exception());

            // Assert
            sut.Message.Should().Contain(ExpectedTag);
        }

        [Test]
        public void RepoTagException_InnerExceptionSet()
        {
            // Arrange
            var expectedException = new Exception();
            // Act
            var sut = new DockerImageNotFoundException(ExpectedRepository, ExpectedTag, expectedException);

            // Assert
            sut.InnerException.Should().Be(expectedException);
        }

        [Test]
        public void RepositoryTagIdException_MessageContainsImageTag()
        {
            // Act
            var sut = new DockerImageNotFoundException(ExpectedId, ExpectedTag, new Exception());

            // Assert
            sut.Message.Should().Contain(ExpectedTag);
        }

        [Test]
        public void RepositoryTagIdException_MessageContainsImageRepository()
        {
            // Act
            var sut = new DockerImageNotFoundException(ExpectedId, ExpectedTag, ExpectedRepository,new Exception());

            // Assert
            sut.Message.Should().Contain(ExpectedRepository);
        }

        [Test]
        public void RepositoryTagIdException_MessageContainsImageId()
        {
            // Act
            var sut = new DockerImageNotFoundException(ExpectedId, ExpectedTag, ExpectedRepository, new Exception());

            // Assert
            sut.Message.Should().Contain(ExpectedId);
        }

        [Test]
        public void RepositoryTagIdException_InnerExceptionSet()
        {
            // Arrange
            var expectedException = new Exception();
            // Act
            var sut = new DockerImageNotFoundException(ExpectedId, ExpectedTag, expectedException);

            // Assert
            sut.InnerException.Should().Be(expectedException);
        }
    }
}
