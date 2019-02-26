using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Medallion.Shell;

namespace Dockhand.Interfaces
{
    internal interface ICommandWrapper
    {
        ICommandResult Result { get; }
        StreamReader StandardError { get; }
        StreamWriter StandardInput { get; }
        StreamReader StandardOutput { get; }
        Task<CommandResult> Task { get; }

        IEnumerable<string> GetOutputAndErrorLines();
        void Kill();
        void Wait();
    }
}