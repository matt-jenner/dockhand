using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dockhand.Models;
using Newtonsoft.Json;

namespace Dockhand
{
    public class DockerContainerManager
    {
        private readonly string _workingDirectory;

        public DockerContainerManager(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        public async Task BuildImageAsync(string dockerfile, string target, string repository, string tag)
        {
            var cmd = DockerCommands.Image.Build(dockerfile, target, repository, tag);
            var command = cmd.RunCommand(_workingDirectory, new CancellationToken());

            await command.Task;

            var output = command.GetOutputAndErrorLines().ToList();
            var success = command.Result.Success;

            if (!success)
            {
                throw new DockerCommandException(cmd, output);
            }
        }

        public async Task<IEnumerable<DockerImageResult>> GetImagesAsync()
        {
            var cmd = DockerCommands.Image.List();
            var command = cmd.RunCommand(_workingDirectory, new CancellationToken());

            await command.Task;

            var output = command.GetOutputAndErrorLines().ToList();
            var success = command.Result.Success;

            if (!success)
            {
                throw new DockerCommandException(cmd, output);
            }

            var response = output.Select(JsonConvert.DeserializeObject<DockerImageResult>).ToList();
            return response;
        }

        public async Task<string> StartContainerAsync(string imageId)
        {
            var cmd = DockerCommands.Image.RunContainer(imageId, new[] {new DockerPortMapping(80, 5000)});
            var command = cmd.RunCommand(_workingDirectory, new CancellationToken());

            await command.Task;

            var success = command.Result.Success;

            if (!success)
            {
                throw new DockerCommandException(cmd, command.GetOutputAndErrorLines().ToList());
            }

            return command.Result.StandardOutput.Substring(0,12);
        }

        public async Task KillContainerAsync(string containerId)
        {
            var cmd = DockerCommands.Container.Kill(containerId);
            var command = cmd.RunCommand(_workingDirectory, new CancellationToken());

            await command.Task;

            var success = command.Result.Success;

            if (!success)
            {
                throw new DockerCommandException(cmd, command.GetOutputAndErrorLines().ToList());
            }
        }

        public async Task RemoveContainer(string containerId)
        {
            var cmd = DockerCommands.Container.Remove(containerId);
            var command = cmd.RunCommand(_workingDirectory, new CancellationToken());

            await command.Task;

            var success = command.Result.Success;

            if (!success)
            {
                throw new DockerCommandException(cmd, command.GetOutputAndErrorLines().ToList());
            }
        }

        public async Task RemoveImage(string imageId)
        {
            var cmd = DockerCommands.Image.Remove(imageId);
            var command = cmd.RunCommand(_workingDirectory, new CancellationToken());

            await command.Task;

            var success = command.Result.Success;

            if (!success)
            {
                throw new DockerCommandException(cmd, command.GetOutputAndErrorLines().ToList());
            }
        }

        public ContainerStatsObservation GetContainerStats(string containerId, TimeSpan monitorPeriod)
        {
            var cts = new CancellationTokenSource(monitorPeriod);
            var cmd = DockerCommands.Container.GetStats(containerId);

            var output = new List<string>();
            while (!cts.Token.IsCancellationRequested)
            {
                var command = cmd.RunCommand(_workingDirectory);

                command.Wait();
                output.Add(command.StandardOutput.ReadToEnd());
            }

            return new ContainerStatsObservation(output);
        }
    }
}
