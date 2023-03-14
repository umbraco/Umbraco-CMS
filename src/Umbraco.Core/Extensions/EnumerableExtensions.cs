// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;

namespace Umbraco.Extensions;

/// <summary>
///     Extensions for enumerable sources
/// </summary>
public static class EnumerableExtensions
{
    public static bool IsCollectionEmpty<T>(this IReadOnlyCollection<T>? list) => list == null || list.Count == 0;

    /// <summary>
    ///     Wraps this object instance into an IEnumerable{T} consisting of a single item.
    /// </summary>
    /// <typeparam name="T"> Type of the object. </typeparam>
    /// <param name="item"> The instance that will be wrapped. </param>
    /// <returns> An IEnumerable{T} consisting of a single item. </returns>
    public static IEnumerable<T> Yield<T>(this T item)
    {
        // see EnumeratorBenchmarks - this is faster, and allocates less, than returning an array
        yield return item;
    }

    internal static bool HasDuplicates<T>(this IEnumerable<T> items, bool includeNull)
    {
        var hs = new HashSet<T>();
        foreach (T item in items)
        {
            if ((item != null || includeNull) && !hs.Add(item))
            {
                return true;
            }
        }

        return false;
    }

    public static IEnumerable<IEnumerable<T>> InGroupsOf<T>(this IEnumerable<T>? source, int groupSize)
    {
        if (source == null)
        {
            throw new ArgumentNullException("source");
        }

        if (groupSize <= 0)
        {
            throw new ArgumentException("Must be greater than zero.", "groupSize");
        }

        // following code derived from MoreLinq and does not allocate bazillions of tuples
        T[]? temp = null;
        var count = 0;

        foreach (T item in source)
        {
            if (temp == null)
            {
                temp = new T[groupSize];
            }

            temp[count++] = item;
            if (count != groupSize)
            {
                continue;
            }

            yield return temp /*.Select(x => x)*/;
            temp = null;
            count = 0;
        }

        if (temp != null && count > 0)
        {
            yield return temp.Take(count);
        }
    }

    public static IEnumerable<TResult> SelectByGroups<TResult, TSource>(
        this IEnumerable<TSource> source,
        Func<IEnumerable<TSource>, IEnumerable<TResult>> selector,
        int groupSize)
    {
        // don't want to use a SelectMany(x => x) here - isn't this better?
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (IEnumerable<TResult> resultGroup in source.InGroupsOf(groupSize).Select(selector))
        {
            foreach (TResult result in resultGroup)
            {
                yield return result;
            }
        }
    }

    /// <summary>
    ///     Returns a sequence of length <paramref name="count" /> whose elements are the result of invoking
    ///     <paramref name="factory" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="factory">The factory.</param>
    /// <param name="count">The count.</param>
    /// <returns></returns>
    public static IEnumerable<T> Range<T>(Func<int, T> factory, int count)
    {
        for (var i = 1; i <= count; i++)
        {
            yield return factory.Invoke(i - 1);
        }
    }

    /// <summary>The if not null.</summary>
    /// <param name="items">The items.</param>
    /// <param name="action">The action.</param>
    /// <typeparam name="TItem">The type</typeparam>
    public static void IfNotNull<TItem>(this IEnumerable<TItem> items, Action<TItem> action)
        where TItem : class
    {
        if (items != null)
        {
            foreach (TItem item in items)
            {
                item.IfNotNull(action);
            }
        }
    }

