using System.Collections;
using System.Linq.Expressions;
using System.Text;
using NPoco;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Infrastructure.Persistence.Querying;

/// <summary>
///     Represents a query builder.
/// </summary>
/// <remarks>A query builder translates Linq queries into Sql queries.</remarks>
public class Query<T> : IQuery<T>
{
    private readonly ISqlContext _sqlContext;
    private readonly List<Tuple<string, object[]>> _wheres = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Persistence.Querying.Query{T}"/> class using the specified SQL context.
    /// </summary>
    /// <param name="sqlContext">The <see cref="ISqlContext"/> instance that provides SQL generation and mapping capabilities for the query.</param>
    public Query(ISqlContext sqlContext) => _sqlContext = sqlContext;

    /// <summary>
    ///     Adds a <c>WHERE</c> clause to the query using the specified predicate expression.
    /// </summary>
    /// <param name="predicate">The predicate expression used to filter the query results. If <c>null</c>, no filtering is applied.</param>
    /// <returns>An <see cref="IQuery{T}"/> representing the query with the applied <c>WHERE</c> clause.</returns>
    public virtual IQuery<T> Where(Expression<Func<T, bool>>? predicate)
    {
        if (predicate is null)
        {
            return this;
        }

        var expressionHelper = new ModelToSqlExpressionVisitor<T>(_sqlContext.SqlSyntax, _sqlContext.Mappers);
        var whereExpression = expressionHelper.Visit(predicate);
        _wheres.Add(new Tuple<string, object[]>(whereExpression, expressionHelper.GetSqlParameters()));
        return this;
    }

    /// <summary>
    ///     Adds a where-in clause to the query for the specified field and values.
    /// </summary>
    /// <param name="fieldSelector">An expression that selects the field to which the where-in clause will be applied.</param>
    /// <param name="values">A collection of values to match against the selected field.</param>
    /// <returns>An <see cref="IQuery{T}"/> representing the query with the where-in clause applied.</returns>
    public virtual IQuery<T> WhereIn(Expression<Func<T, object>>? fieldSelector, IEnumerable? values)
    {
        if (fieldSelector is null)
        {
            return this;
        }

        var expressionHelper = new ModelToSqlExpressionVisitor<T>(_sqlContext.SqlSyntax, _sqlContext.Mappers);
        var whereExpression = expressionHelper.Visit(fieldSelector);
        _wheres.Add(new Tuple<string, object[]>(whereExpression + " IN (@values)", new object[] { new { values } }));
        return this;
    }

    /// <summary>
    /// Adds a <c>WHERE NOT IN</c> clause to the query for the specified field and values.
    /// </summary>
    /// <param name="fieldSelector">An expression that selects the field to which the <c>WHERE NOT IN</c> clause will be applied.</param>
    /// <param name="values">A collection of values to exclude from the results for the specified field.</param>
    /// <returns>The current <see cref="IQuery{T}"/> instance with the <c>WHERE NOT IN</c> clause applied.</returns>
    public virtual IQuery<T> WhereNotIn(Expression<Func<T, object>>? fieldSelector, IEnumerable? values)
    {
        if (fieldSelector is null)
        {
            return this;
        }

        var expressionHelper = new ModelToSqlExpressionVisitor<T>(_sqlContext.SqlSyntax, _sqlContext.Mappers);
        var whereExpression = expressionHelper.Visit(fieldSelector);
        _wheres.Add(new Tuple<string, object[]>(whereExpression + " NOT IN (@values)", new object[] { new { values } }));
        return this;
    }

    /// <summary>
    /// Adds a set of where clauses to the query, combined using the logical OR operator.
    /// </summary>
    /// <param name="predicates">A collection of predicate expressions to combine with OR in the where clause. If <c>null</c> or empty, no clauses are added.</param>
    /// <returns>The current <see cref="IQuery{T}"/> instance with the OR-ed where clauses applied.</returns>
    public virtual IQuery<T> WhereAny(IEnumerable<Expression<Func<T, bool>>>? predicates)
    {
        if (predicates is null)
        {
            return this;
        }

        StringBuilder? sb = null;
        List<object>? parameters = null;
        Sql<ISqlContext>? sql = null;
        foreach (Expression<Func<T, bool>> predicate in predicates)
        {
            // see notes in Where()
            var expressionHelper = new ModelToSqlExpressionVisitor<T>(_sqlContext.SqlSyntax, _sqlContext.Mappers);
            var whereExpression = expressionHelper.Visit(predicate);

            if (sb == null)
            {
                sb = new StringBuilder("(");
                parameters = new List<object>();
                sql = Sql.BuilderFor(_sqlContext);
            }
            else
            {
                sb.Append(" OR ");
                sql?.Append(" OR ");
            }

            sb.Append(whereExpression);
            parameters?.AddRange(expressionHelper.GetSqlParameters());
            sql?.Append(whereExpression, expressionHelper.GetSqlParameters());
        }

        if (sb == null)
        {
            return this;
        }

        sb.Append(")");
        _wheres.Add(Tuple.Create("(" + sql?.SQL + ")", sql?.Arguments)!);

        return this;
    }

    /// <summary>
    /// Returns all translated where clauses and their associated SQL parameters.
    /// </summary>
    /// <returns>
    /// An enumerable of tuples, each containing a where clause string and its associated SQL parameters.
    /// </returns>
    public IEnumerable<Tuple<string, object[]>> GetWhereClauses() => _wheres;
}
