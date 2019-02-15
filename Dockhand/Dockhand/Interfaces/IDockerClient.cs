using System.Collections.Generic;
using System.Threading.Tasks;
using Dockhand.Models;

namespace Dockhand.Interfaces
{
    public interface IDockerClient
    {
        Task<DockerImage> BuildImageAsync(string dockerfile, string target, string repository, string tag);
        Task<DockerImage> GetImageAsync(string repository, string tag);
        Task<IEnumerable<DockerImage>> GetImagesAsync();
        Task<bool> ImageExistsAsync(string id);
        Task<bool> ContainerExistsAsync(string id);
    }
}