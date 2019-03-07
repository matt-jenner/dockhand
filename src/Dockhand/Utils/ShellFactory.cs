using System.Threading;
using Dockhand.Interfaces;

namespace Dockhand.Utils
{
    internal class ShellFactory : IShellFactory
    {
        ICancellableShell IShellFactory.Create(CancellationToken? ct, string workingDirectory)
        {
            return (ICancellableShell)new CancellableShell(ct, workingDirectory);
        }
    }
}
