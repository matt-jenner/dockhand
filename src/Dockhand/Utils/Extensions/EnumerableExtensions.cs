using System.Collections.Generic;
using System.Linq;

namespace Dockhand.Utils.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> collection) => !collection.Any();
    }
}
