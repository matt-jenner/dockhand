using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dockhand.Utils.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> collection) => !collection.Any();
    }
}
