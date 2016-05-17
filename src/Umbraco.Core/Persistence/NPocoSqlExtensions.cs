using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NPoco;
using Umbraco.Core.Persistence.Querying;

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
            var expresionist = new PocoToSqlExpressionHelper<T>(sql.SqlContext);
            var whereExpression = expresionist.Visit(predicate);
            sql.Where(whereExpression, expresionist.GetSqlParameters());
            return sql;
        }

        public static Sql<SqlContext> WhereIn<T>(this Sql<SqlContext> sql, Expression<Func<T, object>> fieldSelector, IEnumerable values)
        {
            var expresionist = new PocoToSqlExpressionHelper<T>(sql.SqlContext);
            var fieldExpression = expresionist.Visit(fieldSelector);
            sql.Where(fieldExpression + " IN (@values)", new { /*@values =*/ values });
            return sql;
        }

        #endregion

        #region From

        public static Sql<SqlContext> From<T>(this Sql<SqlContext> sql)
        {
            var type = typeof (T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            var tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            sql.From(sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName));
            return sql;
        }

        #endregion

        #region OrderBy, GroupBy

        public static Sql<SqlContext> OrderBy<T>(this Sql<SqlContext> sql, Expression<Func<T, object>> columnMember)
        {
            var column = ExpressionHelper.FindProperty(columnMember) as PropertyInfo;
            var columnName = column.FirstAttribute<ColumnAttribute>().Name;

            var type = typeof (T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            var tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            // need to ensure the order by is in brackets, see: https://github.com/toptensoftware/PetaPoco/issues/177
            var sqlSyntax = sql.SqlContext.SqlSyntax;
            var syntax = $"({sqlSyntax.GetQuotedTableName(tableName)}.{sqlSyntax.GetQuotedColumnName(columnName)})";

            sql.OrderBy(syntax);
            return sql;
        }

        public static Sql<SqlContext> OrderByDescending<T>(this Sql<SqlContext> sql, Expression<Func<T, object>> columnMember)
        {
            var column = ExpressionHelper.FindProperty(columnMember) as PropertyInfo;
            var columnName = column.FirstAttribute<ColumnAttribute>().Name;

            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            var tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            var sqlSyntax = sql.SqlContext.SqlSyntax;
            var syntax = $"{sqlSyntax.GetQuotedTableName(tableName)}.{sqlSyntax.GetQuotedColumnName(columnName)} DESC";

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
            var columnName = column.FirstAttribute<ColumnAttribute>().Name;

            sql.GroupBy(sql.SqlContext.SqlSyntax.GetQuotedColumnName(columnName));
            return sql;
        }

        #endregion

        #region Joins

        public static Sql<SqlContext>.SqlJoinClause<SqlContext>  InnerJoin<T>(this Sql<SqlContext> sql)
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            var tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.InnerJoin(sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName));
        }

        public static Sql<SqlContext>.SqlJoinClause<SqlContext> LeftJoin<T>(this Sql<SqlContext> sql)
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            var tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.LeftJoin(sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName));
        }

        public static Sql<SqlContext>.SqlJoinClause<SqlContext> RightJoin<T>(this Sql<SqlContext> sql)
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            var tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.RightJoin(sql.SqlContext.SqlSyntax.GetQuotedTableName(tableName));
        }

        public static Sql<SqlContext> On<TLeft, TRight>(this Sql<SqlContext>.SqlJoinClause<SqlContext> clause,
            Expression<Func<TLeft, object>> leftMember, Expression<Func<TRight, object>> rightMember,
            params object[] args)
        {
            var sqlSyntax = clause.SqlContext.SqlSyntax;

            var leftType = typeof (TLeft);
            var rightType = typeof (TRight);
            var leftTableName = sqlSyntax.GetQuotedTableName(leftType.FirstAttribute<TableNameAttribute>().Value);
            var rightTableName = sqlSyntax.GetQuotedTableName(rightType.FirstAttribute<TableNameAttribute>().Value);

            var left = ExpressionHelper.FindProperty(leftMember) as PropertyInfo;
            var right = ExpressionHelper.FindProperty(rightMember) as PropertyInfo;
            var leftColumnName = sqlSyntax.GetQuotedColumnName(left.FirstAttribute<ColumnAttribute>().Name);
            var rightColumnName = sqlSyntax.GetQuotedColumnName(right.FirstAttribute<ColumnAttribute>().Name);

            var onClause = $"{leftTableName}.{leftColumnName} = {rightTableName}.{rightColumnName}";
            return clause.On(onClause);
        }

        #endregion

        #region Select

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

        public static Sql<SqlContext> Select<T>(this Sql<SqlContext> sql, Func<RefSql, RefSql> refexpr = null)
        {
            var pd = sql.SqlContext.PocoDataFactory.ForType(typeof (T));

            var tableName = pd.TableInfo.TableName;
            var columns = pd.QueryColumns.Select(x => GetColumn(sql.SqlContext.DatabaseType,
                tableName,
                x.Value.ColumnName,
                string.IsNullOrEmpty(x.Value.ColumnAlias) ? x.Value.MemberInfoKey : x.Value.ColumnAlias));

            sql.Select(string.Join(", ", columns));

            if (refexpr == null) return sql;
            refexpr(new RefSql(sql, null));
            return sql;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="refSql"></param>
        /// <param name="refexpr"></param>
        /// <param name="referenceName">The name of the DTO reference.</param>
        /// <param name="tableAlias">The name of the table alias.</param>
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
            var pd = refSql.Sql.SqlContext.PocoDataFactory.ForType(typeof (T));

            var tableName = tableAlias ?? pd.TableInfo.TableName;
            var columns = pd.QueryColumns.Select(x => GetColumn(refSql.Sql.SqlContext.DatabaseType,
                tableName,
                x.Value.ColumnName,
                string.IsNullOrEmpty(x.Value.ColumnAlias) ? x.Value.MemberInfoKey : x.Value.ColumnAlias,
                referenceName));

            refSql.Sql.Append(", " + string.Join(", ", columns));

            if (refexpr == null) return refSql;
            refexpr(new RefSql(refSql.Sql, referenceName));
            return refSql;
        }

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

        private static string GetColumn(DatabaseType dbType, string tableName, string columnName, string columnAlias, string referenceName = null)
        {
            tableName = dbType.EscapeTableName(tableName);
            columnName = dbType.EscapeSqlIdentifier(columnName);
            columnAlias = dbType.EscapeSqlIdentifier((referenceName == null ? "" : (referenceName + "__")) + columnAlias);
            return tableName + "." + columnName + " AS " + columnAlias;
        }

        #endregion
    }
}
