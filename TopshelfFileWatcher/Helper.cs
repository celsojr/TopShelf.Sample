using System;
using System.Collections.Generic;

namespace TopshelfFileWatcher
{
    using static Double;

    internal class Helper
    {
        internal static double GetInterval(string value)
            => TryParse(value, out var interval) 
                ? TimeSpan.FromMinutes(interval).TotalMilliseconds
                : TimeSpan.FromMinutes(1).TotalMilliseconds;
    }

    internal static class ExtensionsMethods
    {
        internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
                action(item);
        }
    }
}