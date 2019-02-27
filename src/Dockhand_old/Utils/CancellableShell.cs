using System.Threading;
using CShellNet;
using Medallion.Shell;

namespace Dockhand.Utils
{
    class CancellableShell : CShell
    {
        private CancellationToken? _ct;

        public CancellableShell(CancellationToken? ct, string startingFolder = null) : base(startingFolder)
        {
            _ct = ct;
        }

        public override void SetCommandOptions(Shell.Options options)
        {
            if (_ct != null)
            {
                options.CancellationToken(_ct.Value);
            }

            base.SetCommandOptions(options);
        }
    }
}
