using System.Runtime.InteropServices;
using System.Threading;
using Dockhand.Interfaces;

namespace Dockhand.Utils
{
    class CommandFactory : IRunCommands
    {
        public ICommandWrapper RunCommand(string command, string workingDirectory, CancellationToken? ct = null)
        {
            ICommandWrapper cmd;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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

            var shell = new CancellableShell(ct, workingDirectory);
            return new CommandWrapper(shell.Run("/bin/bash", "-c", $"\"{escapedCmd}\""));
        }

        private ICommandWrapper RunViaCmd(string cmd, string workingDirectory, CancellationToken? ct)
        {
            var shell = new CancellableShell(ct, workingDirectory);
            return new CommandWrapper(shell.Run("cmd.exe", "/C", cmd));
        }
    }
}
