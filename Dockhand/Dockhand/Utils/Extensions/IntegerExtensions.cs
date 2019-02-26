using System;
using System.Collections.Generic;
using System.Text;

namespace Dockhand.Utils
{
    public static class IntegerExtensions
    {
        public static bool IsValidAsANetworkPort(this int input) => input > 0 && input <= 65535;

    }
}
