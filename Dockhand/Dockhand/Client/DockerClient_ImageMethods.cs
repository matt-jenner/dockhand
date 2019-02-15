using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dockhand.Dtos;
using Dockhand.Exceptions;
using Dockhand.Models;
using Dockhand.Utils;
using Newtonsoft.Json;

namespace Dockhand.Client
{
    public partial class DockerClient
    {
        public async Task<DockerImage> BuildImageAsync(string dockerfile, string target, string repository, string tag)
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

            return await GetImageAsync(repository, tag);
        }

        public async Task<DockerImage> GetImageAsync(string repository, string tag)
        {
            try
            {
                return (await GetImagesAsync()).First(i => i.Repository == repository && i.Tag == tag);
            }
            catch (DockerCommandException e)
            {
                throw new DockerImageNotFoundException(repository, tag, e);
            }
        }

        public async Task<IEnumerable<DockerImage>> GetImagesAsync()
        {
            var cmd = DockerCommands.Image.List();
            var command = cmd.RunCommand(_workingDirectory);

            await command.Task;

            var output = command.GetOutputAndErrorLines().ToList();
            var success = command.Result.Success;

            if (!success)
            {
                throw new DockerCommandException(cmd, output);
            }

            var response = output.Select(o => new DockerImage(this, JsonConvert.DeserializeObject<DockerImageResult>(o))).ToList();
            return response;
        }

        public async Task<bool> ImageExistsAsync(string id)
        {
            try
            {
                var cmd = DockerCommands.Image.ListIds;
                var command = cmd.RunCommand(_workingDirectory);

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
                throw new DockerImageNotFoundException(id, e);
            }
        }

        internal async Task RemoveImageAsync(string imageId)
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
    }
}
