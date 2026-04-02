namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal static class TupleExtensions
{
    /// <summary>
    /// Applies a specified function to corresponding elements of two lists contained in a tuple, returning a sequence of the results.
    /// </summary>
    /// <param name="t">A tuple containing two lists whose elements will be paired.</param>
    /// <param name="relator">A function that takes an element from each list and produces a result.</param>
    /// <returns>An enumerable containing the results of applying <paramref name="relator"/> to each pair of elements from the two lists. The sequence length is equal to the shorter of the two lists.</returns>
    public static IEnumerable<TResult> Map<T1, T2, TResult>(
        this Tuple<List<T1>, List<T2>> t,
        Func<T1, T2, TResult> relator) => t.Item1.Zip(t.Item2, relator);

    // public static IEnumerable<TResult> Map<T1, T2, T3, TResult>(this Tuple<List<T1>, List<T2>, List<T3>> t, Func<T1, T2, T3, TResult> relator)
    //        {
    //            return t.Item1.Zip(t.Item2, t.Item3, relator);
    //        }
}
