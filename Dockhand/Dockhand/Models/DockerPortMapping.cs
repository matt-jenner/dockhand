using System;
using System.Collections.Generic;
using System.Text;

namespace Dockhand.Models
{
    class DockerPortMapping
    {
        public int InternalPort { get; }
        public int ExternalPort { get; }

        public DockerPortMapping(int internalPort, int externalPort)
        {
            InternalPort = internalPort;
            ExternalPort = externalPort;
        }

        public override string ToString()
        {
            return $"{ExternalPort}:{InternalPort}";
        }
    }
}
