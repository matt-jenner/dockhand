using System;

namespace Dockhand.Exceptions
{
    class DockerImageDeletedException : Exception
    {
        public DockerImageDeletedException(string id) : base($"The docker image with id '{id}' has been deleted.")
        {
        }
    }
}
