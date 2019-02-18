using System.Collections.Generic;
using System.Threading.Tasks;
using Medallion.Shell;
using Medallion.Shell.Streams;

namespace Dockhand.Interfaces
{
    internal interface ICommandWrapper
    {
        ICommandResult Result { get; }
        ProcessStreamReader StandardError { get; }
        ProcessStreamWriter StandardInput { get; }
        ProcessStreamReader StandardOutput { get; }
        Task<CommandResult> Task { get; }

        IEnumerable<string> GetOutputAndErrorLines();
        void Kill();
        void Wait();
    }
}