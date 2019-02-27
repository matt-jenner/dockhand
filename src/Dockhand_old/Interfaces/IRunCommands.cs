using System.Threading;

namespace Dockhand.Interfaces
{
    internal interface IRunCommands
    {
        ICommandWrapper RunCommand(string command, string workingDirectory, CancellationToken? ct = null);
    }
}