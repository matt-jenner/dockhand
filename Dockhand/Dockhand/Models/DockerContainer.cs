using System;
using System.Threading.Tasks;
using Dockhand.Client;
using Dockhand.Dtos;
using Dockhand.Exceptions;

namespace Dockhand.Models
{
    public class DockerContainer
    {
        public string Id { get; }
        public DockerPortMapping[] PortMappings { get; }
        public bool Deleted { get; private set; }

        private readonly DockerClient _client;

        internal DockerContainer(DockerClient client, string containerId, DockerPortMapping[] portMappings)
        {
            _client = client;
            Id = containerId;
            PortMappings = portMappings;
            Deleted = false;
        }

        public async Task KillAsync() => await EnsureExistsBefore(() => _client.KillContainerAsync(Id));

        public async Task RemoveAsync() => await EnsureExistsBefore(() => _client.RemoveContainerAsync(Id));

        public async Task<ContainerStatsObservation> MonitorStatsFor(TimeSpan period) =>
            await EnsureExistsBefore<ContainerStatsObservation>(() => _client.GetContainerStats(Id, period));

        private async Task<T> EnsureExistsBefore<T>(Func<Task<T>> commandFunc)
        {
            if (Deleted)
            {
                throw new DockerContainerDeletedException(Id);
            }

            try
            {
                return await commandFunc();
            }
            catch (DockerCommandException e)
            {
                // Check if the container exists still
                Deleted = !(await _client.ContainerExistsAsync(Id));

                if (Deleted)
                {
                    throw new DockerContainerNotFoundException(Id, e);
                }

                // Otherwise throw the DockerCommandException
                throw;
            }
        }

        private async Task EnsureExistsBefore(Func<Task> commandFunc)
        {
            if (Deleted)
            {
                throw new DockerContainerDeletedException(Id);
            }

            try
            {
                await commandFunc();
            }
            catch (DockerCommandException e)
            {
                // Check if the image exists still
                Deleted = !(await _client.ContainerExistsAsync(Id));

                if (Deleted)
                {
                    throw new DockerContainerNotFoundException(Id, e);
                }

                // Otherwise throw the DockerCommandException
                throw;
            }
        }
    }
}
