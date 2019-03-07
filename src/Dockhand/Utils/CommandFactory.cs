using System.Runtime.InteropServices;
using System.Threading;
using Dockhand.Interfaces;

namespace Dockhand.Utils
{
    class CommandFactory : IRunCommands
    {
        private readonly IShellFactory _shellFactory;
        private readonly IDockhandEnvironment _environment;

        public CommandFactory(IShellFactory shellFactory, IDockhandEnvironment environment)
        {
            _shellFactory = shellFactory;
            _environment = environment;
        }

        public ICommandWrapper RunCommand(string command, string workingDirectory, CancellationToken? ct = null)
        {
            ICommandWrapper cmd;
            if (_environment.IsWindows)
            {
                command = command.Replace('/', '\\');
                cmd = RunViaCmd(command, workingDirectory, ct);
            }
            else
            {
                command = command.Replace('\\', '/');
                cmd = RunViaBash(command, workingDirectory, ct);
            }

            return cmd;
        }

        private ICommandWrapper RunViaBash(string cmd, string workingDirectory, CancellationToken? ct)
        {
            var escapedCmd = cmd.Replace("\"", "\\\"");

            var shell = _shellFactory.Create(ct, workingDirectory);
            return new CommandWrapper(shell.Run("/bin/bash", "-c", $"\"{escapedCmd}\""));
        }

        private ICommandWrapper RunViaCmd(string cmd, string workingDirectory, CancellationToken? ct)
        {
            var shell = _shellFactory.Create(ct, workingDirectory);
            return new CommandWrapper(shell.Run("cmd.exe", "/C", cmd));
        }
    }
}
