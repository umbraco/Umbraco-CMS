namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal static class TupleExtensions
{
    public static IEnumerable<TResult> Map<T1, T2, TResult>(
        this Tuple<List<T1>, List<T2>> t,
        Func<T1, T2, TResult> relator) => t.Item1.Zip(t.Item2, relator);

    // public static IEnumerable<TResult> Map<T1, T2, T3, TResult>(this Tuple<List<T1>, List<T2>, List<T3>> t, Func<T1, T2, T3, TResult> relator)
    //        {
    //            return t.Item1.Zip(t.Item2, t.Item3, relator);
    //        }
}
