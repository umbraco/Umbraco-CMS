using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NPoco;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Provides extension methods to NPoco Sql class.
    /// </summary>
    public static class NPocoSqlExtensions
    {
        public static Sql From<T>(this Sql sql, ISqlSyntaxProvider sqlSyntax)
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.From(sqlSyntax.GetQuotedTableName(tableName));
        }

        public static Sql Where<T>(this Sql sql, ISqlSyntaxProvider sqlSyntax, Expression<Func<T, bool>> predicate)
        {
            var expresionist = new PocoToSqlExpressionHelper<T>(sqlSyntax);
            var whereExpression = expresionist.Visit(predicate);
            return sql.Where(whereExpression, expresionist.GetSqlParameters());
        }

        public static Sql WhereIn<T>(this Sql sql, ISqlSyntaxProvider sqlSyntax, Expression<Func<T, object>> fieldSelector, IEnumerable values)
        {
            var expresionist = new PocoToSqlExpressionHelper<T>(sqlSyntax);
            var fieldExpression = expresionist.Visit(fieldSelector);
            return sql.Where(fieldExpression + " IN (@values)", new {@values = values});
        }

        public static Sql OrderBy<TColumn>(this Sql sql, ISqlSyntaxProvider sqlSyntax, Expression<Func<TColumn, object>> columnMember)
        {
            var column = ExpressionHelper.FindProperty(columnMember) as PropertyInfo;
            var columnName = column.FirstAttribute<ColumnAttribute>().Name;

            var type = typeof(TColumn);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            //need to ensure the order by is in brackets, see: https://github.com/toptensoftware/PetaPoco/issues/177
            var syntax = string.Format("({0}.{1})",
                sqlSyntax.GetQuotedTableName(tableName),
                sqlSyntax.GetQuotedColumnName(columnName));

            return sql.OrderBy(syntax);
        }

        public static Sql OrderByDescending<TColumn>(this Sql sql, ISqlSyntaxProvider sqlSyntax, Expression<Func<TColumn, object>> columnMember)
        {
            var column = ExpressionHelper.FindProperty(columnMember) as PropertyInfo;
            var columnName = column.FirstAttribute<ColumnAttribute>().Name;

            var type = typeof(TColumn);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            var syntax = string.Format("{0}.{1} DESC",
                sqlSyntax.GetQuotedTableName(tableName),
                sqlSyntax.GetQuotedColumnName(columnName));

            return sql.OrderBy(syntax);
        }

        public static Sql GroupBy<TColumn>(this Sql sql, ISqlSyntaxProvider sqlProvider, Expression<Func<TColumn, object>> columnMember)
        {
            var column = ExpressionHelper.FindProperty(columnMember) as PropertyInfo;
            var columnName = column.FirstAttribute<ColumnAttribute>().Name;

            return sql.GroupBy(sqlProvider.GetQuotedColumnName(columnName));
        }

        public static Sql.SqlJoinClause InnerJoin<T>(this Sql sql, ISqlSyntaxProvider sqlSyntax)
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.InnerJoin(sqlSyntax.GetQuotedTableName(tableName));
        }

        public static Sql.SqlJoinClause LeftJoin<T>(this Sql sql, ISqlSyntaxProvider sqlSyntax)
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.LeftJoin(sqlSyntax.GetQuotedTableName(tableName));
        }

        public static Sql.SqlJoinClause LeftOuterJoin<T>(this Sql sql, ISqlSyntaxProvider sqlSyntax)
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.LeftOuterJoin(sqlSyntax.GetQuotedTableName(tableName));
        }

        public static Sql.SqlJoinClause RightJoin<T>(this Sql sql, ISqlSyntaxProvider sqlSyntax)
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.RightJoin(sqlSyntax.GetQuotedTableName(tableName));
        }

        public static Sql On<TLeft, TRight>(this Sql.SqlJoinClause sql, ISqlSyntaxProvider sqlSyntax, Expression<Func<TLeft, object>> leftMember,
                                           Expression<Func<TRight, object>> rightMember, params object[] args)
        {
            var leftType = typeof(TLeft);
            var rightType = typeof(TRight);
            var leftTableName = leftType.FirstAttribute<TableNameAttribute>().Value;
            var rightTableName = rightType.FirstAttribute<TableNameAttribute>().Value;

            var left = ExpressionHelper.FindProperty(leftMember) as PropertyInfo;
            var right = ExpressionHelper.FindProperty(rightMember) as PropertyInfo;
            var leftColumnName = left.FirstAttribute<ColumnAttribute>().Name;
            var rightColumnName = right.FirstAttribute<ColumnAttribute>().Name;

            string onClause = string.Format("{0}.{1} = {2}.{3}",
                sqlSyntax.GetQuotedTableName(leftTableName),
                sqlSyntax.GetQuotedColumnName(leftColumnName),
                sqlSyntax.GetQuotedTableName(rightTableName),
                sqlSyntax.GetQuotedColumnName(rightColumnName));
            return sql.On(onClause);
        }

        public static Sql OrderByDescending(this Sql sql, params object[] columns)
        {
            return sql.Append(new Sql("ORDER BY " + String.Join(", ", (from x in columns select x + " DESC").ToArray())));
        }

        public static Sql.SqlJoinClause LeftOuterJoin(this Sql sql, string table) { return Join(sql, "LEFT OUTER JOIN ", table); }
        public static Sql.SqlJoinClause RightJoin(this Sql sql, string table) { return Join(sql, "RIGHT JOIN ", table); }

        // copied from NPoco (private)
        private static Sql.SqlJoinClause Join(Sql sql, string joinType, string table)
        {
            return new Sql.SqlJoinClause(sql.Append(new Sql(joinType + table)));
        }

        public static Sql SelectCount(this Sql sql)
        {
            return sql.Select("COUNT(*)");
        }

        public static Sql Select<T>(this Sql sql, Func<RefSql, RefSql> refexpr = null)
        {
            Database database = ApplicationContext.Current.DatabaseContext.Database; // fixme.npoco
            var pd = database.PocoDataFactory.ForType(typeof(T));
            var tableName = pd.TableInfo.TableName;
            var columns = pd.QueryColumns.Select(x => GetColumn(database.DatabaseType,
                tableName,
                x.Value.ColumnName,
                string.IsNullOrEmpty(x.Value.ColumnAlias) ? x.Value.MemberInfoKey : x.Value.ColumnAlias));
            sql = sql.Select(string.Join(", ", columns));

            if (refexpr != null)
            {
                var nsql = new RefSql(sql, null);
                refexpr(nsql);
            }

            return sql;
        }

        public static RefSql Select<T>(this RefSql sql, Func<RefSql, RefSql> refexpr = null)
        {
            return sql.Select<T>(null, refexpr);
        }

        public static RefSql Select<T>(this RefSql sql, string referenceName, Func<RefSql, RefSql> refexpr = null)
        {
            Database database = ApplicationContext.Current.DatabaseContext.Database; // fixme.npoco
            if (referenceName == null) referenceName = typeof(T).Name;
            if (sql.Prefix != null) referenceName = sql.Prefix + PocoData.Separator + referenceName;
            var pd = database.PocoDataFactory.ForType(typeof(T));
            var tableName = pd.TableInfo.TableName;
            var columns = pd.QueryColumns.Select(x => GetColumn(database.DatabaseType,
                tableName,
                x.Value.ColumnName,
                string.IsNullOrEmpty(x.Value.ColumnAlias) ? x.Value.MemberInfoKey : x.Value.ColumnAlias,
                referenceName));
            sql.Sql.Append(", " + string.Join(", ", columns));

            if (refexpr != null)
            {
                var nsql = new RefSql(sql.Sql, referenceName);
                refexpr(nsql);
            }

            return sql;
        }

        public class RefSql
        {
            public RefSql(Sql sql, string prefix)
            {
                Sql = sql;
                Prefix = prefix;
            }

            public Sql Sql { get; private set; }
            public string Prefix { get; private set; }
        }

        private static string GetColumn(DatabaseType dbType, string tableName, string columnName, string columnAlias, string referenceName = null)
        {
            tableName = dbType.EscapeTableName(tableName);
            columnName = dbType.EscapeSqlIdentifier(columnName);
            columnAlias = dbType.EscapeSqlIdentifier((referenceName == null ? "" : (referenceName + "__")) + columnAlias);
            return tableName + "." + columnName + " AS " + columnAlias;
        }
    }
}