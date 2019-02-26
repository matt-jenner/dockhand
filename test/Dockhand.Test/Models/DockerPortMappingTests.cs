using System;
using Dockhand.Models;
using ExpectedObjects;
using FluentAssertions;
using NUnit.Framework;

namespace Dockhand.Test.Models
{
    [TestFixture]
    public class DockerPortMappingTests
    {
        private const int MaxValidPort = 65535;
        private const int MinValidPort = 1;

        [Test]
        public void CorrectlyFormatsPortMappings()
        {
            // Arrange
            const int expectedInternalPort = MinValidPort;
            const int expectedExternalPort = MaxValidPort;
            var sut = new DockerPortMapping(MinValidPort, MaxValidPort);

            // Act
            var result = sut.ToString();

            // Assert
            result.Should().Be($"{expectedExternalPort}:{expectedInternalPort}");
        }

        [Test]
        [TestCase(-1, -1)]
        [TestCase(-1, 1)]
        [TestCase(1, -1)]
        [TestCase(MaxValidPort + 1, MinValidPort)]
        [TestCase(MinValidPort, MaxValidPort + 1)]
        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        public void ItRejectsInvalidPorts(int internalPort, int externalPort)
        {
            // Arrange/Act
            var exception = Assert.Catch(() => new DockerPortMapping(internalPort, externalPort));

            // Assert
            exception.Should().BeOfType<ArgumentException>();
        }

        [Test]
        [TestCase(MinValidPort, MaxValidPort)]
        [TestCase(MaxValidPort, MinValidPort)]
        [TestCase(MinValidPort, MinValidPort)]
        [TestCase(MaxValidPort, MaxValidPort)]
        public void ItAllowsValidPorts(int internalPort, int externalPort)
        {
            // Arrange/Act/Assert
            Assert.DoesNotThrow(() => new DockerPortMapping(internalPort, externalPort));
        }

        [Test]
        public void ItSetsPortsProperly()
        {
            // Arrange/Act
            var sut = new DockerPortMapping(1, 2);

            // Assert
            var expectedResult = new {ExternalPort = 2, InternalPort = 1}.ToExpectedObject();
            expectedResult.ShouldMatch(sut);
        }
    }
}
