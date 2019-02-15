using System;
using System.Collections.Generic;
using System.Linq;

namespace Dockhand.Exceptions
{
    class DockerCommandException : Exception
    {
        public DockerCommandException(string command, IEnumerable<string> output) : base($"The docker command did not exit with the expected result.{Environment.NewLine}" +
                                                                                         $"Command: {command}{Environment.NewLine}" +
                                                                                         $"Process Output:{Environment.NewLine}{string.Join(Environment.NewLine, output.ToArray())}")
        {
        }
    }
}
