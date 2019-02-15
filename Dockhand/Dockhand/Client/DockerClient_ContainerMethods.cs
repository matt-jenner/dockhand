using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dockhand.Dtos;
using Dockhand.Exceptions;
using Dockhand.Interfaces;
using Dockhand.Models;
using Dockhand.Utils;

namespace Dockhand.Client
{
    public partial class DockerClient : IDockerClient
    {
        public async Task<bool> ContainerExistsAsync(string id)
        {
            try
            {
                var cmd = DockerCommands.Container.ListIds;
                var command = _commandFactory.RunCommand(cmd, _workingDirectory);

                await command.Task;

                var output = command.GetOutputAndErrorLines().ToList();

                if (!command.Result.Success)
                {
                    throw new DockerCommandException(cmd, output);
                }

                return output.Any(o => o.Trim() == id);
            }
            catch (DockerCommandException e)
            {
                throw new DockerContainerNotFoundException(id, e);
            }
        }

        internal async Task<DockerContainer> StartContainerAsync(string imageId, DockerPortMapping[] portMappings)
        {
            var cmd = DockerCommands.Image.RunContainer(imageId, portMappings);
            var command = _commandFactory.RunCommand(cmd, _workingDirectory, new CancellationToken());

            await command.Task;

            var success = command.Result.Success;

            if (!success)
            {
                throw new DockerCommandException(cmd, command.GetOutputAndErrorLines().ToList());
            }

            var containerId = command.Result.StandardOutput.Substring(0,12);

            return new DockerContainer(this, containerId, portMappings);
        }

        internal async Task KillContainerAsync(string containerId)
        {
            var cmd = DockerCommands.Container.Kill(containerId);
            var command = _commandFactory.RunCommand(cmd, _workingDirectory, new CancellationToken());

            await command.Task;

            var success = command.Result.Success;

            if (!success)
            {
                throw new DockerCommandException(cmd, command.GetOutputAndErrorLines().ToList());
            }
        }

        internal async Task RemoveContainerAsync(string containerId)
        {
            var cmd = DockerCommands.Container.Remove(containerId);
            var command = _commandFactory.RunCommand(cmd, _workingDirectory, new CancellationToken());

            await command.Task;

            var success = command.Result.Success;

            if (!success)
            {
                throw new DockerCommandException(cmd, command.GetOutputAndErrorLines().ToList());
            }
        }

        internal async Task<ContainerStatsObservation> GetContainerStats(string containerId, TimeSpan monitorPeriod)
        {
            var cts = new CancellationTokenSource(monitorPeriod);
            var cmd = DockerCommands.Container.GetStats(containerId);

            var output = new List<string>();
            while (!cts.Token.IsCancellationRequested)
            {
                var command = _commandFactory.RunCommand(cmd, _workingDirectory);

                await command.Task;

                output.Add(command.StandardOutput.ReadToEnd());
            }

            return new ContainerStatsObservation(output);
        }
    }
}
