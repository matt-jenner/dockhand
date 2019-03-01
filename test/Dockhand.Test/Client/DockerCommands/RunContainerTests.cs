using System;
using System.Collections.Generic;
using System.Text;
using Dockhand.Dtos;
using FluentAssertions;
using NUnit.Framework;

namespace Dockhand.Test.Client.DockerCommands
{
    [TestFixture]
    public class RunContainerTests
    {
        private const string ImageId = "atestimageid";

        [Test]
        public void WithoutOptions_StartsWithDefaultCommandParams()
        {
            // Arrange/Act
            var result = Dockhand.Client.DockerCommands.Image.RunContainer(ImageId, null);

            // Assert
            result.Should().Be($"docker run -d {ImageId}");
        }

        [Test]
        public void WithEmptyOptions()
        {
            //Arrange
            var options = new StartContainerOptions();

            // Act
            var result = Dockhand.Client.DockerCommands.Image.RunContainer(ImageId, options);

            // Assert
            result.Should().Be($"docker run -d {ImageId}");
        }

        [Test]
        public void WithOptionsMemoryLimit_AddsArgument()
        {
            //Arrange
            var options = new StartContainerOptions().WithMemoryLimit(1);

            // Act
            var result = Dockhand.Client.DockerCommands.Image.RunContainer(ImageId, options);

            // Assert
            result.Should().Be($"docker run -d --memory=\"1m\" {ImageId}");
        }

        [Test]
        public void WithOptionsCpuLimit_AddsArgument()
        {
            //Arrange
            var options = new StartContainerOptions().WithCpuLimit(0m);

            // Act
            var result = Dockhand.Client.DockerCommands.Image.RunContainer(ImageId, options);

            // Assert
            result.Should().Contain($"docker run -d --cpus=\"0\" {ImageId}");
        }

        [Test]
        public void WithOptionsMemAndCpuLimit_AddsArguments()
        {
            //Arrange
            var options = new StartContainerOptions()
                .WithCpuLimit(16.5m)
                .WithMemoryLimit(0);

            // Act
            var result = Dockhand.Client.DockerCommands.Image.RunContainer(ImageId, options);

            // Assert
            result.Should().Contain($"docker run -d --memory=\"0m\" --cpus=\"16.5\" {ImageId}");
        }

        [Test]
        [TestCase(null, null, null, null, "")]
        [TestCase(10, 2000, null, null, "-p 2000:10 ")]
        [TestCase(10, 2000, 30, 4000, "-p 2000:10 -p 4000:30 ")]
        public void WithOptionsPortMappings_AddsArgumentPerPort(int? internalPort1, int? externalPort1, int? internalPort2, int? externalPort2, string expectedPortArguments)
        {
            //Arrange
            var options = new StartContainerOptions();
            
            if (internalPort1.HasValue && externalPort1.HasValue)
            {
                options.ExposePort(internalPort1.Value, externalPort1.Value);
            }

            if (internalPort2.HasValue && externalPort2.HasValue)
            {
                options.ExposePort(internalPort2.Value, externalPort2.Value);
            }

            // Act
            var result = Dockhand.Client.DockerCommands.Image.RunContainer(ImageId, options);

            // Assert
            result.Should().Contain($"docker run -d {expectedPortArguments}{ImageId}");
        }
    }
}
