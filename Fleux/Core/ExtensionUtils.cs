namespace Fleux.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ExtensionUtils
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var i in collection)
            {
                action(i);
            }
        }
    }
}
