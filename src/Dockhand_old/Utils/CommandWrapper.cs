using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Dockhand.Interfaces;
using Medallion.Shell;

namespace Dockhand.Utils
{
    [ExcludeFromCodeCoverage]
    internal class CommandWrapper : ICommandWrapper
    {
        private readonly Command _command;
        private ICommandResult _result;

        internal CommandWrapper(Command command)
        {
            _command = command;
            _result = null;
        }

        /// <summary>Writes to the process standard input</summary>
        public StreamWriter StandardInput => new StreamWriter(_command.StandardInput.BaseStream);

        /// <summary>Reads from the process standard output</summary>
        public StreamReader StandardOutput => new StreamReader(_command.StandardOutput.BaseStream);

        /// <summary>Reads from the process standard error</summary>
        public StreamReader StandardError => new StreamReader(_command.StandardError.BaseStream);

        /// <summary> Kills the process if it is still executing</summary>
        public void Kill() => _command.Kill();

        /// <summary>
        /// A convenience method for <code>command.Task.Wait()</code>. If the task faulted or was cancelled,
        /// this will throw the faulting <see cref="T:System.Exception" /> or <see cref="T:System.Threading.Tasks.TaskCanceledException" /> rather than
        /// the wrapped <see cref="T:System.AggregateException" /> thrown by <see cref="P:System.Threading.Tasks.Task`1.Result" />
        /// </summary>
        public void Wait() => _command.Wait();

        /// <summary>
        /// A convenience method for <code>command.Task.Result</code>. If the task faulted or was canceled,
        /// this will throw the faulting <see cref="T:System.Exception" /> or <see cref="T:System.Threading.Tasks.TaskCanceledException" /> rather than
        /// the wrapped <see cref="T:System.AggregateException" /> thrown by <see cref="P:System.Threading.Tasks.Task`1.Result" />
        /// </summary>
        public ICommandResult Result => _result ?? (_result = new CommandResultWrapper(_command.Result));

        /// <summary>
        /// A Task representing the progress of this <see cref="T:Dockhand.Utils.CommandWrapper" />
        /// </summary>
        public Task<CommandResult> Task => _command.Task;

        /// <summary>
        /// Returns a single streaming <see cref="T:System.Collections.Generic.IEnumerable`1" /> which merges the outputs of
        /// <see cref="M:Medallion.Shell.Streams.ProcessStreamReader.GetLines" /> on <see cref="P:Medallion.Shell.Command.StandardOutput" /> and
        /// <see cref="P:Medallion.Shell.Command.StandardError" />. This is similar to doing 2&gt;&amp;1 on the command line.
        /// 
        /// Merging at the line level means that interleaving of the outputs cannot break up any single lines
        /// </summary>
        public IEnumerable<string> GetOutputAndErrorLines() => _command.GetOutputAndErrorLines();
    }
}
