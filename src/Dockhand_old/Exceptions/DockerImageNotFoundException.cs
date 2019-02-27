using System;

namespace Dockhand.Exceptions
{
    class DockerImageNotFoundException : Exception
    {
        public DockerImageNotFoundException(string repository, string tag) : base($"The docker image for '{repository}:{tag}' was not found when requesting image information.")
        {
        }

        public DockerImageNotFoundException(string repository, string tag, Exception e) : base($"The docker image for '{repository}:{tag}' was not found when requesting image information.", e)
        {
        }

        public DockerImageNotFoundException(string repository, string tag, string id, Exception e) : base($"The docker image with id '{id}' with label '{repository}:{tag}' was not found when requesting image information.", e)
        {
        }

        public DockerImageNotFoundException(string id, Exception e) : base($"The docker image with id '{id}' was not found when requesting image information.", e)
        {
        }
    }
}
