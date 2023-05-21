using System.Linq.Expressions;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Querying;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods to <see cref="ISqlContext" />.
/// </summary>
public static class SqlContextExtensions
{
    /// <summary>
    ///     Visit an expression.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <param name="sqlContext">An <see cref="ISqlContext" />.</param>
    /// <param name="expression">An expression to visit.</param>
    /// <param name="alias">An optional table alias.</param>
    /// <returns>A SQL statement, and arguments, corresponding to the expression.</returns>
    public static (string Sql, object[] Args) VisitDto<TDto>(this ISqlContext sqlContext, Expression<Func<TDto, object>> expression, string? alias = null)
    {
        var visitor = new PocoToSqlExpressionVisitor<TDto>(sqlContext, alias);
        var visited = visitor.Visit(expression);
        return (visited, visitor.GetSqlParameters());
    }

    /// <summary>
    ///     Visit an expression.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <typeparam name="TOut">The type returned by the expression.</typeparam>
    /// <param name="sqlContext">An <see cref="ISqlContext" />.</param>
    /// <param name="expression">An expression to visit.</param>
    /// <param name="alias">An optional table alias.</param>
    /// <returns>A SQL statement, and arguments, corresponding to the expression.</returns>
    public static (string Sql, object[] Args) VisitDto<TDto, TOut>(this ISqlContext sqlContext, Expression<Func<TDto, TOut>> expression, string? alias = null)
    {
        var visitor = new PocoToSqlExpressionVisitor<TDto>(sqlContext, alias);
        var visited = visitor.Visit(expression);
        return (visited, visitor.GetSqlParameters());
    }

    /// <summary>
    ///     Visit an expression.
    /// </summary>
    /// <typeparam name="TDto1">The type of the first DTO.</typeparam>
    /// <typeparam name="TDto2">The type of the second DTO.</typeparam>
    /// <param name="sqlContext">An <see cref="ISqlContext" />.</param>
    /// <param name="expression">An expression to visit.</param>
    /// <param name="alias1">An optional table alias for the first DTO.</param>
    /// <param name="alias2">An optional table alias for the second DTO.</param>
    /// <returns>A SQL statement, and arguments, corresponding to the expression.</returns>
    public static (string Sql, object[] Args) VisitDto<TDto1, TDto2>(this ISqlContext sqlContext, Expression<Func<TDto1, TDto2, object?>> expression, string? alias1 = null, string? alias2 = null)
    {
        var visitor = new PocoToSqlExpressionVisitor<TDto1, TDto2>(sqlContext, alias1, alias2);
        var visited = visitor.Visit(expression);
        return (visited, visitor.GetSqlParameters());
    }

    /// <summary>
    ///     Visit an expression.
    /// </summary>
    /// <typeparam name="TDto1">The type of the first DTO.</typeparam>
    /// <typeparam name="TDto2">The type of the second DTO.</typeparam>
    /// <typeparam name="TOut">The type returned by the expression.</typeparam>
    /// <param name="sqlContext">An <see cref="ISqlContext" />.</param>
    /// <param name="expression">An expression to visit.</param>
    /// <param name="alias1">An optional table alias for the first DTO.</param>
    /// <param name="alias2">An optional table alias for the second DTO.</param>
    /// <returns>A SQL statement, and arguments, corresponding to the expression.</returns>
    public static (string Sql, object[] Args) VisitDto<TDto1, TDto2, TOut>(this ISqlContext sqlContext, Expression<Func<TDto1, TDto2, TOut>> expression, string? alias1 = null, string? alias2 = null)
    {
        var visitor = new PocoToSqlExpressionVisitor<TDto1, TDto2>(sqlContext, alias1, alias2);
        var visited = visitor.Visit(expression);
        return (visited, visitor.GetSqlParameters());
    }

    /// <summary>
    ///     Visit a model expression.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="sqlContext">An <see cref="ISqlContext" />.</param>
    /// <param name="expression">An expression to visit.</param>
    /// <returns>A SQL statement, and arguments, corresponding to the expression.</returns>
    public static (string Sql, object[] Args) VisitModel<TModel>(
        this ISqlContext sqlContext,
        Expression<Func<TModel, object?>> expression)
    {
        var visitor = new ModelToSqlExpressionVisitor<TModel>(sqlContext.SqlSyntax, sqlContext.Mappers);
        var visited = visitor.Visit(expression);
        return (visited, visitor.GetSqlParameters());
    }

    /// <summary>
    ///     Visit a model expression representing a field.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="sqlContext">An <see cref="ISqlContext" />.</param>
    /// <param name="field">An expression to visit, representing a field.</param>
    /// <returns>The name of the field.</returns>
    public static string VisitModelField<TModel>(this ISqlContext sqlContext, Expression<Func<TModel, object?>> field)
    {
        (string sql, object[] _) = sqlContext.VisitModel(field);

        // going to return "<field> = @0"
        // take the first part only
        var pos = sql.IndexOf(' ');
        return sql.Substring(0, pos);
    }
}
