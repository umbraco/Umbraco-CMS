using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Extensions
{
    public static partial class NPocoSqlExtensions
    {
        #region Where

        /// <summary>
        /// Appends a WHERE clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="predicate">A predicate to transform and append to the Sql statement.</param>
        /// <param name="alias">An optional alias for the table.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> Where<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, bool>> predicate, string? alias = null)
        {
            var (s, a) = sql.SqlContext.VisitDto(predicate, alias);
            return sql.Where(s, a);
        }

        /// <summary>
        /// Appends a WHERE clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto1">The type of Dto 1.</typeparam>
        /// <typeparam name="TDto2">The type of Dto 2.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="predicate">A predicate to transform and append to the Sql statement.</param>
        /// <param name="alias1">An optional alias for Dto 1 table.</param>
        /// <param name="alias2">An optional alias for Dto 2 table.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> Where<TDto1, TDto2>(this Sql<ISqlContext> sql, Expression<Func<TDto1, TDto2, bool>> predicate, string? alias1 = null, string? alias2 = null)
        {
            var (s, a) = sql.SqlContext.VisitDto(predicate, alias1, alias2);
            return sql.Where(s, a);
        }

        /// <summary>
        /// Appends a WHERE IN clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="field">An expression specifying the field.</param>
        /// <param name="values">The values.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> WhereIn<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field, IEnumerable? values)
        {
            var fieldName = sql.SqlContext.SqlSyntax.GetFieldName(field);
            sql.Where(fieldName + " IN (@values)", new { values });
            return sql;
        }

        /// <summary>
        /// Appends a WHERE IN clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="field">An expression specifying the field.</param>
        /// <param name="values">A subquery returning the value.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> WhereIn<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field, Sql<ISqlContext>? values)
        {
            return WhereIn(sql, field, values, false, null);
        }

        public static Sql<ISqlContext> WhereIn<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field, Sql<ISqlContext>? values, string tableAlias)
        {
            return sql.WhereIn(field, values, false, tableAlias);
        }


        public static Sql<ISqlContext> WhereLike<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> fieldSelector, Sql<ISqlContext>? valuesSql)
        {
            var fieldName = sql.SqlContext.SqlSyntax.GetFieldName(fieldSelector);
            sql.Where(fieldName + " LIKE (" + valuesSql?.SQL + ")", valuesSql?.Arguments);
            return sql;
        }

        public static Sql<ISqlContext> Union(this Sql<ISqlContext> sql, Sql<ISqlContext> sql2)
        {
            return sql.Append( " UNION ").Append(sql2);
        }

        public static Sql<ISqlContext>.SqlJoinClause<ISqlContext> InnerJoinNested(this Sql<ISqlContext> sql, Sql<ISqlContext> nestedQuery, string alias)
        {
            return new Sql<ISqlContext>.SqlJoinClause<ISqlContext>(sql.Append("INNER JOIN (").Append(nestedQuery)
                .Append($") [{alias}]"));
        }

        public static Sql<ISqlContext> WhereLike<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> fieldSelector, string likeValue)
        {
            var fieldName = sql.SqlContext.SqlSyntax.GetFieldName(fieldSelector);
            sql.Where(fieldName + " LIKE ('" + likeValue + "')");
            return sql;
        }

        /// <summary>
        /// Appends a WHERE NOT IN clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="field">An expression specifying the field.</param>
        /// <param name="values">The values.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> WhereNotIn<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field, IEnumerable values)
        {
            var fieldName = sql.SqlContext.SqlSyntax.GetFieldName(field);
            sql.Where(fieldName + " NOT IN (@values)", new { values });
            return sql;
        }

        /// <summary>
        /// Appends a WHERE NOT IN clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="field">An expression specifying the field.</param>
        /// <param name="values">A subquery returning the value.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> WhereNotIn<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field, Sql<ISqlContext> values)
        {
            return sql.WhereIn(field, values, true);
        }

        /// <summary>
        /// Appends multiple OR WHERE IN clauses to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="fields">Expressions specifying the fields.</param>
        /// <param name="values">The values.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql WhereAnyIn<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>>[] fields, IEnumerable values)
        {
            ISqlSyntaxProvider sqlSyntax = sql.SqlContext.SqlSyntax;
            var fieldNames = fields.Select(x => sqlSyntax.GetFieldName(x)).ToArray();
            var sb = new StringBuilder();
            sb.Append("(");
            for (var i = 0; i < fieldNames.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(" OR ");
                }

                sb.Append(fieldNames[i]);
                sql.Append(" IN (@values)");
            }
            sb.Append(")");
            sql.Where(sb.ToString(), new { values });
            return sql;
        }

        private static Sql<ISqlContext> WhereIn<T>(this Sql<ISqlContext> sql, Expression<Func<T, object?>> fieldSelector, Sql? valuesSql, bool not)
        {
            return WhereIn(sql, fieldSelector, valuesSql, not, null);
        }

        private static Sql<ISqlContext> WhereIn<T>(this Sql<ISqlContext> sql, Expression<Func<T, object?>> fieldSelector, Sql? valuesSql, bool not, string? tableAlias)
        {
            var fieldName = sql.SqlContext.SqlSyntax.GetFieldName(fieldSelector, tableAlias);
            sql.Where(fieldName + (not ? " NOT" : "") +" IN (" + valuesSql?.SQL + ")", valuesSql?.Arguments);
            return sql;
        }

        /// <summary>
        /// Appends multiple OR WHERE clauses to the Sql statement.
        /// </summary>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="predicates">The WHERE predicates.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> WhereAny(this Sql<ISqlContext> sql, params Func<Sql<ISqlContext>, Sql<ISqlContext>>[] predicates)
        {
            var wsql = new Sql<ISqlContext>(sql.SqlContext);

            wsql.Append("(");
            for (var i = 0; i < predicates.Length; i++)
            {
                if (i > 0)
                {
                    wsql.Append(") OR (");
                }

                var temp = new Sql<ISqlContext>(sql.SqlContext);
                temp = predicates[i](temp);
                wsql.Append(temp.SQL.TrimStart("WHERE "), temp.Arguments);
            }
            wsql.Append(")");

            return sql.Where(wsql.SQL, wsql.Arguments);
        }

        /// <summary>
        /// Appends a WHERE NOT NULL clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="field">Expression specifying the field.</param>
        /// <param name="tableAlias">An optional alias for the table.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> WhereNotNull<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field, string? tableAlias = null)
        {
            return sql.WhereNull(field, tableAlias, true);
        }

        /// <summary>
        /// Appends a WHERE [NOT] NULL clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="field">Expression specifying the field.</param>
        /// <param name="tableAlias">An optional alias for the table.</param>
        /// <param name="not">A value indicating whether to NOT NULL.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> WhereNull<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field, string? tableAlias = null, bool not = false)
        {
            var column = sql.GetColumns(columnExpressions: new[] { field }, tableAlias: tableAlias, withAlias: false).First();
            return sql.Where("(" + column + " IS " + (not ? "NOT " : "") + "NULL)");
        }

        #endregion

        #region From

        /// <summary>
        /// Appends a FROM clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="alias">An optional table alias</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> From<TDto>(this Sql<ISqlContext> sql, string? alias = null)
        {
            Type type = typeof (TDto);
            var tableName = type.GetTableName();

            var from = sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName);
            if (!string.IsNullOrWhiteSpace(alias))
            {
                from += " " + sql.SqlContext.SqlSyntax.GetQuotedTableName(alias);
            }

            sql.From(from);

            return sql;
        }

        #endregion

        #region OrderBy, GroupBy

        /// <summary>
        /// Appends an ORDER BY clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="field">An expression specifying the field.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> OrderBy<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field)
        {
            return sql.OrderBy("(" + sql.SqlContext.SqlSyntax.GetFieldName(field) + ")");
        }

        public static Sql<ISqlContext> OrderBy<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field, string alias)
        {
            return sql.OrderBy("(" + sql.SqlContext.SqlSyntax.GetFieldName(field, alias) + ")");
        }

        /// <summary>
        /// Appends an ORDER BY clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="fields">Expression specifying the fields.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> OrderBy<TDto>(this Sql<ISqlContext> sql, params Expression<Func<TDto, object?>>[] fields)
        {
            ISqlSyntaxProvider sqlSyntax = sql.SqlContext.SqlSyntax;
            var columns = fields.Length == 0
                ? sql.GetColumns<TDto>(withAlias: false)
                : fields.Select(x => sqlSyntax.GetFieldName(x)).ToArray();
            return sql.OrderBy(columns);
        }

        /// <summary>
        /// Appends an ORDER BY DESC clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="field">An expression specifying the field.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> OrderByDescending<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field)
        {
            return sql.OrderByDescending(sql.SqlContext.SqlSyntax.GetFieldName(field));
        }

        /// <summary>
        /// Appends an ORDER BY DESC clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="fields">Expression specifying the fields.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> OrderByDescending<TDto>(this Sql<ISqlContext> sql, params Expression<Func<TDto, object?>>[] fields)
        {
            ISqlSyntaxProvider sqlSyntax = sql.SqlContext.SqlSyntax;
            var columns = fields.Length == 0
                ? sql.GetColumns<TDto>(withAlias: false)
                : fields.Select(x => sqlSyntax.GetFieldName(x)).ToArray();
            return sql.OrderByDescending(columns);
        }

        /// <summary>
        /// Appends an ORDER BY DESC clause to the Sql statement.
        /// </summary>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="fields">Fields.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> OrderByDescending(this Sql<ISqlContext> sql, params string?[] fields)
        {
            return sql.Append("ORDER BY " + string.Join(", ", fields.Select(x => x + " DESC")));
        }

        /// <summary>
        /// Appends a GROUP BY clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="field">An expression specifying the field.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> GroupBy<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field)
        {
            return sql.GroupBy(sql.SqlContext.SqlSyntax.GetFieldName(field));
        }

        /// <summary>
        /// Appends a GROUP BY clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="fields">Expression specifying the fields.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> GroupBy<TDto>(this Sql<ISqlContext> sql, params Expression<Func<TDto, object?>>[] fields)
        {
            ISqlSyntaxProvider sqlSyntax = sql.SqlContext.SqlSyntax;
            var columns = fields.Length == 0
                ? sql.GetColumns<TDto>(withAlias: false)
                : fields.Select(x => sqlSyntax.GetFieldName(x)).ToArray();
            return sql.GroupBy(columns);
        }

        /// <summary>
        /// Appends more ORDER BY or GROUP BY fields to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="fields">Expressions specifying the fields.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> AndBy<TDto>(this Sql<ISqlContext> sql, params Expression<Func<TDto, object?>>[] fields)
        {
            ISqlSyntaxProvider sqlSyntax = sql.SqlContext.SqlSyntax;
            var columns = fields.Length == 0
                ? sql.GetColumns<TDto>(withAlias: false)
                : fields.Select(x => sqlSyntax.GetFieldName(x)).ToArray();
            return sql.Append(", " + string.Join(", ", columns));
        }

        /// <summary>
        /// Appends more ORDER BY DESC fields to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="fields">Expressions specifying the fields.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> AndByDescending<TDto>(this Sql<ISqlContext> sql, params Expression<Func<TDto, object?>>[] fields)
        {
            ISqlSyntaxProvider sqlSyntax = sql.SqlContext.SqlSyntax;
            var columns = fields.Length == 0
                ? sql.GetColumns<TDto>(withAlias: false)
                : fields.Select(x => sqlSyntax.GetFieldName(x)).ToArray();
            return sql.Append(", " + string.Join(", ", columns.Select(x => x + " DESC")));
        }

        #endregion

        #region Joins

        /// <summary>
        /// Appends a CROSS JOIN clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="alias">An optional alias for the joined table.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> CrossJoin<TDto>(this Sql<ISqlContext> sql, string? alias = null)
        {
            Type type = typeof(TDto);
            var tableName = type.GetTableName();
            var join = sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName);
            if (alias != null)
            {
                join += " " + sql.SqlContext.SqlSyntax.GetQuotedTableName(alias);
            }

            return sql.Append("CROSS JOIN " + join);
        }

        /// <summary>
        /// Appends an INNER JOIN clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="alias">An optional alias for the joined table.</param>
        /// <returns>A SqlJoin statement.</returns>
        public static Sql<ISqlContext>.SqlJoinClause<ISqlContext> InnerJoin<TDto>(this Sql<ISqlContext> sql, string? alias = null)
        {
            Type type = typeof(TDto);
            var tableName = type.GetTableName();
            var join = sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName);
            if (alias != null)
            {
                join += " " + sql.SqlContext.SqlSyntax.GetQuotedTableName(alias);
            }

            return sql.InnerJoin(join);
        }

        /// <summary>
        /// Appends an INNER JOIN clause using a nested query.
        /// </summary>
        /// <param name="sql">The SQL statement.</param>
        /// <param name="nestedSelect">The nested sql query.</param>
        /// <param name="alias">An optional alias for the joined table.</param>
        /// <returns>A SqlJoin statement.</returns>
        public static Sql<ISqlContext>.SqlJoinClause<ISqlContext> InnerJoin(this Sql<ISqlContext> sql, Sql<ISqlContext> nestedSelect, string? alias = null)
        {
            var join = $"({nestedSelect.SQL})";
            if (alias is not null)
            {
                join += " " + sql.SqlContext.SqlSyntax.GetQuotedTableName(alias);
            }

            return sql.InnerJoin(join);
        }

        /// <summary>
        /// Appends a LEFT JOIN clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="alias">An optional alias for the joined table.</param>
        /// <returns>A SqlJoin statement.</returns>
        public static Sql<ISqlContext>.SqlJoinClause<ISqlContext> LeftJoin<TDto>(this Sql<ISqlContext> sql, string? alias = null)
        {
            Type type = typeof(TDto);
            var tableName = type.GetTableName();
            var join = sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName);
            if (alias != null)
            {
                join += " " + sql.SqlContext.SqlSyntax.GetQuotedTableName(alias);
            }

            return sql.LeftJoin(join);
        }

        /// <summary>
        /// Appends a LEFT JOIN clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="nestedJoin">A nested join statement.</param>
        /// <param name="alias">An optional alias for the joined table.</param>
        /// <returns>A SqlJoin statement.</returns>
        /// <remarks>Nested statement produces LEFT JOIN xxx JOIN yyy ON ... ON ...</remarks>
        public static Sql<ISqlContext>.SqlJoinClause<ISqlContext> LeftJoin<TDto>(
            this Sql<ISqlContext> sql,
            Func<Sql<ISqlContext>, Sql<ISqlContext>> nestedJoin,
            string? alias = null) =>
            sql.SqlContext.SqlSyntax.LeftJoinWithNestedJoin<TDto>(sql, nestedJoin, alias);

        /// <summary>
        /// Appends an LEFT JOIN clause using a nested query.
        /// </summary>
        /// <param name="sql">The SQL statement.</param>
        /// <param name="nestedSelect">The nested sql query.</param>
        /// <param name="alias">An optional alias for the joined table.</param>
        /// <returns>A SqlJoin statement.</returns>
        public static Sql<ISqlContext>.SqlJoinClause<ISqlContext> LeftJoin(this Sql<ISqlContext> sql, Sql<ISqlContext> nestedSelect, string? alias = null)
        {
            var join = $"({nestedSelect.SQL})";
            if (alias is not null)
            {
                join += " " + sql.SqlContext.SqlSyntax.GetQuotedTableName(alias);
            }

            return sql.LeftJoin(join);
        }

        /// <summary>
        /// Appends a RIGHT JOIN clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="alias">An optional alias for the joined table.</param>
        /// <returns>A SqlJoin statement.</returns>
        public static Sql<ISqlContext>.SqlJoinClause<ISqlContext> RightJoin<TDto>(this Sql<ISqlContext> sql, string? alias = null)
        {
            Type type = typeof(TDto);
            var tableName = type.GetTableName();
            var join = sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName);
            if (alias != null)
            {
                join += " " + sql.SqlContext.SqlSyntax.GetQuotedTableName(alias);
            }

            return sql.RightJoin(join);
        }

        /// <summary>
        /// Appends an ON clause to a SqlJoin statement.
        /// </summary>
        /// <typeparam name="TLeft">The type of the left Dto.</typeparam>
        /// <typeparam name="TRight">The type of the right Dto.</typeparam>
        /// <param name="sqlJoin">The Sql join statement.</param>
        /// <param name="leftField">An expression specifying the left field.</param>
        /// <param name="rightField">An expression specifying the right field.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> On<TLeft, TRight>(this Sql<ISqlContext>.SqlJoinClause<ISqlContext> sqlJoin,
            Expression<Func<TLeft, object?>> leftField, Expression<Func<TRight, object?>> rightField)
        {
            // TODO: ugly - should define on SqlContext!

            var xLeft = new Sql<ISqlContext>(sqlJoin.SqlContext).Columns(leftField);
            var xRight = new Sql<ISqlContext>(sqlJoin.SqlContext).Columns(rightField);
            return sqlJoin.On(xLeft + " = " + xRight);

            //var sqlSyntax = clause.SqlContext.SqlSyntax;

            //var leftType = typeof (TLeft);
            //var rightType = typeof (TRight);
            //var leftTableName = leftType.GetTableName();
            //var rightTableName = rightType.GetTableName();

            //var leftColumn = ExpressionHelper.FindProperty(leftMember) as PropertyInfo;
            //var rightColumn = ExpressionHelper.FindProperty(rightMember) as PropertyInfo;

            //var leftColumnName = leftColumn.GetColumnName();
            //var rightColumnName = rightColumn.GetColumnName();

            //string onClause = $"{sqlSyntax.GetQuotedTableName(leftTableName)}.{sqlSyntax.GetQuotedColumnName(leftColumnName)} = {sqlSyntax.GetQuotedTableName(rightTableName)}.{sqlSyntax.GetQuotedColumnName(rightColumnName)}";
            //return clause.On(onClause);
        }

        /// <summary>
        /// Appends an ON clause to a SqlJoin statement.
        /// </summary>
        /// <param name="sqlJoin">The Sql join statement.</param>
        /// <param name="on">A Sql fragment to use as the ON clause body.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> On(this Sql<ISqlContext>.SqlJoinClause<ISqlContext> sqlJoin, Func<Sql<ISqlContext>, Sql<ISqlContext>> on)
        {
            var sql = new Sql<ISqlContext>(sqlJoin.SqlContext);
            sql = on(sql);
            var text = sql.SQL.Trim().TrimStart("WHERE").Trim();
            return sqlJoin.On(text, sql.Arguments);
        }

        /// <summary>
        /// Appends an ON clause to a SqlJoin statement.
        /// </summary>
        /// <typeparam name="TDto1">The type of Dto 1.</typeparam>
        /// <typeparam name="TDto2">The type of Dto 2.</typeparam>
        /// <param name="sqlJoin">The SqlJoin statement.</param>
        /// <param name="predicate">A predicate to transform and use as the ON clause body.</param>
        /// <param name="aliasLeft">An optional alias for Dto 1 table.</param>
        /// <param name="aliasRight">An optional alias for Dto 2 table.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> On<TDto1, TDto2>(this Sql<ISqlContext>.SqlJoinClause<ISqlContext> sqlJoin, Expression<Func<TDto1, TDto2, bool>> predicate, string? aliasLeft = null, string? aliasRight = null)
        {
            var expresionist = new PocoToSqlExpressionVisitor<TDto1, TDto2>(sqlJoin.SqlContext, aliasLeft, aliasRight);
            var onExpression = expresionist.Visit(predicate);
            return sqlJoin.On(onExpression, expresionist.GetSqlParameters());
        }

        /// <summary>
        /// Appends an ON clause to a SqlJoin statement.
        /// </summary>
        /// <typeparam name="TDto1">The type of Dto 1.</typeparam>
        /// <typeparam name="TDto2">The type of Dto 2.</typeparam>
        /// <typeparam name="TDto3">The type of Dto 3.</typeparam>
        /// <param name="sqlJoin">The SqlJoin statement.</param>
        /// <param name="predicate">A predicate to transform and use as the ON clause body.</param>
        /// <param name="aliasLeft">An optional alias for Dto 1 table.</param>
        /// <param name="aliasRight">An optional alias for Dto 2 table.</param>
        /// <param name="aliasOther">An optional alias for Dto 3 table.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> On<TDto1, TDto2, TDto3>(this Sql<ISqlContext>.SqlJoinClause<ISqlContext> sqlJoin, Expression<Func<TDto1, TDto2, TDto3, bool>> predicate, string? aliasLeft = null, string? aliasRight = null, string? aliasOther = null)
        {
            var expresionist = new PocoToSqlExpressionVisitor<TDto1, TDto2, TDto3>(sqlJoin.SqlContext, aliasLeft, aliasRight, aliasOther);
            var onExpression = expresionist.Visit(predicate);
            return sqlJoin.On(onExpression, expresionist.GetSqlParameters());
        }

        #endregion

        #region Select

        /// <summary>
        /// Alters a Sql statement to return a maximum amount of rows.
        /// </summary>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="count">The maximum number of rows to return.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> SelectTop(this Sql<ISqlContext> sql, int count)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            return sql.SqlContext.SqlSyntax.SelectTop(sql, count);
        }

        /// <summary>
        /// Creates a SELECT COUNT(*) Sql statement.
        /// </summary>
        /// <param name="sql">The origin sql.</param>
        /// <param name="alias">An optional alias.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> SelectCount(this Sql<ISqlContext> sql, string? alias = null)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            var text = "COUNT(*)";
            if (alias != null)
            {
                text += " AS " + sql.SqlContext.SqlSyntax.GetQuotedColumnName(alias);
            }

            return sql.Select(text);
        }

        /// <summary>
        /// Creates a SELECT COUNT Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO to count.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="fields">Expressions indicating the columns to count.</param>
        /// <returns>The Sql statement.</returns>
        /// <remarks>
        /// <para>If <paramref name="fields"/> is empty, all columns are counted.</para>
        /// </remarks>
        public static Sql<ISqlContext> SelectCount<TDto>(this Sql<ISqlContext> sql, params Expression<Func<TDto, object?>>[] fields)
            => sql.SelectCount(null, fields);

        /// <summary>
        /// Creates a SELECT COUNT Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO to count.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="alias">An alias.</param>
        /// <param name="fields">Expressions indicating the columns to count.</param>
        /// <returns>The Sql statement.</returns>
        /// <remarks>
        /// <para>If <paramref name="fields"/> is empty, all columns are counted.</para>
        /// </remarks>
        public static Sql<ISqlContext> SelectCount<TDto>(this Sql<ISqlContext> sql, string? alias, params Expression<Func<TDto, object?>>[] fields)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            ISqlSyntaxProvider sqlSyntax = sql.SqlContext.SqlSyntax;
            var columns = fields.Length == 0
                ? sql.GetColumns<TDto>(withAlias: false)
                : fields.Select(x => sqlSyntax.GetFieldName(x)).ToArray();
            var text = "COUNT (" + string.Join(", ", columns) + ")";
            if (alias != null)
            {
                text += " AS " + sql.SqlContext.SqlSyntax.GetQuotedColumnName(alias);
            }

            return sql.Select(text);
        }

        /// <summary>
        /// Creates a SELECT * Sql statement.
        /// </summary>
        /// <param name="sql">The origin sql.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> SelectAll(this Sql<ISqlContext> sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            return sql.Select("*");
        }

        /// <summary>
        /// Creates a SELECT Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO to select.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="fields">Expressions indicating the columns to select.</param>
        /// <returns>The Sql statement.</returns>
        /// <remarks>
        /// <para>If <paramref name="fields"/> is empty, all columns are selected.</para>
        /// </remarks>
        public static Sql<ISqlContext> Select<TDto>(this Sql<ISqlContext> sql, params Expression<Func<TDto, object?>>[] fields)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            return sql.Select(sql.GetColumns(columnExpressions: fields));
        }

        /// <summary>
        /// Creates a SELECT DISTINCT Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO to select.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="fields">Expressions indicating the columns to select.</param>
        /// <returns>The Sql statement.</returns>
        /// <remarks>
        /// <para>If <paramref name="fields"/> is empty, all columns are selected.</para>
        /// </remarks>
        public static Sql<ISqlContext> SelectDistinct<TDto>(this Sql<ISqlContext> sql, params Expression<Func<TDto, object?>>[] fields)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            var columns = sql.GetColumns(columnExpressions: fields);
            sql.Append("SELECT DISTINCT " + string.Join(", ", columns));
            return sql;
        }

        public static Sql<ISqlContext> SelectDistinct(this Sql<ISqlContext> sql, params object[] columns)
        {
            sql.Append("SELECT DISTINCT " + string.Join(", ", columns));
            return sql;
        }

        //this.Append("SELECT " + string.Join(", ", columns), new object[0]);

        /// <summary>
        /// Creates a SELECT Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO to select.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="tableAlias">A table alias.</param>
        /// <param name="fields">Expressions indicating the columns to select.</param>
        /// <returns>The Sql statement.</returns>
        /// <remarks>
        /// <para>If <paramref name="fields"/> is empty, all columns are selected.</para>
        /// </remarks>
        public static Sql<ISqlContext> Select<TDto>(this Sql<ISqlContext> sql, string tableAlias, params Expression<Func<TDto, object?>>[] fields)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            return sql.Select(sql.GetColumns(tableAlias: tableAlias, columnExpressions: fields));
        }

        /// <summary>
        /// Adds columns to a SELECT Sql statement.
        /// </summary>
        /// <param name="sql">The origin sql.</param>
        /// <param name="fields">Columns to select.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> AndSelect(this Sql<ISqlContext> sql, params string[] fields)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            return sql.Append(", " + string.Join(", ", fields));
        }

        /// <summary>
        /// Adds columns to a SELECT Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO to select.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="fields">Expressions indicating the columns to select.</param>
        /// <returns>The Sql statement.</returns>
        /// <remarks>
        /// <para>If <paramref name="fields"/> is empty, all columns are selected.</para>
        /// </remarks>
        public static Sql<ISqlContext> AndSelect<TDto>(this Sql<ISqlContext> sql, params Expression<Func<TDto, object?>>[] fields)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            return sql.Append(", " + string.Join(", ", sql.GetColumns(columnExpressions: fields)));
        }

        /// <summary>
        /// Adds columns to a SELECT Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO to select.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="tableAlias">A table alias.</param>
        /// <param name="fields">Expressions indicating the columns to select.</param>
        /// <returns>The Sql statement.</returns>
        /// <remarks>
        /// <para>If <paramref name="fields"/> is empty, all columns are selected.</para>
        /// </remarks>
        public static Sql<ISqlContext> AndSelect<TDto>(this Sql<ISqlContext> sql, string tableAlias, params Expression<Func<TDto, object?>>[] fields)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            return sql.Append(", " + string.Join(", ", sql.GetColumns(tableAlias: tableAlias, columnExpressions: fields)));
        }

        /// <summary>
        /// Adds a COUNT(*) to a SELECT Sql statement.
        /// </summary>
        /// <param name="sql">The origin sql.</param>
        /// <param name="alias">An optional alias.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> AndSelectCount(this Sql<ISqlContext> sql, string? alias = null)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            var text = ", COUNT(*)";
            if (alias != null)
            {
                text += " AS " + sql.SqlContext.SqlSyntax.GetQuotedColumnName(alias);
            }

            return sql.Append(text);
        }

        /// <summary>
        /// Adds a COUNT to a SELECT Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO to count.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="fields">Expressions indicating the columns to count.</param>
        /// <returns>The Sql statement.</returns>
        /// <remarks>
        /// <para>If <paramref name="fields"/> is empty, all columns are counted.</para>
        /// </remarks>
        public static Sql<ISqlContext> AndSelectCount<TDto>(this Sql<ISqlContext> sql, params Expression<Func<TDto, object?>>[] fields)
            => sql.AndSelectCount(null, fields);

        /// <summary>
        /// Adds a COUNT to a SELECT Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO to count.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="alias">An alias.</param>
        /// <param name="fields">Expressions indicating the columns to count.</param>
        /// <returns>The Sql statement.</returns>
        /// <remarks>
        /// <para>If <paramref name="fields"/> is empty, all columns are counted.</para>
        /// </remarks>
        public static Sql<ISqlContext> AndSelectCount<TDto>(this Sql<ISqlContext> sql, string? alias = null, params Expression<Func<TDto, object?>>[] fields)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            ISqlSyntaxProvider sqlSyntax = sql.SqlContext.SqlSyntax;
            var columns = fields.Length == 0
                ? sql.GetColumns<TDto>(withAlias: false)
                : fields.Select(x => sqlSyntax.GetFieldName(x)).ToArray();
            var text = ", COUNT (" + string.Join(", ", columns) + ")";
            if (alias != null)
            {
                text += " AS " + sql.SqlContext.SqlSyntax.GetQuotedColumnName(alias);
            }

            return sql.Append(text);
        }

        /// <summary>
        /// Creates a SELECT Sql statement with a referenced Dto.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto to select.</typeparam>
        /// <param name="sql">The origin Sql.</param>
        /// <param name="reference">An expression specifying the reference.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> Select<TDto>(this Sql<ISqlContext> sql, Func<SqlRef<TDto>, SqlRef<TDto>> reference)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            sql.Select(sql.GetColumns<TDto>());

            reference?.Invoke(new SqlRef<TDto>(sql, null));
            return sql;
        }

        /// <summary>
        /// Creates a SELECT Sql statement with a referenced Dto.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto to select.</typeparam>
        /// <param name="sql">The origin Sql.</param>
        /// <param name="reference">An expression specifying the reference.</param>
        /// <param name="sqlexpr">An expression to apply to the Sql statement before adding the reference selection.</param>
        /// <returns>The Sql statement.</returns>
        /// <remarks>The <paramref name="sqlexpr"/> expression applies to the Sql statement before the reference selection
        /// is added, so that it is possible to add (e.g. calculated) columns to the referencing Dto.</remarks>
        public static Sql<ISqlContext> Select<TDto>(this Sql<ISqlContext> sql, Func<SqlRef<TDto>, SqlRef<TDto>> reference, Func<Sql<ISqlContext>, Sql<ISqlContext>> sqlexpr)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            sql.Select(sql.GetColumns<TDto>());

            sql = sqlexpr(sql);

            reference(new SqlRef<TDto>(sql, null));
            return sql;
        }

        /// <summary>
        /// Creates a SELECT CASE WHEN EXISTS query, which returns 1 if the sub query returns any results, and 0 if not.
        /// </summary>
        /// <param name="sql">The original SQL.</param>
        /// <param name="nestedSelect">The nested select to run the query against.</param>
        /// <returns>The updated Sql statement.</returns>
        public static Sql<ISqlContext> SelectAnyIfExists(this Sql<ISqlContext> sql, Sql<ISqlContext> nestedSelect)
        {
            sql.Append("SELECT CASE WHEN EXISTS (");
            sql.Append(nestedSelect);
            sql.Append(")");
            sql.Append("THEN 1 ELSE 0 END");
            return sql;
        }

        /// <summary>
        /// Represents a Dto reference expression.
        /// </summary>
        /// <typeparam name="TDto">The type of the referencing Dto.</typeparam>
        public class SqlRef<TDto>
        {
            /// <summary>
            /// Initializes a new Dto reference expression.
            /// </summary>
            /// <param name="sql">The original Sql expression.</param>
            /// <param name="prefix">The current Dtos prefix.</param>
            public SqlRef(Sql<ISqlContext> sql, string? prefix)
            {
                Sql = sql;
                Prefix = prefix;
            }

            /// <summary>
            /// Gets the original Sql expression.
            /// </summary>
            public Sql<ISqlContext> Sql { get; }

            /// <summary>
            /// Gets the current Dtos prefix.
            /// </summary>
            public string? Prefix { get; }

            /// <summary>
            /// Appends fields for a referenced Dto.
            /// </summary>
            /// <typeparam name="TRefDto">The type of the referenced Dto.</typeparam>
            /// <param name="field">An expression specifying the referencing field.</param>
            /// <param name="reference">An optional expression representing a nested reference selection.</param>
            /// <returns>A SqlRef statement.</returns>
            public SqlRef<TDto> Select<TRefDto>(Expression<Func<TDto, TRefDto>> field, Func<SqlRef<TRefDto>, SqlRef<TRefDto>>? reference = null)
                => Select(field, null, reference);

            /// <summary>
            /// Appends fields for a referenced Dto.
            /// </summary>
            /// <typeparam name="TRefDto">The type of the referenced Dto.</typeparam>
            /// <param name="field">An expression specifying the referencing field.</param>
            /// <param name="tableAlias">The referenced Dto table alias.</param>
            /// <param name="reference">An optional expression representing a nested reference selection.</param>
            /// <returns>A SqlRef statement.</returns>
            public SqlRef<TDto> Select<TRefDto>(Expression<Func<TDto, TRefDto>> field, string? tableAlias, Func<SqlRef<TRefDto>, SqlRef<TRefDto>>? reference = null)
            {
                PropertyInfo? property = field == null ? null : ExpressionHelper.FindProperty(field).Item1 as PropertyInfo;
                return Select(property, tableAlias, reference);
            }

            /// <summary>
            /// Selects referenced DTOs.
            /// </summary>
            /// <typeparam name="TRefDto">The type of the referenced DTOs.</typeparam>
            /// <param name="field">An expression specifying the referencing field.</param>
            /// <param name="reference">An optional expression representing a nested reference selection.</param>
            /// <returns>A referenced DTO expression.</returns>
            /// <remarks>
            /// <para>The referencing property has to be a <c>List{<typeparamref name="TRefDto"/>}</c>.</para>
            /// </remarks>
            public SqlRef<TDto> Select<TRefDto>(Expression<Func<TDto, List<TRefDto>>> field, Func<SqlRef<TRefDto>, SqlRef<TRefDto>>? reference = null)
                => Select(field, null, reference);

            /// <summary>
            /// Selects referenced DTOs.
            /// </summary>
            /// <typeparam name="TRefDto">The type of the referenced DTOs.</typeparam>
            /// <param name="field">An expression specifying the referencing field.</param>
            /// <param name="tableAlias">The DTO table alias.</param>
            /// <param name="reference">An optional expression representing a nested reference selection.</param>
            /// <returns>A referenced DTO expression.</returns>
            /// <remarks>
            /// <para>The referencing property has to be a <c>List{<typeparamref name="TRefDto"/>}</c>.</para>
            /// </remarks>
            public SqlRef<TDto> Select<TRefDto>(Expression<Func<TDto, List<TRefDto>>> field, string? tableAlias, Func<SqlRef<TRefDto>, SqlRef<TRefDto>>? reference = null)
            {
                PropertyInfo? property = field == null ? null : ExpressionHelper.FindProperty(field).Item1 as PropertyInfo;
                return Select(property, tableAlias, reference);
            }

            private SqlRef<TDto> Select<TRefDto>(PropertyInfo? propertyInfo, string? tableAlias, Func<SqlRef<TRefDto>, SqlRef<TRefDto>>? nested = null)
            {
                var referenceName = propertyInfo?.Name ?? typeof (TDto).Name;
                if (Prefix != null)
                {
                    referenceName = Prefix + PocoData.Separator + referenceName;
                }

                var columns = Sql.GetColumns<TRefDto>(tableAlias, referenceName);
                Sql.Append(", " + string.Join(", ", columns));

                nested?.Invoke(new SqlRef<TRefDto>(Sql, referenceName));
                return this;
            }
        }
        /// <summary>
        /// Gets fields for a Dto.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="fields">Expressions specifying the fields.</param>
        /// <returns>The comma-separated list of fields.</returns>
        /// <remarks>
        /// <para>If <paramref name="fields"/> is empty, all fields are selected.</para>
        /// </remarks>
        public static string Columns<TDto>(this Sql<ISqlContext> sql, params Expression<Func<TDto, object?>>[] fields)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            return string.Join(", ", sql.GetColumns(columnExpressions: fields, withAlias: false));
        }

        /// <summary>
        /// Gets fields for a Dto.
        /// </summary>
        public static string ColumnsForInsert<TDto>(this Sql<ISqlContext> sql, params Expression<Func<TDto, object?>>[]? fields)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            return string.Join(", ", sql.GetColumns(columnExpressions: fields, withAlias: false, forInsert: true));
        }

        /// <summary>
        /// Gets fields for a Dto.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="alias">The Dto table alias.</param>
        /// <param name="fields">Expressions specifying the fields.</param>
        /// <returns>The comma-separated list of fields.</returns>
        /// <remarks>
        /// <para>If <paramref name="fields"/> is empty, all fields are selected.</para>
        /// </remarks>
        public static string Columns<TDto>(this Sql<ISqlContext> sql, string alias, params Expression<Func<TDto, object?>>[] fields)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            return string.Join(", ", sql.GetColumns(columnExpressions: fields, withAlias: false, tableAlias: alias));
        }

        #endregion

        #region Delete

        public static Sql<ISqlContext> Delete(this Sql<ISqlContext> sql)
        {
            sql.Append("DELETE");
            return sql;
        }

        public static Sql<ISqlContext> Delete<TDto>(this Sql<ISqlContext> sql)
        {
            Type type = typeof(TDto);
            var tableName = type.GetTableName();

            // FROM optional SQL server, but not elsewhere.
            sql.Append($"DELETE FROM {sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName)}");
            return sql;
        }

        #endregion

        #region Update

        public static Sql<ISqlContext> Update(this Sql<ISqlContext> sql)
        {
            sql.Append("UPDATE");
            return sql;
        }

        public static Sql<ISqlContext> Update<TDto>(this Sql<ISqlContext> sql)
        {
            Type type = typeof(TDto);
            var tableName = type.GetTableName();

            sql.Append($"UPDATE {sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName)}");
            return sql;
        }

        public static Sql<ISqlContext> Update<TDto>(this Sql<ISqlContext> sql, Func<SqlUpd<TDto>, SqlUpd<TDto>> updates)
        {
            Type type = typeof(TDto);
            var tableName = type.GetTableName();

            sql.Append($"UPDATE {sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName)} SET");

            var u = new SqlUpd<TDto>(sql.SqlContext);
            u = updates(u);
            var first = true;
            foreach (Tuple<string, object?> setExpression in u.SetExpressions)
            {
                switch (setExpression.Item2)
                {
                    case null:
                        sql.Append((first ? "" : ",") + " " + setExpression.Item1 + "=NULL");
                        break;
                    case string s when s == string.Empty:
                        sql.Append((first ? "" : ",") + " " + setExpression.Item1 + "=''");
                        break;
                    default:
                        sql.Append((first ? "" : ",") + " " + setExpression.Item1 + "=@0", setExpression.Item2);
                        break;
                }

                first = false;
            }

            if (!first)
            {
                sql.Append(" ");
            }

            return sql;
        }

        public class SqlUpd<TDto>
        {
            private readonly ISqlContext _sqlContext;
            private readonly List<Tuple<string, object?>> _setExpressions = new List<Tuple<string, object?>>();

            public SqlUpd(ISqlContext sqlContext)
            {
                _sqlContext = sqlContext;
            }

            public SqlUpd<TDto> Set(Expression<Func<TDto, object?>> fieldSelector, object? value)
            {
                var fieldName = _sqlContext.SqlSyntax.GetFieldNameForUpdate(fieldSelector);
                _setExpressions.Add(new Tuple<string, object?>(fieldName, value));
                return this;
            }

            public List<Tuple<string, object?>> SetExpressions => _setExpressions;
        }

        #endregion

        #region Hints

        /// <summary>
        /// Appends the relevant ForUpdate hint.
        /// </summary>
        /// <param name="sql">The Sql statement.</param>
        /// <returns>The Sql statement.</returns>
        /// <remarks>
        /// NOTE: This method will not work for all queries, only simple ones!
        /// </remarks>
        public static Sql<ISqlContext> ForUpdate(this Sql<ISqlContext> sql)
            => sql.SqlContext.SqlSyntax.InsertForUpdateHint(sql);

        public static Sql<ISqlContext> AppendForUpdateHint(this Sql<ISqlContext> sql)
            => sql.SqlContext.SqlSyntax.AppendForUpdateHint(sql);

        #endregion

        #region Aliasing

        internal static string GetAliasedField(this Sql<ISqlContext> sql, string field)
        {
            // get alias, if aliased
            //
            // regex looks for pattern "([\w+].[\w+]) AS ([\w+])" ie "(field) AS (alias)"
            // and, if found & a group's field matches the field name, returns the alias
            //
            // so... if query contains "[umbracoNode].[nodeId] AS [umbracoNode__nodeId]"
            // then GetAliased for "[umbracoNode].[nodeId]" returns "[umbracoNode__nodeId]"

            MatchCollection matches = sql.SqlContext.SqlSyntax.AliasRegex.Matches(sql.SQL);
            Match? match = matches.Cast<Match>().FirstOrDefault(m => m.Groups[1].Value.InvariantEquals(field));
            return match == null ? field : match.Groups[2].Value;
        }

        #endregion

        #region Utilities

        private static string[] GetColumns<TDto>(this Sql<ISqlContext> sql, string? tableAlias = null, string? referenceName = null, Expression<Func<TDto, object?>>[]? columnExpressions = null, bool withAlias = true, bool forInsert = false)
        {
            PocoData? pd = sql.SqlContext.PocoDataFactory.ForType(typeof (TDto));
            var tableName = tableAlias ?? pd.TableInfo.TableName;
            var queryColumns = pd.QueryColumns.ToList();

            Dictionary<string, string>? aliases = null;

            if (columnExpressions != null && columnExpressions.Length > 0)
            {
                var names = columnExpressions.Select(x =>
                {
                    (MemberInfo member, var alias) = ExpressionHelper.FindProperty(x);
                    var field = member as PropertyInfo;
                    var fieldName = field?.GetColumnName();
                    if (alias != null && fieldName is not null)
                    {
                        if (aliases == null)
                        {
                            aliases = new Dictionary<string, string>();
                        }

                        aliases[fieldName] = alias;
                    }
                    return fieldName;
                }).ToArray();

                //only get the columns that exist in the selected names
                queryColumns = queryColumns.Where(x => names.Contains(x.Key)).ToList();

                //ensure the order of the columns in the expressions is the order in the result
                queryColumns.Sort((a, b) => names.IndexOf(a.Key).CompareTo(names.IndexOf(b.Key)));
            }

            string? GetAlias(PocoColumn column)
            {
                if (aliases != null && aliases.TryGetValue(column.ColumnName, out var alias))
                {
                    return alias;
                }

                return withAlias ? (string.IsNullOrEmpty(column.ColumnAlias) ? column.MemberInfoKey : column.ColumnAlias) : null;
            }

            return queryColumns
                .Select(x => sql.SqlContext.SqlSyntax.GetColumn(sql.SqlContext.DatabaseType, tableName, x.Value.ColumnName, GetAlias(x.Value)!, referenceName, forInsert: forInsert))
                .ToArray();
        }

        public static string GetTableName(this Type type)
        {
            // TODO: returning string.Empty for now
            // BUT the code bits that calls this method cannot deal with string.Empty so we
            // should either throw, or fix these code bits...
            TableNameAttribute? attr = type.FirstAttribute<TableNameAttribute>();
            return string.IsNullOrWhiteSpace(attr?.Value) ? string.Empty : attr.Value;
        }

        private static string GetColumnName(this PropertyInfo column)
        {
            ColumnAttribute? attr = column.FirstAttribute<ColumnAttribute>();
            return string.IsNullOrWhiteSpace(attr?.Name) ? column.Name : attr.Name;
        }

        public static string ToText(this Sql sql)
        {
            var text = new StringBuilder();
            sql.ToText(text);
            return text.ToString();
        }

        public static void ToText(this Sql sql, StringBuilder text)
        {
            ToText(sql.SQL, sql.Arguments, text);
        }

        public static void ToText(string? sql, object[]? arguments, StringBuilder text)
        {
            text.AppendLine(sql);

            if (arguments == null || arguments.Length == 0)
            {
                return;
            }

            text.Append(" --");

            var i = 0;
            foreach (var arg in arguments)
            {
                text.Append(" @");
                text.Append(i++);
                text.Append(":");
                text.Append(arg);
            }

            text.AppendLine();
        }

        #endregion
    }
}
