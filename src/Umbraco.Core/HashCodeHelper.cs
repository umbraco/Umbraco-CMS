namespace Umbraco.Cms.Core;

/// <summary>
///     Borrowed from http://stackoverflow.com/a/2575444/694494
/// </summary>
public static class HashCodeHelper
{
    /// <summary>
    ///     Gets a combined hash code for two values.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <param name="arg1">The first value.</param>
    /// <param name="arg2">The second value.</param>
    /// <returns>A combined hash code.</returns>
    public static int GetHashCode<T1, T2>(T1 arg1, T2 arg2)
    {
        unchecked
        {
            return (31 * arg1!.GetHashCode()) + arg2!.GetHashCode();
        }
    }

    /// <summary>
    ///     Gets a combined hash code for three values.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <typeparam name="T3">The type of the third value.</typeparam>
    /// <param name="arg1">The first value.</param>
    /// <param name="arg2">The second value.</param>
    /// <param name="arg3">The third value.</param>
    /// <returns>A combined hash code.</returns>
    public static int GetHashCode<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3)
    {
        unchecked
        {
            var hash = arg1!.GetHashCode();
            hash = (31 * hash) + arg2!.GetHashCode();
            return (31 * hash) + arg3!.GetHashCode();
        }
    }

    /// <summary>
    ///     Gets a combined hash code for four values.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <typeparam name="T3">The type of the third value.</typeparam>
    /// <typeparam name="T4">The type of the fourth value.</typeparam>
    /// <param name="arg1">The first value.</param>
    /// <param name="arg2">The second value.</param>
    /// <param name="arg3">The third value.</param>
    /// <param name="arg4">The fourth value.</param>
    /// <returns>A combined hash code.</returns>
    public static int GetHashCode<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        unchecked
        {
            var hash = arg1!.GetHashCode();
            hash = (31 * hash) + arg2!.GetHashCode();
            hash = (31 * hash) + arg3!.GetHashCode();
            return (31 * hash) + arg4!.GetHashCode();
        }
    }

    /// <summary>
    ///     Gets a combined hash code for an array of values.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="list">The array of values.</param>
    /// <returns>A combined hash code.</returns>
    public static int GetHashCode<T>(T[] list)
    {
        unchecked
        {
            var hash = 0;
            foreach (T item in list)
            {
                if (item == null)
                {
                    continue;
                }

                hash = (31 * hash) + item.GetHashCode();
            }

            return hash;
        }
    }

    /// <summary>
    ///     Gets a combined hash code for an enumerable collection of values.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="list">The collection of values.</param>
    /// <returns>A combined hash code.</returns>
    public static int GetHashCode<T>(IEnumerable<T> list)
    {
        unchecked
        {
            var hash = 0;
            foreach (T item in list)
            {
                if (item == null)
                {
                    continue;
                }

                hash = (31 * hash) + item.GetHashCode();
            }

            return hash;
        }
    }

    /// <summary>
    ///     Gets a hashcode for a collection for that the order of items
    ///     does not matter.
    ///     So {1, 2, 3} and {3, 2, 1} will get same hash code.
    /// </summary>
    public static int GetHashCodeForOrderNoMatterCollection<T>(
        IEnumerable<T> list)
    {
        unchecked
        {
            var hash = 0;
            var count = 0;
            foreach (T item in list)
            {
                if (item == null)
                {
                    continue;
                }

                hash += item.GetHashCode();
                count++;
            }

            return (31 * hash) + count.GetHashCode();
        }
    }

    /// <summary>
    ///     Alternative way to get a hashcode is to use a fluent
    ///     interface like this:<br />
    ///     return 0.CombineHashCode(field1).CombineHashCode(field2).
    ///     CombineHashCode(field3);
    /// </summary>
    public static int CombineHashCode<T>(this int hashCode, T arg)
    {
        unchecked
        {
            return (31 * hashCode) + arg!.GetHashCode();
        }
    }
}
