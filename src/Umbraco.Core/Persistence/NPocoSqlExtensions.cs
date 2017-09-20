using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NPoco;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    public static class NPocoSqlExtensions
    {
        // note: here we take benefit from the fact that NPoco methods that return a Sql, such as
        // when doing "sql = sql.Where(...)" actually append to, and return, the original Sql, not
        // a new one.

        #region Where

        public static Sql<SqlContext> Where<T>(this Sql<SqlContext> sql, Expression<Func<T, bool>> predicate)
        {
            var expresionist = new PocoToSqlExpressionVisitor<T>(sql.SqlContext);
            var whereExpression = expresionist.Visit(predicate);
            sql.Where(whereExpression, expresionist.GetSqlParameters());
            return sql;
        }

        public static Sql<SqlContext> WhereIn<T>(this Sql<SqlContext> sql, Expression<Func<T, object>> fieldSelector, IEnumerable values)
        {
            var fieldName = GetFieldName(fieldSelector, sql.SqlContext.SqlSyntax);
            sql.Where(fieldName + " IN (@values)", new { values });
            return sql;
        }

        public static Sql WhereAnyIn<TDto>(this Sql<SqlContext> sql, Expression<Func<TDto, object>>[] fieldSelectors, IEnumerable values)
        {
            var fieldNames = fieldSelectors.Select(x => GetFieldName(x, sql.SqlContext.SqlSyntax)).ToArray();
            var sb = new StringBuilder();
            sb.Append("(");
            for (var i = 0; i < fieldNames.Length; i++)
            {
                if (i > 0) sb.Append(" OR ");
                sb.Append(fieldNames[i]);
                sql.Append(" IN (@values)");
            }
            sb.Append(")");
            sql.Where(sb.ToString(), new { values });
            return sql;
        }

        public static Sql<SqlContext> WhereIn<T>(this Sql<SqlContext> sql, Expression<Func<T, object>> fieldSelector, Sql<SqlContext> inSql)
        {
            return sql.WhereIn(fieldSelector, inSql, false);
        }

        public static Sql<SqlContext> WhereNotIn<T>(this Sql<SqlContext> sql, Expression<Func<T, object>> fieldSelector, Sql<SqlContext> inSql)
        {
            return sql.WhereIn(fieldSelector, inSql, true);
        }

        private static Sql<SqlContext> WhereIn<T>(this Sql<SqlContext> sql, Expression<Func<T, object>> fieldSelector, Sql<SqlContext> inSql, bool not)
        {
            var fieldName = GetFieldName(fieldSelector, sql.SqlContext.SqlSyntax);
            sql.Where(fieldName + (not ? " NOT" : "") +" IN (" + inSql.SQL + ")"); // fixme what about args?
            return sql;
        }

        #endregion

        #region From

        public static Sql<SqlContext> From<T>(this Sql<SqlContext> sql)
        {
            var type = typeof (T);
            var tableName = type.GetTableName();

            sql.From(sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName));
            return sql;
        }

        #endregion

        #region OrderBy, GroupBy

        public static Sql<SqlContext> OrderBy<T>(this Sql<SqlContext> sql, Expression<Func<T, object>> columnMember)
        {
            var syntax = "(" + GetFieldName(columnMember, sql.SqlContext.SqlSyntax) + ")";
            sql.OrderBy(syntax);
            return sql;
        }

        public static Sql<SqlContext> OrderByDescending<T>(this Sql<SqlContext> sql, Expression<Func<T, object>> columnMember)
        {
            var syntax = "(" + GetFieldName(columnMember, sql.SqlContext.SqlSyntax) + ") DESC";
            sql.OrderBy(syntax);
            return sql;
        }

        public static Sql<SqlContext> OrderByDescending(this Sql<SqlContext> sql, params object[] columns)
        {
            sql.Append("ORDER BY " + string.Join(", ", columns.Select(x => x + " DESC")));
            return sql;
        }

        public static Sql<SqlContext> GroupBy<T>(this Sql<SqlContext> sql, Expression<Func<T, object>> columnMember)
        {
            var column = ExpressionHelper.FindProperty(columnMember) as PropertyInfo;
            var columnName = column.GetColumnName();

            sql.GroupBy(sql.SqlContext.SqlSyntax.GetQuotedColumnName(columnName));
            return sql;
        }

        #endregion

        #region Joins

        public static Sql<SqlContext>.SqlJoinClause<SqlContext>  InnerJoin<T>(this Sql<SqlContext> sql)
        {
            var type = typeof(T);
            var tableName = type.GetTableName();

            return sql.InnerJoin(sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName));
        }

        public static Sql<SqlContext>.SqlJoinClause<SqlContext> LeftJoin<T>(this Sql<SqlContext> sql)
        {
            var type = typeof(T);
            var tableName = type.GetTableName();

            return sql.LeftJoin(sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName));
        }

        public static Sql<SqlContext>.SqlJoinClause<SqlContext> RightJoin<T>(this Sql<SqlContext> sql)
        {
            var type = typeof(T);
            var tableName = type.GetTableName();

            return sql.RightJoin(sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName));
        }

        public static Sql<SqlContext> On<TLeft, TRight>(this Sql<SqlContext>.SqlJoinClause<SqlContext> clause,
            Expression<Func<TLeft, object>> leftMember, Expression<Func<TRight, object>> rightMember,
            params object[] args)
        {
            var sqlSyntax = clause.SqlContext.SqlSyntax;

            var leftType = typeof (TLeft);
            var rightType = typeof (TRight);
            var leftTableName = leftType.GetTableName();
            var rightTableName = rightType.GetTableName();

            var leftColumn = ExpressionHelper.FindProperty(leftMember) as PropertyInfo;
            var rightColumn = ExpressionHelper.FindProperty(rightMember) as PropertyInfo;

            var leftColumnName = leftColumn.GetColumnName();
            var rightColumnName = rightColumn.GetColumnName();

            string onClause = $"{sqlSyntax.GetQuotedTableName(leftTableName)}.{sqlSyntax.GetQuotedColumnName(leftColumnName)} = {sqlSyntax.GetQuotedTableName(rightTableName)}.{sqlSyntax.GetQuotedColumnName(rightColumnName)}";
            return clause.On(onClause);
        }

        #endregion

        #region Select

        public static Sql<SqlContext> SelectTop(this Sql<SqlContext> sql, int count)
        {
            return sql.SqlContext.SqlSyntax.SelectTop(sql, count);
        }

        public static Sql<SqlContext> SelectCount(this Sql<SqlContext> sql)
        {
            sql.Select("COUNT(*)");
            return sql;
        }

        public static Sql<SqlContext> SelectAll(this Sql<SqlContext> sql)
        {
            sql.Select("*");
            return sql;
        }

        /// <summary>
        /// Creates a SELECT Sql statement.
        /// </summary>
        /// <typeparam name="T">The type of the DTO to select.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="refexpr">An optional reference Sql expression.</param>
        /// <returns>The Sql statement.</returns>
        /// <remarks>
        /// <para>Use <paramref name="refexpr"/> to select referenced DTOs.</para>
        /// </remarks>
        public static Sql<SqlContext> Select<T>(this Sql<SqlContext> sql, Func<RefSql, RefSql> refexpr = null)
        {
            sql.Select(sql.GetColumns<T>());

            if (refexpr == null) return sql;
            refexpr(new RefSql(sql, null));
            return sql;
        }

        public static Sql<SqlContext> Zelect<TDto>(this Sql<SqlContext> sql, Func<RefSql<TDto>, RefSql<TDto>> refexpr = null)
        {
            sql.Select(sql.GetColumns<TDto>());

            if (refexpr == null) return sql;
            refexpr(new RefSql<TDto>(sql, null));
            return sql;
        }

        /// <summary>
        /// Creates a SELECT Sql statement.
        /// </summary>
        /// <typeparam name="T">The type of the DTO to select.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="sqlexpr">A selection Sql expression.</param>
        /// <param name="refexpr">A reference Sql expression.</param>
        /// <returns>The Sql statement.</returns>
        /// <remarks>
        /// <para>Use <paramref name="sqlexpr"/> to complement the selection..</para>
        /// <para>Use <paramref name="refexpr"/> to select referenced DTOs.</para>
        /// </remarks>
        public static Sql<SqlContext> Select<T>(this Sql<SqlContext> sql, Func<Sql<SqlContext>, Sql<SqlContext>> sqlexpr, Func<RefSql, RefSql> refexpr)
        {
            sql.Select(sql.GetColumns<T>());

            sql = sqlexpr(sql);

            refexpr(new RefSql(sql, null));
            return sql;
        }

        public static Sql<SqlContext> Zelect<TDto>(this Sql<SqlContext> sql, Func<Sql<SqlContext>, Sql<SqlContext>> sqlexpr, Func<RefSql<TDto>, RefSql<TDto>> refexpr)
        {
            sql.Select(sql.GetColumns<TDto>());

            sql = sqlexpr(sql);

            refexpr(new RefSql<TDto>(sql, null));
            return sql;
        }

        /// <summary>
        /// Complements a SELECT Sql statement with a referenced DTO.
        /// </summary>
        /// <typeparam name="T">The type of the DTO to select.</typeparam>
        /// <param name="refSql">The origin sql.</param>
        /// <param name="refexpr">An optional, nested, reference Sql expression.</param>
        /// <param name="referenceName">The optional name of the DTO reference.</param>
        /// <param name="tableAlias">The optional name of the table alias.</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>Select&lt;Foo>() produces: [foo].[value] AS [Foo_Value]</para>
        /// <para>With tableAlias: [tableAlias].[value] AS [Foo_Value]</para>
        /// <para>With referenceName: [foo].[value] AS [referenceName_Value]</para>
        /// </remarks>
        public static RefSql Select<T>(this RefSql refSql, Func<RefSql, RefSql> refexpr = null, string referenceName = null, string tableAlias = null)
        {
            if (referenceName == null) referenceName = typeof (T).Name;
            if (refSql.Prefix != null) referenceName = refSql.Prefix + PocoData.Separator + referenceName;

            var columns = refSql.Sql.GetColumns<T>(referenceName);
            refSql.Sql.Append(", " + string.Join(", ", columns));

            if (refexpr == null) return refSql;
            refexpr(new RefSql(refSql.Sql, referenceName));
            return refSql;
        }

        /// <summary>
        /// Creates a SELECT Sql statement.
        /// </summary>
        /// <typeparam name="T">The type of the DTO to select.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="columnExpressions">Expressions indicating the columns to select.</param>
        /// <returns>The Sql statement.</returns>
        /// <remarks>
        /// <para>If <paramref name="columnExpressions"/> is empty, all columns are selected.</para>
        /// </remarks>
        public static Sql<SqlContext> Select<T>(this Sql<SqlContext> sql, params Expression<Func<T, object>>[] columnExpressions)
        {
            var columns = columnExpressions.Length == 0
                ? sql.GetColumns<T>()
                : columnExpressions.Select(x => GetFieldName(x, sql.SqlContext.SqlSyntax)).ToArray();

            sql.Select(columns);
            return sql;
        }

        // fixme - obsolete
        public class RefSql
        {
            public RefSql(Sql<SqlContext> sql, string prefix)
            {
                Sql = sql;
                Prefix = prefix;
            }

            public Sql<SqlContext> Sql { get; }
            public string Prefix { get; }
        }

        public class RefSql<TDto>
        {
            public RefSql(Sql<SqlContext> sql, string prefix)
            {
                Sql = sql;
                Prefix = prefix;
            }

            public Sql<SqlContext> Sql { get; }
            public string Prefix { get; }

            public RefSql<TDto> Select<TRefDto>(Func<RefSql<TRefDto>, RefSql<TRefDto>> refexpr = null)
                => Select(null, null, refexpr);

            //fixme rename duplicate "ref expr"

            public RefSql<TDto> Select<TRefDto>(Expression<Func<TDto, object>> referenceExpression, Func<RefSql<TRefDto>, RefSql<TRefDto>> refexpr = null)
                => Select(referenceExpression, null, refexpr);

            public RefSql<TDto> Select<TRefDto>(Expression<Func<TDto, List<TRefDto>>> referenceExpression, Func<RefSql<TRefDto>, RefSql<TRefDto>> refexpr = null)
                => Select(referenceExpression, null, refexpr);

            public RefSql<TDto> Select<TRefDto>(Expression<Func<TDto, object>> referenceExpression, string tableAlias, Func<RefSql<TRefDto>, RefSql<TRefDto>> refexpr = null)
            {
                string referenceName = null;
                if (referenceExpression != null)
                {
                    var property = ExpressionHelper.FindProperty(referenceExpression) as PropertyInfo;
                    if (property == null)
                        throw new InvalidOperationException("Could not get property specified in expression.");
                    referenceName = property.Name;
                }
                if (referenceName == null) referenceName = typeof(TDto).Name;
                if (Prefix != null) referenceName = Prefix + PocoData.Separator + referenceName;

                var columns = Sql.GetColumns<TRefDto>(referenceName);
                Sql.Append(", " + string.Join(", ", columns));

                if (refexpr == null) return this;
                refexpr(new RefSql<TRefDto>(Sql, referenceName));
                return this;
            }

            // fixme - also handle the case... when it's not a List but a single one
            // fixme - DRY
            public RefSql<TDto> Select<TRefDto>(Expression<Func<TDto, List<TRefDto>>> referenceExpression, string tableAlias, Func<RefSql<TRefDto>, RefSql<TRefDto>> refexpr = null)
            {
                string referenceName = null;
                if (referenceExpression != null)
                {
                    var property = ExpressionHelper.FindProperty(referenceExpression) as PropertyInfo;
                    if (property == null)
                        throw new InvalidOperationException("Could not get property specified in expression.");
                    referenceName = property.Name;
                }
                if (referenceName == null) referenceName = typeof(TDto).Name;
                if (Prefix != null) referenceName = Prefix + PocoData.Separator + referenceName;

                var columns = Sql.GetColumns<TRefDto>(referenceName);
                Sql.Append(", " + string.Join(", ", columns));

                if (refexpr == null) return this;
                refexpr(new RefSql<TRefDto>(Sql, referenceName));
                return this;
            }
        }

        /// <summary>
        /// Gets the column names of a DTO.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO.</typeparam>
        /// <param name="sql">The origin sql.</param>
        /// <param name="columnExpressions">Expressions indicating the columns to select.</param>
        /// <returns>The comma-separated list of columns.</returns>
        /// <remarks>
        /// <para>If <paramref name="columnExpressions"/> is empty, all columns are selected.</para>
        /// </remarks>
        public static string Columns<TDto>(this Sql<SqlContext> sql, params Expression<Func<TDto, object>>[] columnExpressions)
        {
            var columns = columnExpressions.Length == 0
                ? sql.GetColumns<TDto>()
                : columnExpressions.Select(x => GetFieldName(x, sql.SqlContext.SqlSyntax)).ToArray();

            return string.Join(", ", columns);
        }

        #endregion

        #region Utilities

        private static object[] GetColumns<TDto>(this Sql<SqlContext> sql, string referenceName = null)
        {
            var pd = sql.SqlContext.PocoDataFactory.ForType(typeof (TDto));
            var tableName = pd.TableInfo.TableName;
            return pd.QueryColumns.Select(x => (object) GetColumn(sql.SqlContext.DatabaseType,
                tableName, x.Value.ColumnName,
                string.IsNullOrEmpty(x.Value.ColumnAlias) ? x.Value.MemberInfoKey : x.Value.ColumnAlias,
                referenceName)).ToArray();
        }

        private static string GetColumn(DatabaseType dbType, string tableName, string columnName, string columnAlias, string referenceName = null)
        {
            tableName = dbType.EscapeTableName(tableName);
            columnName = dbType.EscapeSqlIdentifier(columnName);
            columnAlias = dbType.EscapeSqlIdentifier((referenceName == null ? "" : (referenceName + "__")) + columnAlias);
            return tableName + "." + columnName + " AS " + columnAlias;
        }

        private static string GetTableName(this Type type)
        {
            // todo: returning string.Empty for now
            // BUT the code bits that calls this method cannot deal with string.Empty so we
            // should either throw, or fix these code bits...
            var attr = type.FirstAttribute<TableNameAttribute>();
            return string.IsNullOrWhiteSpace(attr?.Value) ? string.Empty : attr.Value;
        }

        private static string GetColumnName(this PropertyInfo column)
        {
            var attr = column.FirstAttribute<ColumnAttribute>();
            return string.IsNullOrWhiteSpace(attr?.Name) ? column.Name : attr.Name;
        }

        private static string GetFieldName<TDto>(Expression<Func<TDto, object>> fieldSelector, ISqlSyntaxProvider sqlSyntax)
        {
            var field = ExpressionHelper.FindProperty(fieldSelector) as PropertyInfo;
            var fieldName = field.GetColumnName();

            var type = typeof (TDto);
            var tableName = type.GetTableName();

            return sqlSyntax.GetQuotedTableName(tableName) + "." + sqlSyntax.GetQuotedColumnName(fieldName);
        }

        #endregion
    }
}
