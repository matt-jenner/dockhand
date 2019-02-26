using System;
using Dockhand.Utils;

namespace Dockhand.Models
{
    public class DockerPortMapping
    {
        public int InternalPort { get; }
        public int ExternalPort { get; }

        public DockerPortMapping(int internalPort, int externalPort)
        {
            CheckInternalPortIsValid(internalPort);
            CheckExternalPortIsValid(externalPort);

            InternalPort = internalPort;
            ExternalPort = externalPort;
        }

        public override string ToString()
        {
            return $"{ExternalPort}:{InternalPort}";
        }

        private void CheckInternalPortIsValid(int value)
        {
            HandleInvalidInput("internal", value);
        }

        private void CheckExternalPortIsValid(int value)
        {
            HandleInvalidInput("external", value);
        }

        private void HandleInvalidInput(string portName, int value)
        {
            if (value.IsValidAsANetworkPort()) { return; }
            throw new ArgumentException($"The {portName} network port specified ({value}) is not valid, the acceptable range is 1-65535");
        }
    }
}
