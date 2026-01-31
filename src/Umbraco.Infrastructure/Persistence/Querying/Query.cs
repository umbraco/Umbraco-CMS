using System.Collections;
using System.Linq.Expressions;
using System.Text;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Querying;

/// <summary>
///     Represents a query builder.
/// </summary>
/// <remarks>A query builder translates Linq queries into Sql queries.</remarks>
public class Query<T> : IQuery<T>
{
    private readonly ISqlContext _sqlContext;
    private readonly List<Tuple<string, object[]>> _wheres = new();

    public Query(ISqlContext sqlContext) => _sqlContext = sqlContext;

    /// <summary>
    ///     Adds a where clause to the query.
    /// </summary>
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
    ///     Adds a where-in clause to the query.
    /// </summary>
    public virtual IQuery<T> WhereIn(Expression<Func<T, object>>? fieldSelector, IEnumerable? values)
    {
        return WhereInOrNotIn(fieldSelector, values, true);
    }

    /// <summary>
    ///     Adds a where-not-in clause to the query.
    /// </summary>
    public virtual IQuery<T> WhereNotIn(Expression<Func<T, object>>? fieldSelector, IEnumerable? values)
    {
        return WhereInOrNotIn(fieldSelector, values, false);
    }

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
    public virtual IQuery<T> WhereInOrNotIn(Expression<Func<T, object>>? fieldSelector, IEnumerable? values, bool isIn = true)
    {
        if (fieldSelector is null)
        {
            return this;
        }

        var expressionHelper = new ModelToSqlExpressionVisitor<T>(_sqlContext.SqlSyntax, _sqlContext.Mappers);
        var whereExpression = expressionHelper.Visit(fieldSelector);

        FixCompareCasing(ref values, ref whereExpression);

        var inNot = isIn ? string.Empty : " NOT";
        _wheres.Add(new Tuple<string, object[]>($"{whereExpression}{inNot} IN (@values)", new object[] { new { values } }));
        return this;
    }

    private static void FixCompareCasing(ref IEnumerable? values, ref string whereExpression)
    {
        Attempt<string[]> attempt = values.TryConvertTo<string[]>();
        if (attempt.Success)
        {
            // fix for case insensitive comparison in databases like PostgreSQL
            whereExpression = $"LOWER({whereExpression})";
            values = attempt.Result?.Select(v => v.ToLower());
        }
    }


    /// <summary>
    ///     Adds a set of OR-ed where clauses to the query.
    /// </summary>
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
                sql = NPoco.Sql.BuilderFor(_sqlContext);
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
    ///     Returns all translated where clauses and their sql parameters
    /// </summary>
    public IEnumerable<Tuple<string, object[]>> GetWhereClauses() => _wheres;
}
