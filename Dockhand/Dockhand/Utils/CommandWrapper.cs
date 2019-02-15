using System.Collections.Generic;
using System.Threading.Tasks;
using Dockhand.Interfaces;
using Medallion.Shell;
using Medallion.Shell.Streams;

namespace Dockhand.Utils
{
    internal class CommandWrapper : ICommandWrapper
    {
        private readonly Command _command;

        internal CommandWrapper(Command command)
        {
            _command = command;
        }

        /// <summary>Writes to the process's standard input</summary>
        public ProcessStreamWriter StandardInput => _command.StandardInput;

        /// <summary>Reads from the process's standard output</summary>
        public ProcessStreamReader StandardOutput => _command.StandardOutput;

        /// <summary>Reads from the process's standard error</summary>
        public ProcessStreamReader StandardError => _command.StandardError;

        /// <summary>
        /// Kills the <see cref="P:Medallion.Shell.Command.Process" /> if it is still executing
        /// </summary>
        public void Kill() => _command.Kill();

        /// <summary>
        /// A convenience method for <code>command.Task.Wait()</code>. If the task faulted or was canceled,
        /// this will throw the faulting <see cref="T:System.Exception" /> or <see cref="T:System.Threading.Tasks.TaskCanceledException" /> rather than
        /// the wrapped <see cref="T:System.AggregateException" /> thrown by <see cref="P:System.Threading.Tasks.Task`1.Result" />
        /// </summary>
        public void Wait() => _command.Wait();

        /// <summary>
        /// A convenience method for <code>command.Task.Result</code>. If the task faulted or was canceled,
        /// this will throw the faulting <see cref="T:System.Exception" /> or <see cref="T:System.Threading.Tasks.TaskCanceledException" /> rather than
        /// the wrapped <see cref="T:System.AggregateException" /> thrown by <see cref="P:System.Threading.Tasks.Task`1.Result" />
        /// </summary>
        public CommandResult Result => _command.Result;

        /// <summary>
        /// A <see cref="P:Medallion.Shell.Command.Task" /> representing the progress of this <see cref="T:Medallion.Shell.Command" />
        /// </summary>
        public Task<CommandResult> Task => _command.Task;

        /// <summary>
        /// Returns a single streaming <see cref="T:System.Collections.Generic.IEnumerable`1" /> which merges the outputs of
        /// <see cref="M:Medallion.Shell.Streams.ProcessStreamReader.GetLines" /> on <see cref="P:Medallion.Shell.Command.StandardOutput" /> and
        /// <see cref="P:Medallion.Shell.Command.StandardError" />. This is similar to doing 2&gt;&amp;1 on the command line.
        /// 
        /// Merging at the line level means that interleaving of the outputs cannot break up any single
        /// lines
        /// </summary>
        public IEnumerable<string> GetOutputAndErrorLines() => _command.GetOutputAndErrorLines();
    }
}
