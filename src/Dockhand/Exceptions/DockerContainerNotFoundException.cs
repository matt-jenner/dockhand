using System;

namespace Dockhand.Exceptions
{
    class DockerContainerNotFoundException : Exception
    {

        public DockerContainerNotFoundException(string id, Exception e) : base($"The docker container with id '{id}' was not found when requesting container information.", e)
        {
        }
    }
}
