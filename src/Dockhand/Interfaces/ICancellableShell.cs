using Medallion.Shell;

namespace Dockhand.Interfaces
{
    internal interface ICancellableShell
    {
        void SetCommandOptions(Shell.Options options);
        Command Run(string executable, params object[] arguments);
    }
}