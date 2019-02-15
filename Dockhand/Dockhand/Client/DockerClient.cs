using System;
using System.IO;

namespace Dockhand.Client
{
    public partial class DockerClient
    {
        private readonly string _workingDirectory;

        private DockerClient(string workingDirectory)
        {
            if (!Directory.Exists(workingDirectory))
            {
                throw new ArgumentException($"The working directory specified does not exist ('{workingDirectory}').", nameof(workingDirectory));
            }

            _workingDirectory = workingDirectory;
        }

        public static DockerClient ForDirectory(string workingDirectory)
        {
            return new DockerClient(workingDirectory);
        }



        

        
    }
}
