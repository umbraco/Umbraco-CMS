using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Umbraco.Core.Logging;

namespace Umbraco.Core
{
    ///<summary>
    /// Extensions for enumerable sources
    ///</summary>
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> InGroupsOf<T>(this IEnumerable<T> source, int groupSize)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (groupSize <= 0)
                throw new ArgumentException("Must be greater than zero.", "groupSize");

            return source
                .Select((x, i) => Tuple.Create(i / groupSize, x))
                .GroupBy(t => t.Item1, t => t.Item2);
        }

        /// <summary>The distinct by.</summary>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <returns>the unique list</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            where TKey : IEquatable<TKey>
        {
            return source.Distinct(DelegateEqualityComparer<TSource>.CompareMember(keySelector));
        }

        /// <summary>
        /// Returns a sequence of length <paramref name="count"/> whose elements are the result of invoking <paramref name="factory"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory">The factory.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public static IEnumerable<T> Range<T>(Func<int, T> factory, int count)
        {
            for (int i = 1; i <= count; i++)
            {
                yield return factory.Invoke(i - 1);
            }
        }

        /// <summary>The if not null.</summary>
        /// <param name="items">The items.</param>
        /// <param name="action">The action.</param>
        /// <typeparam name="TItem">The type</typeparam>
        public static void IfNotNull<TItem>(this IEnumerable<TItem> items, Action<TItem> action) where TItem : class
        {
            if (items != null)
            {
                foreach (TItem item in items)
                {
                    item.IfNotNull(action);
                }
            }
        }

        /// <summary>The for each.</summary>
        /// <param name="items">The items.</param>
        /// <param name="func">The func.</param>
        /// <typeparam name="TItem">item type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <returns>the Results</returns>
        public static TResult[] ForEach<TItem, TResult>(this IEnumerable<TItem> items, Func<TItem, TResult> func)
        {
            return items.Select(func).ToArray();
        }

        /// <summary>The for each.</summary>
        /// <param name="items">The items.</param>
        /// <param name="action">The action.</param>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <returns>list of TItem</returns>
        public static IEnumerable<TItem> ForEach<TItem>(this IEnumerable<TItem> items, Action<TItem> action)
        {
            if (items != null)
            {
                foreach (TItem item in items)
                {
                    action(item);
                }
            }

            return items;
        }

        /// <summary>The flatten list.</summary>
        /// <param name="e">The items.</param>
        /// <param name="f">The select child.</param>
        /// <typeparam name="T">Item type</typeparam>
        /// <returns>list of TItem</returns>
        public static IEnumerable<T> FlattenList<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
        {
            return e.SelectMany(c => f(c).FlattenList(f)).Concat(e);
        }

        /// <summary>
        /// Returns true if all items in the other collection exist in this collection
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool ContainsAll<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> other)
        {
            return other.Except(source).Any() == false;
        }

        /// <summary>
        /// Returns true if the source contains any of the items in the other list
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool ContainsAny<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> other)
        {
            return other.Any(source.Contains);
        }

        /// <summary>
        /// Removes all matching items from an <see cref="IList{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="predicate">The predicate.</param>
        /// <remarks></remarks>
        public static void RemoveAll<T>(this IList<T> list, Func<T, bool> predicate)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    list.RemoveAt(i--);
                }
            }
        }

        /// <summary>
        /// Removes all matching items from an <see cref="ICollection{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="predicate">The predicate.</param>
        /// <remarks></remarks>
        public static void RemoveAll<T>(this ICollection<T> list, Func<T, bool> predicate)
        {
            var matches = list.Where(predicate).ToArray();
            foreach (var match in matches)
            {
                list.Remove(match);
            }
        }

        public static IEnumerable<TSource> SelectRecursive<TSource>(
          this IEnumerable<TSource> source,
          Func<TSource, IEnumerable<TSource>> recursiveSelector, int maxRecusionDepth = 100)
        {
            var stack = new Stack<IEnumerator<TSource>>();
            stack.Push(source.GetEnumerator());

            try
            {
                while (stack.Count > 0)
                {
                    if (stack.Count > maxRecusionDepth)
                        throw new InvalidOperationException("Maximum recursion depth reached of " + maxRecusionDepth);

                    if (stack.Peek().MoveNext())
                    {
                        var current = stack.Peek().Current;

                        yield return current;

                        stack.Push(recursiveSelector(current).GetEnumerator());
                    }
                    else
                    {
                        stack.Pop().Dispose();
                    }
                }
            }
            finally
            {
                while (stack.Count > 0)
                {
                    stack.Pop().Dispose();
                }
            }
        }

        /// <summary>
        /// Filters a sequence of values to ignore those which are null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll">The coll.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> coll) where T : class
        {
            return coll.Where(x => x != null);
        }

        public static IEnumerable<TBase> ForAllThatAre<TBase, TActual>(this IEnumerable<TBase> sequence, Action<TActual> projection)
            where TActual : class
        {
            return sequence.Select(
                x =>
                {
                    if (x is TActual)
                    {
                        var casted = x as TActual;
                        projection.Invoke(casted);
                    }
                    return x;
                });
        }

        /// <summary>
        /// Finds the index of the first item matching an expression in an enumerable.
        /// </summary>
        /// <typeparam name="T">The type of the enumerated objects.</typeparam>
        /// <param name="items">The enumerable to search.</param>
        /// <param name="predicate">The expression to test the items against.</param>
        /// <returns>The index of the first matching item, or -1.</returns>
        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            return FindIndex(items, 0, predicate);
        }

        /// <summary>
        /// Finds the index of the first item matching an expression in an enumerable.
        /// </summary>
        /// <typeparam name="T">The type of the enumerated objects.</typeparam>
        /// <param name="items">The enumerable to search.</param>
        /// <param name="startIndex">The index to start at.</param>
        /// <param name="predicate">The expression to test the items against.</param>
        /// <returns>The index of the first matching item, or -1.</returns>
        public static int FindIndex<T>(this IEnumerable<T> items, int startIndex, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");
            if (startIndex < 0) throw new ArgumentOutOfRangeException("startIndex");

            var index = startIndex;
            if (index > 0)
                items = items.Skip(index);

            foreach (var item in items)
            {
                if (predicate(item)) return index;
                index++;
            }

            return -1;
        }

        ///<summary>Finds the index of the first occurence of an item in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="item">The item to find.</param>
        ///<returns>The index of the first matching item, or -1 if the item was not found.</returns>
        public static int IndexOf<T>(this IEnumerable<T> items, T item)
        {
            return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i));
        }

        /// <summary>
        /// Determines if 2 lists have equal elements within them regardless of how they are sorted
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <remarks>
        /// The logic for this is taken from:
        /// http://stackoverflow.com/questions/4576723/test-whether-two-ienumerablet-have-the-same-values-with-the-same-frequencies
        /// 
        /// There's a few answers, this one seems the best for it's simplicity and based on the comment of Eamon
        /// </remarks>
        public static bool UnsortedSequenceEqual<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            if (source == null && other == null) return true;
            if (source == null || other == null) return false;

            var list1Groups = source.ToLookup(i => i);
            var list2Groups = other.ToLookup(i => i);
            return list1Groups.Count == list2Groups.Count
               && list1Groups.All(g => g.Count() == list2Groups[g.Key].Count());
        }

    }
}
