﻿using System;
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

        [Test]
        [TestCaseSource(nameof(AverageStatResultTestCases))]
        public void ItCalculatesAverageCpu(decimal[] cpuValues)
        {
            // Arrange
            var total = cpuValues.Sum();
            var expectedAverage = total > 0 ? total / cpuValues.Count() : 0m;


            var observations = MotherFor
                .ObservationsBuilder
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
                .ObservationsBuilder
                .WithCpuReadings(cpuValues)
                .Build();

            var sut = new ContainerStatsObservation(observations);

            // Act
            var result = sut.MaxCpu();

            //Assert
            result.Should().Be(expectedMax);
        }

        [Test]
        [TestCaseSource(nameof(AverageStatResultTestCases))]
        public void ItCalculatesAverageMem(decimal[] memValues)
        {
            // Arrange
            var total = memValues.Sum();
            var expectedAverage = total > 0 ? total / memValues.Count() : 0m;


            var observations = MotherFor
                .ObservationsBuilder
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
                .ObservationsBuilder
                .WithMemReadings(memValues)
                .Build();

            var sut = new ContainerStatsObservation(observations);

            // Act
            var result = sut.MaxMem();

            //Assert
            result.Should().Be(expectedMax);
        }

        [Test]
        public void ThrowsIfNoResults()
        {
            // Act/Assert
            var exception = Assert.Catch(() => new ContainerStatsObservation(new DockerContainerStat[0]));

            //Assert
            exception.Should().BeOfType<ArgumentException>();
        }
    }
}
