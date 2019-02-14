using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CShellNet;
using Medallion.Shell;

namespace Dockhand
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

        //public static async Task<IEnumerable<string>> RunCommandAsync(this string command, string workingDirectory, CancellationToken ct)
        //{
        //    IEnumerable<string> output = null;
        //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //    {
        //        command = command.Replace('/', '\\');
        //        output = await command.RunViaCmdAsync(workingDirectory, ct);
        //    }
        //    else
        //    {
        //        command = command.Replace('\\', '/');
        //        output = await command.RunViaBashAsync(workingDirectory, ct);
        //    }

        //    return output;
        //}

        //private static async Task<IEnumerable<string>> RunViaBashAsync(this string cmd, string workingDirectory, CancellationToken ct, int? timeoutMsec = null)
        //{
        //    var escapedArgs = cmd.Replace("\"", "\\\"");

        //    var shell = new CancellableShell(ct, workingDirectory);
        //    var command = shell.Run("/bin/bash", "-c", $"\"{escapedArgs}\"");

        //    var tout = command.StandardOutput.ReadToEndAsync();
        //    var terr = command.StandardError.ReadToEndAsync();

        //    await Task.WhenAll(tout, terr);

        //    await Task.Run(() => command.Wait());

        //    var output = await tout;
        //    var errors = await terr;

        //    if (ct.IsCancellationRequested)
        //    {
        //        throw new OperationCanceledException(errors);
        //    }

        //    return output.Split(Environment.NewLine);
        //}

        //private static async Task<IEnumerable<string>> RunViaCmdAsync(this string cmd, string workingDirectory, CancellationToken ct, int? timeoutMsec = null)
        //{
        //    var shell = new CancellableShell(ct, workingDirectory);
        //    var command = shell.Run("cmd.exe", "/C", cmd);

        //    var tout = command.StandardOutput.ReadToEndAsync();
        //    var terr = command.StandardError.ReadToEndAsync();

        //    await Task.WhenAll(tout, terr);

        //    await command.Task;

        //    //await Task.Run(() => command.Wait());

        //    var output = await tout;
        //    var errors = await terr;

        //    if (ct.IsCancellationRequested)
        //    {
        //        throw new OperationCanceledException(errors);
        //    }

        //    return output.Split(Environment.NewLine);
        //}

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

        //private static async Task<IEnumerable<ProcessResponseLine>> ExecuteProcessAsTask(Process process, int? timeoutMSec, bool labelOutput = true)
        //{
        //    try
        //    {
        //        var log = new List<ProcessResponseLine>();
        //        process.OutputDataReceived += (sender, args) => HandleProcessOutput(args.Data, ResponseType.Information, labelOutput, ref log);
        //        process.ErrorDataReceived += (sender, args) => HandleProcessOutput(args.Data, ResponseType.Error, labelOutput, ref log); ;
        //        process.Start();
        //        process.BeginOutputReadLine();
        //        process.BeginErrorReadLine();
        //        process.WaitForExit(timeoutMSec ?? 120000);
        //        if (!process.HasExited)
        //        {
        //            process.Kill();
        //        }
        //        return log;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(await process.StandardError.ReadToEndAsync());
        //        throw;
        //    }
        //}
    }
}
