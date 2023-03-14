using System.Collections;
using System.Linq.Expressions;

namespace Umbraco.Cms.Core.Persistence.Querying;

/// <summary>
///     Represents a query for building Linq translatable SQL queries
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IQuery<T>
{
    /// <summary>
    ///     Adds a where clause to the query
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns>This instance so calls to this method are chainable</returns>
    IQuery<T> Where(Expression<Func<T, bool>> predicate);

    /// <summary>
    ///     Returns all translated where clauses and their sql parameters
    /// </summary>
    /// <returns></returns>
    IEnumerable<Tuple<string, object[]>> GetWhereClauses();

    /// <summary>
    ///     Adds a where-in clause to the query
    /// </summary>
    /// <param name="fieldSelector"></param>
    /// <param name="values"></param>
    /// <returns>This instance so calls to this method are chainable</returns>
    IQuery<T> WhereIn(Expression<Func<T, object>> fieldSelector, IEnumerable? values);

    /// <summary>
    ///     Adds a set of OR-ed where clauses to the query.
    /// </summary>
    /// <param name="predicates"></param>
    /// <returns>This instance so calls to this method are chainable.</returns>
    IQuery<T> WhereAny(IEnumerable<Expression<Func<T, bool>>> predicates);
}
