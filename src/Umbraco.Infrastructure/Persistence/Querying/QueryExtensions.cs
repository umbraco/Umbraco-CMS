using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Infrastructure.Persistence.Querying;

/// <summary>
///     SD: This is a horrible hack but unless we break compatibility with anyone who's actually implemented IQuery{T}
///     there's not much we can do.
///     The IQuery{T} interface is useless without having a GetWhereClauses method and cannot be used for tests.
///     We have to wait till v8 to make this change I suppose.
/// </summary>
internal static class QueryExtensions
{
    /// <summary>
    ///     Returns all translated where clauses and their sql parameters
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Tuple<string, object[]>> GetWhereClauses<T>(this IQuery<T> query)
    {
        if (query is not Query<T> q)
        {
            throw new NotSupportedException(typeof(IQuery<T>) + " cannot be cast to " + typeof(Query<T>));
        }

        return q.GetWhereClauses();
    }
}
