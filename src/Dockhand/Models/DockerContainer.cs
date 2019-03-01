using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dockhand.Client;
using Dockhand.Exceptions;
using Dockhand.Interfaces;

namespace Dockhand.Models
{
    public class DockerContainer : DockerEntity
    {
        public IEnumerable<DockerPortMapping> PortMappings { get; }

        private readonly IDockerClient _client;
        private readonly IRunCommands _commandFactory;

        internal DockerContainer(IDockerClient client, IRunCommands commandFactory, string containerId,
            IEnumerable<DockerPortMapping> portMappings)
        {
            _client = client;
            _commandFactory = commandFactory;
            Id = containerId;
            PortMappings = portMappings ?? new DockerPortMapping[0];
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

            Deleted = true;
        }

        internal async Task<ContainerStatsObservation> GetContainerStats(string containerId, TimeSpan monitorPeriod)
        {
            var cts = new CancellationTokenSource(monitorPeriod);
            var cmd = DockerCommands.Container.GetStats(containerId);

            var output = new List<string>();
            
            do
            {
                var command = _commandFactory.RunCommand(cmd, _client.WorkingDirectory);

                await command.Task;

                if (!command.Result.Success)
                {
                    throw new DockerCommandException(cmd, command.GetOutputAndErrorLines());
                }
                output.Add(command.StandardOutput.ReadLine());
            } while (!cts.Token.IsCancellationRequested);

            return new ContainerStatsObservation(output);
        }

        protected override async Task<bool> ExistsAction() => await _client.ContainerExistsAsync(Id);
        protected override void ErrorAction(DockerCommandException e) => throw new DockerContainerNotFoundException(Id, e);
        protected override void DeletedAction() => throw new DockerContainerDeletedException(Id);
    }
}
