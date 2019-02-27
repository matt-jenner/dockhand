using System.Diagnostics.CodeAnalysis;
using Dockhand.Interfaces;
using Medallion.Shell;

namespace Dockhand.Utils
{
    [ExcludeFromCodeCoverage]
    internal class CommandResultWrapper : ICommandResult
    {
        private readonly CommandResult _commandResult;

        public int ExitCode => _commandResult.ExitCode;

        public bool Success => _commandResult.Success;

        public string StandardOutput => _commandResult.StandardOutput;

        public string StandardError => _commandResult.StandardError;

        internal CommandResultWrapper(CommandResult commandResult)
        {
            _commandResult = commandResult;
        }
    }
}
