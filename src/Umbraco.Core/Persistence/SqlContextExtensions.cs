using System;
using System.Linq.Expressions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Provides extension methods to <see cref="ISqlContext"/>.
    /// </summary>
    public static class SqlContextExtensions
    {
        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO.</typeparam>
        /// <param name="sqlContext">An <see cref="ISqlContext"/>.</param>
        /// <param name="expression">An expression to visit.</param>
        /// <param name="alias">An optional table alias.</param>
        /// <returns>A SQL statement, and arguments, corresponding to the expression.</returns>
        public static (string Sql, object[] Args) Visit<TDto>(this ISqlContext sqlContext, Expression<Func<TDto, object>> expression, string alias = null)
        {
            var expresionist = new PocoToSqlExpressionVisitor<TDto>(sqlContext, alias);
            var visited = expresionist.Visit(expression);
            return (visited, expresionist.GetSqlParameters());
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO.</typeparam>
        /// <typeparam name="TOut">The type returned by the expression.</typeparam>
        /// <param name="sqlContext">An <see cref="ISqlContext"/>.</param>
        /// <param name="expression">An expression to visit.</param>
        /// <param name="alias">An optional table alias.</param>
        /// <returns>A SQL statement, and arguments, corresponding to the expression.</returns>
        public static (string Sql, object[] Args) Visit<TDto, TOut>(this ISqlContext sqlContext, Expression<Func<TDto, TOut>> expression, string alias = null)
        {
            var expresionist = new PocoToSqlExpressionVisitor<TDto>(sqlContext, alias);
            var visited = expresionist.Visit(expression);
            return (visited, expresionist.GetSqlParameters());
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <typeparam name="TDto1">The type of the first DTO.</typeparam>
        /// <typeparam name="TDto2">The type of the second DTO.</typeparam>
        /// <param name="sqlContext">An <see cref="ISqlContext"/>.</param>
        /// <param name="expression">An expression to visit.</param>
        /// <param name="alias1">An optional table alias for the first DTO.</param>
        /// <param name="alias2">An optional table alias for the second DTO.</param>
        /// <returns>A SQL statement, and arguments, corresponding to the expression.</returns>
        public static (string Sql, object[] Args) Visit<TDto1, TDto2>(this ISqlContext sqlContext, Expression<Func<TDto1, TDto2, object>> expression, string alias1 = null, string alias2 = null)
        {
            var expresionist = new PocoToSqlExpressionVisitor<TDto1, TDto2>(sqlContext, alias1, alias2);
            var visited = expresionist.Visit(expression);
            return (visited, expresionist.GetSqlParameters());
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <typeparam name="TDto1">The type of the first DTO.</typeparam>
        /// <typeparam name="TDto2">The type of the second DTO.</typeparam>
        /// <typeparam name="TOut">The type returned by the expression.</typeparam>
        /// <param name="sqlContext">An <see cref="ISqlContext"/>.</param>
        /// <param name="expression">An expression to visit.</param>
        /// <param name="alias1">An optional table alias for the first DTO.</param>
        /// <param name="alias2">An optional table alias for the second DTO.</param>
        /// <returns>A SQL statement, and arguments, corresponding to the expression.</returns>
        public static (string Sql, object[] Args) Visit<TDto1, TDto2, TOut>(this ISqlContext sqlContext, Expression<Func<TDto1, TDto2, TOut>> expression, string alias1 = null, string alias2 = null)
        {
            var expresionist = new PocoToSqlExpressionVisitor<TDto1, TDto2>(sqlContext, alias1, alias2);
            var visited = expresionist.Visit(expression);
            return (visited, expresionist.GetSqlParameters());
        }
    }
}