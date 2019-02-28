using System.Collections.Generic;
using System.Linq;
using Dockhand.Models;

namespace Dockhand.Client
{
    internal static class DockerCommands
    {
        internal static class Image
        {
            internal static string Build(string dockerfile, string target, string repository, string tag)
            {
                return
                    $"docker build . -f {dockerfile} -t {repository}:{tag}{(string.IsNullOrWhiteSpace(target) ? "" : $" --target {target}")}";
            }

            internal static string List()
            {
                const string formatTemplate = @"{""repository"":""{{.Repository}}"",""tag"":""{{.Tag}}"",""id"":""{{.ID}}""}";
                return $"docker images --format {formatTemplate}";
            }

            internal static string ListIds = "docker image ls -q";

            internal static string RunContainer(string imageId, IEnumerable<DockerPortMapping> portMappings, int? memoryLimitMb = null)
            {
                var command = "docker run -d ";
                var portStringArguments = portMappings.Select(p => $"-p {p.ToString()}").ToArray();
                if (portStringArguments.Length > 0)
                {
                    command = command + $" {string.Join(" ", portStringArguments)}";
                }

                if (memoryLimitMb.HasValue)
                {
                    command = command + $" --memory {memoryLimitMb.Value}m";
                }
                return command + $" {imageId}";
            }

            internal static string Remove(string imageId) => $"docker rmi {imageId}";
        }

        internal static class Container
        {
            internal static string GetStats(string containerId)
            {
                const string formatTemplate = @"{""cpu"":""{{.CPUPerc}}"",""mem"":""{{.MemPerc}}""}";
                return
                    $"docker stats --no-stream --format {formatTemplate} {containerId}";
            }

            internal static string Kill(string containerId) => $"docker container kill {containerId}";
            internal static string Remove(string containerId) => $"docker container rm {containerId}";
            internal static string Prune = "docker container prune -f";
            internal static string ListIds = "docker ps -q";
        }
    }
}
