using System.Threading;

namespace Dockhand.Interfaces
{
    interface IShellFactory
    {
        ICancellableShell Create(CancellationToken? ct, string workingDirectory); 
    }
}