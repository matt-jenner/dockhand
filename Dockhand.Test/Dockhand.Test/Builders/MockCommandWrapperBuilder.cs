using System.Collections.Generic;
using Dockhand.Interfaces;
using NSubstitute;

namespace Dockhand.Test.Builders
{
    internal class MockCommandWrapperBuilder
    {
        private bool _resultSuccess;
        private IEnumerable<string> _output;
        private int _exitCode;

        internal MockCommandWrapperBuilder()
        {
            _resultSuccess = true;
            _output = new string[0];
        }

        internal MockCommandWrapperBuilder ThatSucceeds()
        {
            _resultSuccess = true;
            return this;
        }

        internal MockCommandWrapperBuilder ThatFails()
        {
            _resultSuccess = false;
            return this;
        }

        internal MockCommandWrapperBuilder WithExitCode(int exitCode)
        {
            _exitCode = exitCode;
            return this;
        }

        internal MockCommandWrapperBuilder WithOutput(IEnumerable<string> output)
        {
            _output = output;
            return this;
        }

        internal ICommandWrapper Build()
        {
            var commandResult = Substitute.For<ICommandResult>();
            commandResult.Success.Returns(_resultSuccess);
            commandResult.ExitCode.Returns(_exitCode);

            var command = Substitute.For<ICommandWrapper>();
            command
                .Result
                .Returns(commandResult);

            command
                .GetOutputAndErrorLines()
                .Returns(_output);

            return command;
        }
    }
}