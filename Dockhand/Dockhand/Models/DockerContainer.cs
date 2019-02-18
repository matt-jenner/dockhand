using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dockhand.Client;
using Dockhand.Dtos;
using Dockhand.Exceptions;
using Dockhand.Interfaces;

namespace Dockhand.Models
{
    public class DockerContainer : DockerEntity
    {
        public DockerPortMapping[] PortMappings { get; }

        private readonly IDockerClient _client;
        private readonly IRunCommands _commandFactory;

        internal DockerContainer(IDockerClient client, IRunCommands commandFactory, string containerId,
            DockerPortMapping[] portMappings)
        {
            _client = client;
            _commandFactory = commandFactory;
            Id = containerId;
            PortMappings = portMappings;
        }

        public async Task KillAsync() => await EnsureExistsBefore(() => 
            KillContainerAsync(Id));

        public async Task RemoveAsync() => await EnsureExistsBefore(() => 
            RemoveContainerAsync(Id));

        public async Task<ContainerStatsObservation> MonitorStatsFor(TimeSpan period) => 
            await EnsureExistsBefore(() => GetContainerStats(Id, period));

        internal async Task KillContainerAsync(string containerId)
        {
            var cmd = DockerCommands.Container.Kill(containerId);
            var command = _commandFactory.RunCommand(cmd, _client.WorkingDirectory, new CancellationToken());

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
            var command = _commandFactory.RunCommand(cmd, _client.WorkingDirectory, new CancellationToken());

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
                var command = _commandFactory.RunCommand(cmd, _client.WorkingDirectory);

                await command.Task;

                output.Add(command.StandardOutput.ReadToEnd());
            }

            return new ContainerStatsObservation(output);
        }

        protected override async Task<bool> ExistsAction() => await _client.ContainerExistsAsync(Id);
        protected override void ErrorAction(DockerCommandException e) => throw new DockerContainerNotFoundException(Id, e);
        protected override void DeletedAction() => throw new DockerContainerDeletedException(Id);
    }
}
