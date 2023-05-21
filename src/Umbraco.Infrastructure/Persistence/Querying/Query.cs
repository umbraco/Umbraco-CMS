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

    public Query(ISqlContext sqlContext) => _sqlContext = sqlContext;

    /// <summary>
    ///     Adds a where clause to the query.
    /// </summary>
    public virtual IQuery<T> Where(Expression<Func<T, bool>>? predicate)
    {
        if (predicate == null)
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
        if (fieldSelector == null)
        {
            return this;
        }

        var expressionHelper = new ModelToSqlExpressionVisitor<T>(_sqlContext.SqlSyntax, _sqlContext.Mappers);
        var whereExpression = expressionHelper.Visit(fieldSelector);
        _wheres.Add(new Tuple<string, object[]>(whereExpression + " IN (@values)", new object[] { new { values } }));
        return this;
    }

    /// <summary>
    ///     Adds a set of OR-ed where clauses to the query.
    /// </summary>
    public virtual IQuery<T> WhereAny(IEnumerable<Expression<Func<T, bool>>>? predicates)
    {
        if (predicates == null)
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
    ///     Returns all translated where clauses and their sql parameters
    /// </summary>
    public IEnumerable<Tuple<string, object[]>> GetWhereClauses() => _wheres;
}
