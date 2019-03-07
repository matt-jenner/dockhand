using System;
using System.Collections.Generic;
using System.Text;
using Dockhand.Models;
using Dockhand.Test.Builders;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Dockhand.Test.Models
{
    [TestFixture]
    public class DockerContainerStatTests
    {
        public static IEnumerable<decimal> NegativeReadingTestCases => new List<decimal> {-1, Decimal.MinValue};
        public static IEnumerable<decimal> PositiveReadingTestCases => new List<decimal> { 0, 1, Decimal.MaxValue };

        [Test]
        [TestCaseSource(nameof(NegativeReadingTestCases))]
        public void ThrowsIfCpuResultIsNegative(decimal cpuReading)
        {
            // Act/Assert
            var exception = Assert.Catch(() => new DockerContainerStat(cpuReading, 0));

            //Assert
            exception.Should().BeOfType<ArgumentException>();
        }

        [Test]
        [TestCaseSource(nameof(NegativeReadingTestCases))]
        public void ThrowsIfMemoryResultIsNegative(decimal memoryReading)
        {
            // Act/Assert
            var exception = Assert.Catch(() => new DockerContainerStat(0, memoryReading));

            //Assert
            exception.Should().BeOfType<ArgumentException>();
        }

        [Test]
        [TestCaseSource(nameof(PositiveReadingTestCases))]
        public void ConstructorSetsCpuProperty(decimal cpuReading)
        {
            // Act/Assert
            var result = new DockerContainerStat(cpuReading, 0);

            //Assert
            result.Cpu.Should().Be(cpuReading);
        }

        [Test]
        [TestCaseSource(nameof(PositiveReadingTestCases))]
        public void ConstructorSetsMemoryProperty(decimal memoryReading)
        {
            // Act/Assert
            var result = new DockerContainerStat(0, memoryReading);

            //Assert
            result.Memory.Should().Be(memoryReading);
        }
    }
}
