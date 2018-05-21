using System;
using System.Collections.Generic;

namespace Umbraco.ModelsBuilder
{
    public static class EnumerableExtensions
    {
        public static void RemoveAll<T>(this IList<T> list, Func<T, bool> predicate)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    list.RemoveAt(i--); // i-- is important here!
                }
            }
        }

        public static IEnumerable<T> And<T>(this IEnumerable<T> enumerable, T item)
        {
            foreach (var x in enumerable) yield return x;
            yield return item;
        }

        public static IEnumerable<T> AndIfNotNull<T>(this IEnumerable<T> enumerable, T item)
            where T : class
        {
            foreach (var x in enumerable) yield return x;
            if (item != null)
                yield return item;
        }
    }
}
