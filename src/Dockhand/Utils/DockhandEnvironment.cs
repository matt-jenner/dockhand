using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Dockhand.Interfaces;

namespace Dockhand.Utils
{
    internal class DockhandEnvironment : IDockhandEnvironment
    {
        public bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}
