using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dockhand.Client;
using Dockhand.Dtos;
using Dockhand.Exceptions;
using Dockhand.Interfaces;

namespace Dockhand.Models
{
    public class DockerImage : DockerEntity
    {
        public string Repository { get; }
        public string Tag { get; }

        private readonly IDockerClient _client;
        private readonly IRunCommands _commandFactory;
        
        internal DockerImage(IDockerClient client, IRunCommands commandFactory, DockerImageResult imageResult)
        {
            _client = client;
            _commandFactory = commandFactory;
            Id = imageResult.Id;
            Repository = imageResult.Repository;
            Tag = imageResult.Tag;
        }

        public async Task<DockerContainer> StartContainerAsync(Action<StartContainerOptions> configureOptions = null) => await EnsureExistsBefore(() => StartContainerAsync(Id, configureOptions));

        public async Task RemoveAsync() => await EnsureExistsBefore(() => RemoveImageAsync(Id));

        internal async Task<DockerContainer> StartContainerAsync(string imageId, Action<StartContainerOptions> configureOptions = null)
        {
            StartContainerOptions options = null;
            if (configureOptions != null)
            {
                options = new StartContainerOptions();
                configureOptions(options);
                options.Validate();
            }

            var cmd = DockerCommands.Image.RunContainer(imageId, options);
            var command = _commandFactory.RunCommand(cmd, _client.WorkingDirectory, new CancellationToken());

            await command.Task;

            var success = command.Result.Success;

            if (!success)
            {
                throw new DockerCommandException(cmd, command.GetOutputAndErrorLines().ToList());
            }

            var output = command.GetOutputAndErrorLines();
            var firstLine = output.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(firstLine) || firstLine.Length < 12)
            {
                throw new DockerCommandUnexpectedOutputException(cmd, command.GetOutputAndErrorLines().ToList());
            }

            var containerId = firstLine.Substring(0, 12);

            return new DockerContainer(_client, _commandFactory, containerId, options?.DockerPortMappings);
        }

        internal async Task RemoveImageAsync(string imageId)
        {
            var cmd = DockerCommands.Image.Remove(imageId);
            var command = _commandFactory.RunCommand(cmd, _client.WorkingDirectory, new CancellationToken());

            await command.Task;

            var success = command.Result.Success;

            if (!success)
            {
                throw new DockerCommandException(cmd, command.GetOutputAndErrorLines().ToList());
            }

            Deleted = true;
        }

        protected override async Task<bool> ExistsAction() => await _client.ImageExistsAsync(Id);
        protected override void ErrorAction(DockerCommandException e) => throw new DockerImageNotFoundException(Id, e);
        protected override void DeletedAction() => throw new DockerImageDeletedException(Id);
    }
}
