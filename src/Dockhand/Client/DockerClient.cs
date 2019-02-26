using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dockhand.Dtos;
using Dockhand.Exceptions;
using Dockhand.Interfaces;
using Dockhand.Models;
using Dockhand.Utils;
using Newtonsoft.Json;

namespace Dockhand.Client
{
    public class DockerClient : IDockerClient
    {
        public string WorkingDirectory { get; }
        private readonly IRunCommands _commandFactory;

        internal DockerClient(string workingDirectory, IRunCommands commandFactory)
        {
            if (!Directory.Exists(workingDirectory))
            {
                throw new ArgumentException($"The working directory specified does not exist ('{workingDirectory}').", nameof(workingDirectory));
            }

            _commandFactory = commandFactory;
            WorkingDirectory = workingDirectory;
        }

        public static DockerClient ForDirectory(string workingDirectory)
        {
            return new DockerClient(workingDirectory, new CommandFactory());
        }

        public async Task<bool> ContainerExistsAsync(string id)
        {
            var cmd = DockerCommands.Container.ListIds;
            var command = _commandFactory.RunCommand(cmd, WorkingDirectory);

            await command.Task;

            var output = command.GetOutputAndErrorLines().ToList();

            if (!command.Result.Success)
            {
                throw new DockerCommandException(cmd, output);
            }

            return output.Any(o => o.Trim() == id);
        }

        public async Task<DockerImage> BuildImageAsync(string dockerfile, string target, string repository, string tag)
        {
            var cmd = DockerCommands.Image.Build(dockerfile, target, repository, tag);
            var command = _commandFactory.RunCommand(cmd, WorkingDirectory, new CancellationToken());

            await command.Task;
            
            var success = command.Result.Success;

            if (!success)
            {
                var output = command.GetOutputAndErrorLines().ToList();
                throw new DockerCommandException(cmd, output);
            }

            return await GetImageAsync(repository, tag);
        }

        public async Task<DockerImage> GetImageAsync(string repository, string tag)
        {
            try
            {
                return (await GetImagesAsync()).First(i => i.Repository == repository && i.Tag == tag);
            }
            catch (InvalidOperationException e)
            {
                throw new DockerImageNotFoundException(repository, tag, e);
            }
        }

        public async Task<IEnumerable<DockerImage>> GetImagesAsync()
        {
            var cmd = DockerCommands.Image.List();
            var command = _commandFactory.RunCommand(cmd, WorkingDirectory);

            await command.Task;

            var output = command.GetOutputAndErrorLines().ToList();
            var success = command.Result.Success;

            if (!success)
            {
                throw new DockerCommandException(cmd, output);
            }

            var response = output.Select(o => new DockerImage(this, _commandFactory, JsonConvert.DeserializeObject<DockerImageResult>(o))).ToList();
            return response;
        }

        public async Task<bool> ImageExistsAsync(string id)
        {
            var cmd = DockerCommands.Image.ListIds;
            var command = _commandFactory.RunCommand(cmd, WorkingDirectory);

            await command.Task;

            var output = command.GetOutputAndErrorLines().ToList();

            if (!command.Result.Success)
            {
                throw new DockerCommandException(cmd, output);
            }

            return output.Any(o => o.Trim() == id);
        }
    }
}