    /// <summary>
    ///     Returns true if all items in the other collection exist in this collection
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool ContainsAll<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> other)
    {
        if (source == null)
        {
            throw new ArgumentNullException("source");
        }

        if (other == null)
        {
            throw new ArgumentNullException("other");
        }

        return other.Except(source).Any() == false;
    }

    /// <summary>
    ///     Returns true if the source contains any of the items in the other list
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool ContainsAny<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> other) =>
        other.Any(source.Contains);

    /// <summary>
    ///     Removes all matching items from an <see cref="IList{T}" />.
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
    ///     Removes all matching items from an <see cref="ICollection{T}" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list.</param>
    /// <param name="predicate">The predicate.</param>
    /// <remarks></remarks>
    public static void RemoveAll<T>(this ICollection<T> list, Func<T, bool> predicate)
    {
        T[] matches = list.Where(predicate).ToArray();
        foreach (T match in matches)
        {
            list.Remove(match);
        }
    }

    public static IEnumerable<TSource> SelectRecursive<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, IEnumerable<TSource>> recursiveSelector,
        int maxRecusionDepth = 100)
    {
        var stack = new Stack<IEnumerator<TSource>>();
        stack.Push(source.GetEnumerator());

        try
        {
            while (stack.Count > 0)
            {
                if (stack.Count > maxRecusionDepth)
                {
                    throw new InvalidOperationException("Maximum recursion depth reached of " + maxRecusionDepth);
                }

                if (stack.Peek().MoveNext())
                {
                    TSource current = stack.Peek().Current;

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
    ///     Filters a sequence of values to ignore those which are null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="coll">The coll.</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> coll)
        where T : class
        =>
        coll.Where(x => x != null)!;

    public static IEnumerable<TBase> ForAllThatAre<TBase, TActual>(
        this IEnumerable<TBase> sequence,
        Action<TActual> projection)
        where TActual : class =>
        sequence.Select(
            x =>
            {
                if (x is TActual casted)
                {
                    projection.Invoke(casted);
                }

                return x;
            });

    /// <summary>
    ///     Finds the index of the first item matching an expression in an enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the enumerated objects.</typeparam>
    /// <param name="items">The enumerable to search.</param>
    /// <param name="predicate">The expression to test the items against.</param>
    /// <returns>The index of the first matching item, or -1.</returns>
    public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate) =>
        FindIndex(items, 0, predicate);

    /// <summary>
    ///     Finds the index of the first item matching an expression in an enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the enumerated objects.</typeparam>
    /// <param name="items">The enumerable to search.</param>
    /// <param name="startIndex">The index to start at.</param>
    /// <param name="predicate">The expression to test the items against.</param>
    /// <returns>The index of the first matching item, or -1.</returns>
    public static int FindIndex<T>(this IEnumerable<T> items, int startIndex, Func<T, bool> predicate)
    {
        if (items == null)
        {
            throw new ArgumentNullException("items");
        }

        if (predicate == null)
        {
            throw new ArgumentNullException("predicate");
        }

        if (startIndex < 0)
        {
            throw new ArgumentOutOfRangeException("startIndex");
        }

        var index = startIndex;
        if (index > 0)
        {
            items = items.Skip(index);
        }

        foreach (T item in items)
        {
            if (predicate(item))
            {
                return index;
            }

            index++;
        }

        return -1;
    }

    /// <summary>Finds the index of the first occurrence of an item in an enumerable.</summary>
    /// <param name="items">The enumerable to search.</param>
    /// <param name="item">The item to find.</param>
    /// <returns>The index of the first matching item, or -1 if the item was not found.</returns>
    public static int IndexOf<T>(this IEnumerable<T> items, T item) =>
        items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i));

    /// <summary>
    ///     Determines if 2 lists have equal elements within them regardless of how they are sorted
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    /// <remarks>
    ///     The logic for this is taken from:
    ///     http://stackoverflow.com/questions/4576723/test-whether-two-ienumerablet-have-the-same-values-with-the-same-frequencies
    ///     There's a few answers, this one seems the best for it's simplicity and based on the comment of Eamon
    /// </remarks>
    public static bool UnsortedSequenceEqual<T>(this IEnumerable<T>? source, IEnumerable<T>? other)
    {
        if (source == null && other == null)
        {
            return true;
        }

        if (source == null || other == null)
        {
            return false;
        }

        ILookup<T, T> list1Groups = source.ToLookup(i => i);
        ILookup<T, T> list2Groups = other.ToLookup(i => i);
        return list1Groups.Count == list2Groups.Count
               && list1Groups.All(g => g.Count() == list2Groups[g.Key].Count());
    }

    /// <summary>
    ///     Transforms an enumerable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static IEnumerable<TTarget> Transform<TSource, TTarget>(
        this IEnumerable<TSource> source,
        Func<IEnumerable<TSource>, IEnumerable<TTarget>> transform) => transform(source);

    /// <summary>
    ///     Gets a null IEnumerable as an empty IEnumerable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> EmptyNull<T>(this IEnumerable<T>? items) => items ?? Enumerable.Empty<T>();

    // the .OfType<T>() filter is nice when there's only one type
    // this is to support filtering with multiple types
    public static IEnumerable<T> OfTypes<T>(this IEnumerable<T> contents, params Type[] types) =>
        contents.Where(x => types.Contains(x?.GetType()));

    public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source)
    {
        using (IEnumerator<T> e = source.GetEnumerator())
        {
            if (e.MoveNext() == false)
            {
                yield break;
            }

            for (T value = e.Current; e.MoveNext(); value = e.Current)
            {
                yield return value;
            }
        }
    }

    public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Direction sortOrder) => sortOrder == Direction.Ascending
        ? source.OrderBy(keySelector)
        : source.OrderByDescending(keySelector);
}
