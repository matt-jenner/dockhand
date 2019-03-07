using System;
using System.Collections.Generic;
using System.Linq;
using Dockhand.Dtos;
using Dockhand.Models;
using FluentAssertions;
using NUnit.Framework;

namespace Dockhand.Test.Models
{
    [TestFixture]
    public class StartContainerOptionsTests
    {
        [Test]
        public void ConstructorDefaultsSet_Cpu()
        {
            // Arrange/Act
            var sut = new StartContainerOptions();

            // Assert
            sut.CpuLimit.Should().Be(null);
        }

        [Test]
        public void ConstructorDefaultsSet_Memory()
        {
            // Arrange/Act
            var sut = new StartContainerOptions();

            // Assert
            sut.MemoryLimitMb.Should().Be(null);
        }

        [Test]
        public void ConstructorDefaultsSet_PortMappings()
        {
            // Arrange/Act
            var sut = new StartContainerOptions();

            // Assert
            sut.DockerPortMappings.Should().BeEmpty();
        }

        [Test]
        [TestCase(int.MinValue)]
        [TestCase(int.MaxValue)]
        public void WithMemoryLimitSetsMemoryLimitMb(int memoryLimit)
        {
            // Arrange
            var sut = new StartContainerOptions();


            // Act
            sut.WithMemoryLimit(memoryLimit);

            // Assert
            sut.MemoryLimitMb.Should().Be(memoryLimit);
        }

        [Test]
        [TestCase(int.MinValue)]
        [TestCase(int.MaxValue)]
        public void WithCpuLimitSetsCpuLimit(decimal cpuLimit)
        {
            // Arrange
            var sut = new StartContainerOptions();

            // Act
            sut.WithCpuLimit(cpuLimit);

            // Assert
            sut.CpuLimit.Should().Be(cpuLimit);
        }

        [Test]
        [TestCase(80, 3000, 90, 3001)]
        [TestCase(80, 3000, null, null)]
        [TestCase(null, null, null, null)]
        public void ExposePortsAddsPorts(int? internalPort1, int? externalPort1, int? internalPort2, int? externalPort2)
        {
            // Arrange
            var sut = new StartContainerOptions();
            var portMappings = new List<DockerPortMapping>
            {
                GetPortMapping(internalPort1, externalPort1),
                GetPortMapping(internalPort2, externalPort2)
            }.Where(p => p != null);
            // Act
            sut.ExposePorts(portMappings);

            // Assert
            sut.DockerPortMappings.Should().BeEquivalentTo(portMappings);
        }

        [Test]
        [TestCase(80, 3000, 90, 3001)]
        [TestCase(80, 3000, null, null)]
        public void ExposePortsAddsPortsToExisting(int? internalPort1, int? externalPort1, int? internalPort2, int? externalPort2)
        {
            // Arrange
            var sut = new StartContainerOptions();
            sut.ExposePort(42, 42);

            var portMappings = new List<DockerPortMapping>
            {
                GetPortMapping(internalPort1, externalPort1),
                GetPortMapping(internalPort2, externalPort2)
            }.Where(p => p != null);

            var expectedMappings = new List<DockerPortMapping> {new DockerPortMapping(42, 42)};
            expectedMappings.AddRange(portMappings);

            // Act
            sut.ExposePorts(portMappings);

            // Assert
            sut.DockerPortMappings.Should().BeEquivalentTo(expectedMappings);
        }

        [Test]
        public void ExposePortAddsPort()
        {
            // Arrange
            var internalPort = 443;
            var externalPort = 5003;
            var sut = new StartContainerOptions();

            var expectedMappings = new [] { GetPortMapping(internalPort, externalPort) };
            // Act
            sut.ExposePort(443, 5003);

            // Assert
            sut.DockerPortMappings.Should().BeEquivalentTo(expectedMappings);
        }

        [Test]
        public void ExposePortAddsPortToExisting()
        {
            // Arrange
            var internalPort = 43;
            var externalPort = 44;
            var sut = new StartContainerOptions();
            sut.ExposePorts(new [] { GetPortMapping(42, 42) });

            var expectedMappings = new [] { GetPortMapping(42, 42), GetPortMapping(internalPort, externalPort)  };

            // Act
            sut.ExposePort(internalPort, externalPort);

            // Assert
            sut.DockerPortMappings.Should().BeEquivalentTo(expectedMappings);
        }

        [Test]
        public void ValidateWhenCpuCountTooHigh()
        {
            // Arrange
            var sut = new StartContainerOptions();
            sut.WithCpuLimit(Environment.ProcessorCount + 1);

            // Act
            var exception = Assert.Catch(() => sut.Validate());

            // Assert
            exception.Should().BeOfType<ArgumentException>();
        }

        [Test]
        [TestCase(0)]
        [TestCase(int.MinValue)]
        public void ValidateWhenCpuCountTooLow(decimal cpuCount)
        {
            // Arrange
            var sut = new StartContainerOptions();
            sut.WithCpuLimit(cpuCount);

            // Act
            var exception = Assert.Catch(() => sut.Validate());

            // Assert
            exception.Should().BeOfType<ArgumentException>();
        }

        [Test]        
        public void ValidateWhenCpuCountValidAboveZero()
        {
            // Arrange
            var sut = new StartContainerOptions();
            sut.WithCpuLimit(0.1m);

            // Act/Assert
            Assert.DoesNotThrow(() => sut.Validate());
        }

        [Test]
        public void ValidateWhenCpuCountAtMaxAllowed()
        {
            // Arrange
            var sut = new StartContainerOptions();
            sut.WithCpuLimit(Environment.ProcessorCount);

            // Act/Assert
            Assert.DoesNotThrow(() => sut.Validate());
        }

        [Test]
        [TestCase(4)]
        [TestCase(int.MaxValue)]
        public void ValidateWhenMemoryLimitValid(int memLimit)
        {
            // Arrange
            var sut = new StartContainerOptions();
            sut.WithMemoryLimit(memLimit);

            // Act/Assert
            Assert.DoesNotThrow(() => sut.Validate());
        }

        private DockerPortMapping GetPortMapping(int? internalPort, int? externalPort) =>
            (internalPort.HasValue && externalPort.HasValue)
                ? new DockerPortMapping(internalPort.Value, externalPort.Value)
                : null;
    }
}
