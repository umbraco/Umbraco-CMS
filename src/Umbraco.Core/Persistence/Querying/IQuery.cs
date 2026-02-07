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
    ///     Adds a where-not-in clause to the query
    /// </summary>
    /// <param name="fieldSelector"></param>
    /// <param name="values"></param>
    /// <returns>This instance so calls to this method are chainable</returns>
    IQuery<T> WhereNotIn(Expression<Func<T, object>> fieldSelector, IEnumerable? values) => throw new NotImplementedException(); // TODO (V18): Remove default implementation.

    /// <summary>
    ///     Adds a set of OR-ed where clauses to the query.
    /// </summary>
    /// <param name="predicates"></param>
    /// <returns>This instance so calls to this method are chainable.</returns>
    IQuery<T> WhereAny(IEnumerable<Expression<Func<T, bool>>> predicates);

    /// <summary>
    /// Adds a WHERE IN or WHERE NOT IN clause to the query based on the specified field and values.
    /// </summary>
    /// <param name="fieldSelector">An expression that selects the field to apply the IN or NOT IN filter to. If null, no filter is applied.</param>
    /// <param name="values">A collection of values to compare against the selected field. Only records with field values matching (or not
    /// matching, if <paramref name="isIn"/> is false) these values are included.</param>
    /// <param name="isIn">If <see langword="true"/>, applies an IN filter; if <see langword="false"/>, applies a NOT IN filter. The
    /// default is <see langword="true"/>.</param>
    /// <returns>The current query instance with the IN or NOT IN filter applied. If <paramref name="fieldSelector"/> is null,
    /// returns the original query instance without modification.</returns>
    IQuery<T> WhereInOrNotIn(Expression<Func<T, object>>? fieldSelector, IEnumerable? values, bool isIn = true) => throw new NotImplementedException(); // TODO (V18): Remove default implementation.
}
