using System;
using System.Collections.Generic;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extensions to the List type.
    /// </summary>
    internal static class ListExtensions
    {
        // based upon the original Zip<T1, T2, TResult> method
        public static IEnumerable<TResult> Zip<T1, T2, T3, TResult>(this IEnumerable<T1> e1, IEnumerable<T2> e2, IEnumerable<T3> e3,
            Func<T1, T2, T3, TResult> resultSelector)
        {
            if (e1 == null) throw new ArgumentNullException("e1");
            if (e2 == null) throw new ArgumentNullException("e2");
            if (e3 == null) throw new ArgumentNullException("e3");
            if (resultSelector == null) throw new ArgumentNullException("resultSelector");
            return ZipIterator(e1, e2, e3, resultSelector);
        }

        private static IEnumerable<TResult> ZipIterator<T1, T2,T3, TResult>(IEnumerable<T1> ie1, IEnumerable<T2> ie2, IEnumerable<T3> ie3,
            Func<T1, T2, T3, TResult> resultSelector)
        {
            var e1 = ie1.GetEnumerator();
            try
            {
                var e2 = ie2.GetEnumerator();
                var e3 = ie3.GetEnumerator();
                try
                {
                    while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext())
                        yield return resultSelector(e1.Current, e2.Current, e3.Current);
                }
                finally
                {
                    if (e2 != null)
                        e2.Dispose();
                    if (e3 != null)
                        e3.Dispose();
                }
            }
            finally
            {
                if (e1 != null)
                    e1.Dispose();
            }
        }
    }
}
