using System.Runtime.InteropServices;

namespace Dockhand.Interfaces
{
    public interface IDockhandEnvironment
    {
        bool IsWindows { get; }
    }
}