using System;
using System.Collections.Generic;
using System.Linq;
using Dockhand.Models;
using Dockhand.Test.Builders;
using FluentAssertions;
using NUnit.Framework;

namespace Dockhand.Test.Models
{
    [TestFixture]
    public class ContainerStatsObservationTests
    {
        public static IEnumerable<decimal[]> AverageStatResultTestCases =>
            new List<decimal[]>
            {
                new [] { 0.1m, 0.1m, 0.2m },
                new [] { 1m, 3m },
                new [] { 0m, 0m },
            };

        public static IEnumerable<decimal[]> MaxStatResultTestCases =>
            new List<decimal[]>
            {
                new [] { 0.1m, 0.1m, 0.2m },
                new [] { 1m, 3m },
                new [] { 0m, 0m },
            };

        public static IEnumerable<decimal[]> NegativeStatResultTestCases =>
            new List<decimal[]>
            {
                new [] { -1m, 0m },
                new [] { 0m, -1m, 1m },
                new [] { 1m, 1m, -1m },
            };

        [Test]
        [TestCaseSource(nameof(AverageStatResultTestCases))]
        public void ItCalculatesAverageCpu(decimal[] cpuValues)
        {
            // Arrange
            var total = cpuValues.Sum();
            var expectedAverage = total > 0 ? total / cpuValues.Count() : 0m;


            var observations = MotherFor
                .ObservationsJsonBuilder
                .WithCpuReadings(cpuValues)
                .Build();

            var sut = new ContainerStatsObservation(observations);

            // Act
            var result = sut.AverageCpu();
            
            //Assert
            result.Should().Be(expectedAverage);
        }

        [Test]
        [TestCaseSource(nameof(MaxStatResultTestCases))]
        public void ItCalculatesMaxCpu(decimal[] cpuValues)
        {
            // Arrange
            var expectedMax = cpuValues.Max();

            var observations = MotherFor
                .ObservationsJsonBuilder
                .WithCpuReadings(cpuValues)
                .Build();

            var sut = new ContainerStatsObservation(observations);

            // Act
            var result = sut.MaxCpu();

            //Assert
            result.Should().Be(expectedMax);
        }

        [Test]
        [TestCaseSource(nameof(NegativeStatResultTestCases))]
        public void ThrowsIfAnyCpuResultsAreNegative(params decimal[] cpuReadings)
        {
            // Assert
            var observations = MotherFor.ObservationsJsonBuilder.WithCpuReadings(cpuReadings).Build();

            // Act/Assert
            var exception = Assert.Catch(() => new ContainerStatsObservation(observations));

            //Assert
            exception.Should().BeOfType<ArgumentException>();
        }

        [Test]
        [TestCaseSource(nameof(AverageStatResultTestCases))]
        public void ItCalculatesAverageMem(decimal[] memValues)
        {
            // Arrange
            var total = memValues.Sum();
            var expectedAverage = total > 0 ? total / memValues.Count() : 0m;


            var observations = MotherFor
                .ObservationsJsonBuilder
                .WithMemReadings(memValues)
                .Build();

            var sut = new ContainerStatsObservation(observations);

            // Act
            var result = sut.AverageMem();

            //Assert
            result.Should().Be(expectedAverage);
        }

        [Test]
        [TestCaseSource(nameof(MaxStatResultTestCases))]
        public void ItCalculatesMaxMem(decimal[] memValues)
        {
            // Arrange
            var expectedMax = memValues.Max();

            var observations = MotherFor
                .ObservationsJsonBuilder
                .WithMemReadings(memValues)
                .Build();

            var sut = new ContainerStatsObservation(observations);

            // Act
            var result = sut.MaxMem();

            //Assert
            result.Should().Be(expectedMax);
        }

        [Test]
        [TestCaseSource(nameof(NegativeStatResultTestCases))]
        public void ThrowsIfAnyMemResultsAreNegative(params decimal[] memReadings)
        {
            // Assert
            var observations = MotherFor.ObservationsJsonBuilder.WithMemReadings(memReadings).Build();

            // Act/Assert
            var exception = Assert.Catch(() => new ContainerStatsObservation(observations));

            //Assert
            exception.Should().BeOfType<ArgumentException>();
        }

        [Test]
        public void ThrowsIfNoResults()
        {
            // Act/Assert
            var exception = Assert.Catch(() => new ContainerStatsObservation(new string[0]));

            //Assert
            exception.Should().BeOfType<ArgumentException>();
        }
    }
}
