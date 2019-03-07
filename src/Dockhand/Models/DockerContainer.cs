using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dockhand.Client;
using Dockhand.Dtos;
using Dockhand.Exceptions;
using Dockhand.Interfaces;
using Newtonsoft.Json;

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

        public async Task<DockerContainerStat> GetCurrentStat() =>
            await EnsureExistsBefore(() => GetContainerStat(Id));

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

            var output = new List<DockerContainerStat>();
            
            do
            {
                var stat = await GetContainerStat(containerId);
                output.Add(stat);
            } while (!cts.Token.IsCancellationRequested);

            return new ContainerStatsObservation(output);
        }

        internal async Task<DockerContainerStat> GetContainerStat(string containerId)
        {
            var cmd = DockerCommands.Container.GetStats(containerId);
            var command = _commandFactory.RunCommand(cmd, _client.WorkingDirectory);

            await command.Task;

            if (!command.Result.Success)
            {
                throw new DockerCommandException(cmd, command.GetOutputAndErrorLines());
            }

            var output = command.StandardOutput.ReadLine();

            var containerStat = JsonConvert.DeserializeObject<ContainerStatDto>(output);

            return new DockerContainerStat(containerStat.Cpu, containerStat.Mem);
        }

        protected override async Task<bool> ExistsAction() => await _client.ContainerExistsAsync(Id);
        protected override void ErrorAction(DockerCommandException e) => throw new DockerContainerNotFoundException(Id, e);
        protected override void DeletedAction() => throw new DockerContainerDeletedException(Id);
    }
}
