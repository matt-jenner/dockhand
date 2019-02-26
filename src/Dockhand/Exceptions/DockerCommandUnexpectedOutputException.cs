using System;
using System.Collections.Generic;
using System.Linq;

namespace Dockhand.Exceptions
{
    public class DockerCommandUnexpectedOutputException : Exception
    {
        public DockerCommandUnexpectedOutputException(string command, IEnumerable<string> output) : base($"The docker command exited successfully but did not exit with the expected result.{Environment.NewLine}" +
                                                                                         $"Command: {command}{Environment.NewLine}" +
                                                                                         $"Process Output:{Environment.NewLine}{string.Join(Environment.NewLine, output.ToArray())}")
        {
        }
    }
}
