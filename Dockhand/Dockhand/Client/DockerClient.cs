using System;
using System.IO;
using Dockhand.Interfaces;
using Dockhand.Utils;

namespace Dockhand.Client
{
    public partial class DockerClient
    {
        private readonly string _workingDirectory;
        private readonly IRunCommands _commandFactory;

        internal DockerClient(string workingDirectory, IRunCommands commandFactory)
        {
            if (!Directory.Exists(workingDirectory))
            {
                throw new ArgumentException($"The working directory specified does not exist ('{workingDirectory}').", nameof(workingDirectory));
            }

            _commandFactory = commandFactory;
            _workingDirectory = workingDirectory;
        }

        public static DockerClient ForDirectory(string workingDirectory)
        {
            return new DockerClient(workingDirectory, new CommandFactory());
        }
    }
}
