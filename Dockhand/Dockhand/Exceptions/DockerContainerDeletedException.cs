using System;

namespace Dockhand.Exceptions
{
    class DockerContainerDeletedException : Exception
    {
        public DockerContainerDeletedException(string id) : base($"The docker container with id '{id}' has been deleted.")
        {
        }
    }
}
