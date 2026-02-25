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
    /// <summary>
    /// Provides extension methods to enhance or simplify NPoco SQL operations.
    /// </summary>
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
            (string s, object[] a) = sql.SqlContext.VisitDto(predicate, alias);
            return sql.Where(s, a);
        }

        /// <summary>
        /// Adds a WHERE clause to the SQL query based on the specified predicate and optional alias,  and appends a
        /// closing parenthesis to the query. This is used for selects within "WHERE [column] IN (SELECT ...)" statements.
        /// </summary>
        /// <typeparam name="TDto">The type of the data transfer object (DTO) used to define the predicate.</typeparam>
        /// <param name="sql">The SQL query to which the WHERE clause will be added.</param>
        /// <param name="predicate">An expression that defines the condition for the WHERE clause.</param>
        /// <param name="alias">An optional alias to qualify the table or entity in the query. If null, no alias is used.</param>
        /// <returns>The modified SQL query with the appended WHERE clause and closing parenthesis.</returns>
        public static Sql<ISqlContext> WhereClosure<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, bool>> predicate, string? alias = null)
        {
            return sql.Where<TDto>(predicate, alias).Append(")");
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
            (string s, object[] a) = sql.SqlContext.VisitDto(predicate, alias1, alias2);
            return sql.Where(s, a);
        }

        /// <summary>
        /// Appends a WHERE clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto1">The type of Dto 1.</typeparam>
        /// <typeparam name="TDto2">The type of Dto 2.</typeparam>
        /// <typeparam name="TDto3">The type of Dto 3.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="predicate">A predicate to transform and append to the Sql statement.</param>
        /// <param name="alias1">An optional alias for Dto 1 table.</param>
        /// <param name="alias2">An optional alias for Dto 2 table.</param>
        /// <param name="alias3">An optional alias for Dto 3 table.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> Where<TDto1, TDto2, TDto3>(this Sql<ISqlContext> sql, Expression<Func<TDto1, TDto2, TDto3, bool>> predicate, string? alias1 = null, string? alias2 = null, string? alias3 = null)
        {
            var (s, a) = sql.SqlContext.VisitDto(predicate, alias1, alias2, alias3);
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
            sql.Where($"{fieldName} IN (@values)", new { values });
            return sql;
        }

        /// <summary>
        /// Appends a OR IN clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the first Dto.</typeparam>
        /// <typeparam name="TDto2">The type of the second Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="field">An expression specifying the first field.</param>
        /// <param name="values">First values.</param>
        /// <param name="field2">An expression specifying the second field.</param>
        /// <param name="values2">Second values.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> WhereInOr<TDto, TDto2>(
            this Sql<ISqlContext> sql,
            Expression<Func<TDto, object?>> field,
            Expression<Func<TDto2, object?>> field2,
            IEnumerable? values,
            IEnumerable? values2)
        {
            var fieldName = sql.SqlContext.SqlSyntax.GetFieldName(field);
            var fieldName2 = sql.SqlContext.SqlSyntax.GetFieldName(field2);
            sql.Where($"{fieldName} IN (@values) OR {fieldName2} IN (@values2)", new { values }, new { values2 });
            return sql;
        }

        /// <summary>
        /// Appends a WHERE IN clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="field">An expression specifying the field.</param>
        /// <param name="values">The values.</param>
        /// <param name="alias">The table alias for the field.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> WhereIn<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field, IEnumerable? values, string alias)
        {
            var fieldName = sql.SqlContext.SqlSyntax.GetFieldName(field, alias);
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

        /// <summary>
        /// Appends a WHERE IN clause to the specified <paramref name="sql"/> statement, filtering the results where the given field matches any of the provided values.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO (data transfer object) being queried.</typeparam>
        /// <param name="sql">The SQL statement to which the WHERE IN clause will be appended.</param>
        /// <param name="field">An expression specifying the field to filter on.</param>
        /// <param name="values">The collection of values to include in the IN clause.</param>
        /// <param name="tableAlias">The alias of the table to use when qualifying the field in the WHERE IN clause.</param>
        /// <returns>The modified SQL statement with the appended WHERE IN clause.</returns>
        public static Sql<ISqlContext> WhereIn<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field, Sql<ISqlContext>? values, string tableAlias)
        {
            return sql.WhereIn(field, values, false, tableAlias);
        }

        /// <summary>
        /// This builds a WHERE clause with a LIKE statement where the value is built from a subquery.
        /// E.g. for SQL Server: WHERE [field] LIKE CONCAT((SELECT [value] FROM ...), '%').
        /// Or SQLite : WHERE [field] LIKE ((SELECT [value] FROM ...) || '%').
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="fieldSelector">An expression specifying the field.</param>
        /// <param name="valuesSql">The sql object to select the values.</param>
        /// <param name="concatDefault">If ommitted or empty the specific wildcard char is used as suffix for the resulting values from valueSql query.</param>
        /// <returns></returns>
        public static Sql<ISqlContext> WhereLike<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> fieldSelector, Sql<ISqlContext>? valuesSql, string concatDefault = "")
        {
            var fieldName = sql.SqlContext.SqlSyntax.GetFieldName(fieldSelector);
            var concat = sql.SqlContext.SqlSyntax.GetWildcardConcat(concatDefault);
            var likeSelect = sql.SqlContext.SqlSyntax.GetConcat($"({valuesSql?.SQL})", concat);
            sql.Where($"{fieldName} LIKE {likeSelect}", valuesSql?.Arguments);
            return sql;
        }

        /// <summary>
        /// Returns a new SQL query that combines the results of two specified SQL queries using the UNION operator.
        /// </summary>
        /// <param name="sql">The first SQL query.</param>
        /// <param name="sql2">The second SQL query to combine with the first using UNION.</param>
        /// <returns>A <see cref="Sql{ISqlContext}"/> representing the union of the two queries.</returns>
        public static Sql<ISqlContext> Union(this Sql<ISqlContext> sql, Sql<ISqlContext> sql2)
        {
            return sql.Append(" UNION ").Append(sql2);
        }

        /// <summary>
        /// Appends an INNER JOIN clause to the current SQL query, joining with a nested SQL subquery and assigning it an alias.
        /// </summary>
        /// <param name="sql">The base <see cref="Sql{ISqlContext}"/> instance to which the join will be appended.</param>
        /// <param name="nestedQuery">The nested <see cref="Sql{ISqlContext}"/> subquery to join.</param>
        /// <param name="alias">The alias to assign to the nested subquery in the join clause. The alias will be quoted according to the SQL syntax provider.</param>
        /// <returns>A <see cref="SqlJoinClause{ISqlContext}"/> representing the INNER JOIN with the nested subquery and alias.</returns>
        public static Sql<ISqlContext>.SqlJoinClause<ISqlContext> InnerJoinNested(this Sql<ISqlContext> sql, Sql<ISqlContext> nestedQuery, string alias)
        {
            return new Sql<ISqlContext>.SqlJoinClause<ISqlContext>(sql.Append("INNER JOIN (").Append(nestedQuery)
                .Append($") {sql.SqlContext.SqlSyntax.GetQuotedName(alias)}"));
        }

        /// <summary>
        /// Adds a WHERE clause to the SQL query that filters results based on a LIKE comparison for the specified
        /// field.
        /// </summary>
        /// <remarks>Use this method to perform pattern matching queries, such as searching for records
        /// where a field contains, starts with, or ends with a specified substring. The method does not automatically
        /// add wildcard characters; include them in the likeValue parameter as needed.</remarks>
        /// <typeparam name="TDto">The type of the data transfer object representing the table or entity being queried.</typeparam>
        /// <param name="sql">The SQL builder instance to which the WHERE clause will be appended.</param>
        /// <param name="fieldSelector">An expression that selects the field of the entity to apply the LIKE filter to.</param>
        /// <param name="likeValue">The value to use in the LIKE comparison. This can include SQL wildcard characters such as '%' or '_'.</param>
        /// <returns>The updated SQL builder instance with the appended WHERE LIKE clause.</returns>
        public static Sql<ISqlContext> WhereLike<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> fieldSelector, string likeValue)
        {
            var fieldName = sql.SqlContext.SqlSyntax.GetFieldName(fieldSelector);
            sql.Where($"{fieldName} LIKE @0", likeValue);
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
            => sql.WhereIn(field, values, true);

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
            sql.Where(fieldName + (not ? " NOT" : string.Empty) + " IN (" + valuesSql?.SQL + ")", valuesSql?.Arguments);
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
            var column = sql.GetColumns(columnExpressions: [field], tableAlias: tableAlias, withAlias: false).First();
            return sql.Where($"({column} IS {(not ? "NOT " : string.Empty)}NULL)");
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
            Type type = typeof(TDto);
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

        /// <summary>
        /// Appends an ORDER BY clause to the Sql statement using the specified field and alias.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="field">An expression specifying the field.</param>
        /// <param name="alias">The alias to use for the field.</param>
        /// <returns>The Sql statement.</returns>
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
        /// Appends a GROUP BY clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="tableAlias">A table alias.</param>
        /// <param name="fields">Expression specifying the fields.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> GroupBy<TDto>(
            this Sql<ISqlContext> sql,
            string tableAlias,
            params Expression<Func<TDto, object?>>[] fields)
        {
            ISqlSyntaxProvider sqlSyntax = sql.SqlContext.SqlSyntax;
            var columns = fields.Length == 0
                ? sql.GetColumns<TDto>(withAlias: false)
                : fields.Select(x => sqlSyntax.GetFieldName(x, tableAlias)).ToArray();
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
        /// Appends additional fields to an existing ORDER BY or GROUP BY clause in the SQL statement, using the specified table alias.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO (data transfer object).</typeparam>
        /// <param name="sql">The SQL statement to append fields to.</param>
        /// <param name="tableAlias">The alias of the table to use for the fields.</param>
        /// <param name="fields">Expressions specifying the fields to append.</param>
        /// <returns>A <see cref="Sql{ISqlContext}"/> statement with the additional fields appended to the ORDER BY or GROUP BY clause.</returns>
        public static Sql<ISqlContext> AndBy<TDto>(
            this Sql<ISqlContext> sql,
            string tableAlias,
            params Expression<Func<TDto, object?>>[] fields)
        {
            ISqlSyntaxProvider sqlSyntax = sql.SqlContext.SqlSyntax;
            var columns = fields.Length == 0
                ? sql.GetColumns<TDto>(withAlias: false)
                : fields.Select(x => sqlSyntax.GetFieldName(x, tableAlias)).ToArray();
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

            sql.Append("LEFT JOIN " + join, nestedSelect.Arguments);
            return new Sql<ISqlContext>.SqlJoinClause<ISqlContext>(sql);
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
        public static Sql<ISqlContext> On<TLeft, TRight>(
            this Sql<ISqlContext>.SqlJoinClause<ISqlContext> sqlJoin,
            Expression<Func<TLeft, object?>> leftField,
            Expression<Func<TRight, object?>> rightField)
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
        /// Adds a SQL SELECT statement to retrieve the maximum value of the specified field from the table associated
        /// with the specified DTO type.
        /// </summary>
        /// <typeparam name="TDto">The type of the Data Transfer Object (DTO) that represents the table from which the maximum value will be
        /// selected.</typeparam>
        /// <param name="sql">The SQL query builder to which the SELECT statement will be appended. Cannot be <see langword="null"/>.</param>
        /// <param name="field">An expression specifying the field for which the maximum value will be calculated. Cannot be <see
        /// langword="null"/>.</param>
        /// <returns>A modified SQL query builder that includes the SELECT statement for the maximum value of the specified
        /// field.</returns>
        public static Sql<ISqlContext> SelectMax<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field)
        {
            ArgumentNullException.ThrowIfNull(sql);
            ArgumentNullException.ThrowIfNull(field);

            return sql.Select($"MAX ({sql.SqlContext.SqlSyntax.GetFieldName(field)})");
        }

        /// <summary>
        /// Adds a SQL SELECT statement to retrieve the maximum value of the specified field from the table associated
        /// with the specified DTO type.
        /// </summary>
        /// <typeparam name="TDto">The type of the Data Transfer Object (DTO) that represents the table from which the maximum value will be
        /// selected.</typeparam>
        /// <param name="sql">The SQL query builder to which the SELECT statement will be appended. Cannot be <see langword="null"/>.</param>
        /// <param name="field">An expression specifying the field for which the maximum value will be calculated. Cannot be <see
        /// langword="null"/>.</param>
        /// <param name="coalesceValue">COALESCE int value.</param>
        /// <returns>A modified SQL query builder that includes the SELECT statement for the maximum value of the specified
        /// field or the coalesceValue.</returns>
        public static Sql<ISqlContext> SelectMax<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field, int coalesceValue)
        {
            ArgumentNullException.ThrowIfNull(sql);
            ArgumentNullException.ThrowIfNull(field);

            return sql.Select($"COALESCE(MAX ({sql.SqlContext.SqlSyntax.GetFieldName(field)}), {coalesceValue})");
        }

        /// <summary>
        /// Adds a SQL SELECT statement to retrieve the sum of the values of the specified field from the table associated
        /// with the specified DTO type.
        /// </summary>
        /// <typeparam name="TDto">The type of the Data Transfer Object (DTO) that represents the table from which the maximum value will be
        /// selected.</typeparam>
        /// <param name="sql">The SQL query builder to which the SELECT statement will be appended. Cannot be <see langword="null"/>.</param>
        /// <param name="field">An expression specifying the field for which the maximum value will be calculated. Cannot be <see
        /// langword="null"/>.</param>
        /// <returns>A modified SQL query builder that includes the SELECT statement for the maximum value of the specified
        /// field.</returns>
        public static Sql<ISqlContext> SelectSum<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field)
        {
            ArgumentNullException.ThrowIfNull(sql);
            ArgumentNullException.ThrowIfNull(field);

            return sql.Select($"SUM ({sql.SqlContext.SqlSyntax.GetFieldName(field)})");
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
                text += " AS " + sqlSyntax.GetQuotedColumnName(alias);
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

        /// <summary>
        /// Creates a SELECT DISTINCT Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO to select.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="tableAlias">A table alias.</param>
        /// <param name="fields">Expressions indicating the columns to select.</param>
        /// <returns>The Sql statement.</returns>
        /// <remarks>
        /// <para>If <paramref name="fields"/> is empty, all columns are selected.</para>
        /// </remarks>
        public static Sql<ISqlContext> SelectDistinct<TDto>(this Sql<ISqlContext> sql, string tableAlias, params Expression<Func<TDto, object?>>[] fields)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            var columns = sql.GetColumns(tableAlias: tableAlias, columnExpressions: fields);
            sql.Append("SELECT DISTINCT " + string.Join(", ", columns));
            return sql;
        }

        /// <summary>
        /// Creates a <c>SELECT DISTINCT</c> SQL statement for the specified columns.
        /// </summary>
        /// <param name="sql">The original SQL statement to extend.</param>
        /// <param name="columns">The columns to select distinct values from.</param>
        /// <returns>A new <see cref="Sql{ISqlContext}"/> statement with the <c>SELECT DISTINCT</c> clause applied.</returns>
        /// <remarks>
        /// If <paramref name="columns"/> is empty, all columns are selected.
        /// </remarks>
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
                var referenceName = propertyInfo?.Name ?? typeof(TDto).Name;
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

        /// <summary>
        /// Adds a SELECT clause to the SQL query based on the specified conversion function and optional alias, and prepends an
        /// opening parenthesis to the query. This is typically used for subqueries within "WHERE [column] IN (SELECT ...)" statements.
        /// </summary>
        /// <param name="sql">The SQL query to modify by adding a SELECT clause and opening parenthesis.</param>
        /// <param name="converts">A function that configures the columns or expressions to be selected in the subquery.</param>
        /// <returns>The modified SQL query with the prepended SELECT clause and opening parenthesis.</returns>
        public static Sql<ISqlContext> SelectClosure<TDto>(this Sql<ISqlContext> sql, Func<SqlConvert<TDto>, SqlConvert<TDto>> converts)
        {
            sql.Append($"(SELECT ");

            var c = new SqlConvert<TDto>(sql.SqlContext);
            c = converts(c);
            var first = true;
            foreach (string setExpression in c.SetExpressions)
            {
                sql.Append($"{(first ? string.Empty : ",")} {setExpression}");
                first = false;
            }

            if (!first)
            {
                sql.Append(" ");
            }

            return sql;
        }

        /// <summary>
        /// Converts the specified SQL query to a typed SQL query for the given DTO type.
        /// This extension method enables strongly-typed access to query results.
        /// </summary>
        public class SqlConvert<TDto>(ISqlContext sqlContext)
        {
            /// <summary>
            /// Gets the collection of SQL SET expressions generated for the current DTO type.
            /// </summary>
            public List<string> SetExpressions { get; } = [];

            /// <summary>
            /// Adds an expression to convert a unique identifier (GUID) field to its string representation within the SQL query for the specified DTO.
            /// </summary>
            /// <param name="fieldSelector">An expression that selects the unique identifier field to convert.</param>
            /// <returns>The current <see cref="SqlConvert{TDto}"/> instance, allowing for method chaining.</returns>
            public SqlConvert<TDto> ConvertUniqueIdentifierToString(Expression<Func<TDto, object?>> fieldSelector)
            {
                var fieldName = sqlContext.SqlSyntax.GetFieldNameForUpdate(fieldSelector);
                var convertFieldName = string.Format(sqlContext.SqlSyntax.ConvertUniqueIdentifierToString, fieldName);
                SetExpressions.Add(convertFieldName);
                return this;
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// Appends a <c>DELETE</c> statement to the specified SQL query.
        /// </summary>
        /// <param name="sql">The <see cref="Sql{ISqlContext}"/> instance to append the <c>DELETE</c> statement to.</param>
        /// <returns>The same <see cref="Sql{ISqlContext}"/> instance with the <c>DELETE</c> statement appended.</returns>
        public static Sql<ISqlContext> Delete(this Sql<ISqlContext> sql)
        {
            sql.Append("DELETE");
            return sql;
        }

        /// <summary>
        /// Appends a <c>DELETE</c> statement to the specified SQL query.
        /// </summary>
        /// <param name="sql">The <see cref="Sql{ISqlContext}"/> instance to append the <c>DELETE</c> statement to.</param>
        /// <returns>The same <see cref="Sql{ISqlContext}"/> instance with the <c>DELETE</c> statement appended.</returns>
        public static Sql<ISqlContext> Delete<TDto>(this Sql<ISqlContext> sql)
        {
            Type type = typeof(TDto);
            var tableName = type.GetTableName();

            // FROM optional SQL server, but not elsewhere.
            sql.Append($"DELETE FROM {sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName)}");
            return sql;
        }

        /// <summary>
        /// Deletes records from a table based on a predicate.
        /// </summary>
        /// <typeparam name="TDto">Table definition.</typeparam>
        /// <param name="sql">SqlConext</param>
        /// <param name="predicate">A predicate to transform and append to the Sql statement (WHERE clause).</param>
        /// <returns></returns>
        public static Sql<ISqlContext> Delete<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, bool>> predicate)
        {
            (string s, object[] a) = sql.SqlContext.VisitDto(predicate);
            return sql.Delete<TDto>().Where(s, a);
        }

        #endregion

        #region Update

        /// <summary>
        /// Appends the <c>UPDATE</c> keyword to the current SQL query.
        /// </summary>
        /// <param name="sql">The <see cref="Sql{ISqlContext}"/> instance to append the <c>UPDATE</c> keyword to.</param>
        /// <returns>The same <see cref="Sql{ISqlContext}"/> instance with the <c>UPDATE</c> keyword appended.</returns>
        public static Sql<ISqlContext> Update(this Sql<ISqlContext> sql)
        {
            sql.Append("UPDATE");
            return sql;
        }

        /// <summary>
        /// Appends an <c>UPDATE</c> statement to the specified SQL query.
        /// </summary>
        /// <typeparam name="TDto">The type representing the data transfer object (DTO) for the update operation.</typeparam>
        /// <param name="sql">The SQL query to which the <c>UPDATE</c> statement will be appended.</param>
        /// <returns>The <see cref="Sql{ISqlContext}"/> instance with the appended <c>UPDATE</c> statement.</returns>
        public static Sql<ISqlContext> Update<TDto>(this Sql<ISqlContext> sql)
        {
            Type type = typeof(TDto);
            var tableName = type.GetTableName();

            sql.Append($"UPDATE {sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName)}");
            return sql;
        }

        /// <summary>
        /// Appends an UPDATE statement to the specified SQL query.
        /// </summary>
        /// <param name="sql">The SQL query to which the UPDATE statement will be appended.</param>
        /// <returns>The SQL query with the appended UPDATE statement.</returns
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
                        sql.Append((first ? string.Empty : ",") + " " + setExpression.Item1 + "=NULL");
                        break;
                    case string s when s == string.Empty:
                        sql.Append((first ? string.Empty : ",") + " " + setExpression.Item1 + "=''");
                        break;
                    default:
                        sql.Append((first ? string.Empty : ",") + " " + setExpression.Item1 + "=@0", setExpression.Item2);
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

        /// <summary>
        /// Creates an SQL UPDATE statement for the specified data transfer object (DTO) type.
        /// </summary>
        public class SqlUpd<TDto>
        {
            private readonly ISqlContext _sqlContext;

            /// <summary>
            /// Initializes a new instance of the <see cref="SqlUpd{TDto}"/> class for the specified DTO type.
            /// </summary>
            /// <param name="sqlContext">The <see cref="ISqlContext"/> to use for SQL operations.</param>
            public SqlUpd(ISqlContext sqlContext)
            {
                _sqlContext = sqlContext;
            }

            /// <summary>
            /// Sets the specified field to the given value for the update operation.
            /// </summary>
            /// <param name="fieldSelector">An expression selecting the field to update.</param>
            /// <param name="value">The value to assign to the selected field.</param>
            /// <returns>The current <see cref="Umbraco.Extensions.NPocoSqlExtensions.SqlUpd{TDto}"/> instance for chaining.</returns>
            public SqlUpd<TDto> Set(Expression<Func<TDto, object?>> fieldSelector, object? value)
            {
                var fieldName = _sqlContext.SqlSyntax.GetFieldNameForUpdate(fieldSelector);
                SetExpressions.Add(new Tuple<string, object?>(fieldName, value));
                return this;
            }

            /// <summary>
            /// Gets the list of set expressions used in the SQL update statement.
            /// Each tuple contains the column name and the value to set for that column.
            /// </summary>
            public List<Tuple<string, object?>> SetExpressions { get; } = [];
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

        /// <summary>
        /// Appends an update hint to the specified <see cref="Sql{ISqlContext}"/> query, indicating that the query is intended for updating records.
        /// </summary>
        /// <param name="sql">The SQL query object to which the update hint will be appended.</param>
        /// <returns>A new <see cref="Sql{ISqlContext}"/> instance with the update hint applied.</returns>
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

        /// <summary>
        /// Appends a subquery as a derived table with the specified alias to the given SQL query.
        /// </summary>
        /// <param name="sql">The original SQL query to append to.</param>
        /// <param name="subQuery">The subquery to append as a derived table.</param>
        /// <param name="alias">The alias to use for the derived table.</param>
        /// <returns>The modified SQL query with the appended subquery.</returns>
        public static Sql<ISqlContext> AppendSubQuery(this Sql<ISqlContext> sql, Sql<ISqlContext> subQuery, string alias)
        {
            // Append the subquery as a derived table with an alias
            sql.Append("(").Append(subQuery.SQL, subQuery.Arguments).Append($") AS {sql.SqlContext.SqlSyntax.GetQuotedName(alias)}");

            return sql;
        }

        private static string[] GetColumns<TDto>(this Sql<ISqlContext> sql, string? tableAlias = null, string? referenceName = null, Expression<Func<TDto, object?>>[]? columnExpressions = null, bool withAlias = true, bool forInsert = false)
        {
            PocoData? pd = sql.SqlContext.PocoDataFactory.ForType(typeof(TDto));
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
                        aliases ??= new Dictionary<string, string>();
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

        /// <summary>
        /// Returns the table name specified by the <see cref="TableNameAttribute"/> on the given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> whose table name is to be retrieved.</param>
        /// <returns>The table name as a <see cref="string"/>, or <c>string.Empty</c> if the attribute is not present or its value is null or whitespace.</returns>
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

        /// <summary>
        /// Returns the SQL query string representation of the specified <see cref="Sql"/> object.
        /// </summary>
        /// <param name="sql">The <see cref="Sql"/> instance to convert to a SQL string.</param>
        /// <returns>A string containing the textual SQL query.</returns>
        public static string ToText(this Sql sql)
        {
            var text = new StringBuilder();
            sql.ToText(text);
            return text.ToString();
        }

        /// <summary>
        /// Appends the SQL query text represented by the specified <see cref="Sql"/> object to the given <see cref="StringBuilder"/> instance.
        /// This method does not return the SQL text; instead, it appends the generated SQL to the provided <paramref name="text"/> parameter.
        /// </summary>
        /// <param name="sql">The <see cref="Sql"/> object containing the SQL query to convert to text.</param>
        /// <param name="text">The <see cref="StringBuilder"/> instance to which the SQL text will be appended.</param>
        public static void ToText(this Sql sql, StringBuilder text)
        {
            ToText(sql.SQL, sql.Arguments, text);
        }

        /// <summary>
        /// Appends a textual representation of the specified SQL query and its arguments to the provided <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="sql">The SQL query string to be represented as text.</param>
        /// <param name="arguments">The arguments to be included in the textual representation of the SQL query.</param>
        /// <param name="text">The <see cref="StringBuilder"/> instance to which the textual representation will be appended.</param>

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
