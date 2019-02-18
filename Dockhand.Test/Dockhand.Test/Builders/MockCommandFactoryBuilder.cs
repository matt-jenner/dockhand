using Dockhand.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Dockhand.Client;
using NSubstitute;

namespace Dockhand.Test.Builders
{
    internal class MockCommandFactoryBuilder
    {
        private string _workingDirectory;
        private Dictionary<string, ICommandWrapper> _commands;

        internal MockCommandFactoryBuilder()
        {
            _commands = new Dictionary<string, ICommandWrapper>();
        }

        internal MockCommandFactoryBuilder ForWorkingDirectory(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
            return this;
        }

        internal MockCommandFactoryBuilder ForCommandReturn(string commandString, ICommandWrapper command)
        {
            _commands.Add(commandString, command);
            return this;
        }


        internal IRunCommands Build()
        {
            var mockCommandFactory = Substitute.For<IRunCommands>();
            foreach (var command in _commands)
            {
                mockCommandFactory
                    .RunCommand(command.Key, _workingDirectory, Arg.Any<CancellationToken?>())
                    .Returns(command.Value);
            }

            return mockCommandFactory;
        }
    }
}
