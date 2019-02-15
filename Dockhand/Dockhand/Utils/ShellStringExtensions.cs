using System.Runtime.InteropServices;
using System.Threading;
using Medallion.Shell;

namespace Dockhand.Utils
{
    //Based on information found here: https://loune.net/2017/06/running-shell-bash-commands-in-net-core/
    public static class ShellStringExtensions
    {
        public static Command RunCommand(this string command, string workingDirectory, CancellationToken? ct = null)
        {
            Command cmd;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                command = command.Replace('/', '\\');
                cmd = command.RunViaCmd(workingDirectory, ct);
            }
            else
            {
                command = command.Replace('\\', '/');
                cmd = command.RunViaBash(workingDirectory, ct);
            }

            return cmd;
        }

        private static Command RunViaBash(this string cmd, string workingDirectory, CancellationToken? ct)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var shell = new CancellableShell(ct, workingDirectory);
            return shell.Run("/bin/bash", "-c", $"\"{escapedArgs}\"");
        }

        private static Command RunViaCmd(this string cmd, string workingDirectory, CancellationToken? ct)
        {
            var shell = new CancellableShell(ct, workingDirectory);
            return shell.Run("cmd.exe", "/C", cmd);
        }

        //private static void HandleProcessOutput(string input, ResponseType responseType, bool labelOutput, ref List<ProcessResponseLine> log)
        //{
        //    if (input == null) { return; }

        //    // Strip control characters from string
        //    var message = Regex.Replace(input, @"\e\[(\d+;)*(\d+)?[ABCDHJKfmsu]", "");
        //    log.Add(new ProcessResponseLine(responseType, message));
        //}
    }
}
