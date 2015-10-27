using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Extension methods adding strong types to PetaPoco's Sql Builder
    /// </summary>
    public static class PetaPocoSqlExtensions
    {
        [Obsolete("Use the overload specifying ISqlSyntaxProvider instead")]
        public static Sql From<T>(this Sql sql)
        {
            return From<T>(sql, SqlSyntaxContext.SqlSyntaxProvider);
        }

        public static Sql From<T>(this Sql sql, ISqlSyntaxProvider sqlSyntax)
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.From(sqlSyntax.GetQuotedTableName(tableName));
        }

        public static Sql Where<T>(this Sql sql, Expression<Func<T, bool>> predicate)
        {
            var expresionist = new PocoToSqlExpressionHelper<T>();
            var whereExpression = expresionist.Visit(predicate);
            return sql.Where(whereExpression, expresionist.GetSqlParameters());
        }

        public static Sql WhereIn<T>(this Sql sql, Expression<Func<T, object>> fieldSelector, IEnumerable values)
        {
            var expresionist = new PocoToSqlExpressionHelper<T>();
            var fieldExpression = expresionist.Visit(fieldSelector);
            return sql.Where(fieldExpression + " IN (@values)", new {@values = values});
        }

        [Obsolete("Use the overload specifying ISqlSyntaxProvider instead")]
        public static Sql OrderBy<TColumn>(this Sql sql, Expression<Func<TColumn, object>> columnMember)
        {
            return OrderBy<TColumn>(sql, columnMember, SqlSyntaxContext.SqlSyntaxProvider);
        }

        public static Sql OrderBy<TColumn>(this Sql sql, Expression<Func<TColumn, object>> columnMember, ISqlSyntaxProvider sqlSyntax)
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

        [Obsolete("Use the overload specifying ISqlSyntaxProvider instead")]
        public static Sql OrderByDescending<TColumn>(this Sql sql, Expression<Func<TColumn, object>> columnMember)
        {
            return OrderByDescending<TColumn>(sql, columnMember, SqlSyntaxContext.SqlSyntaxProvider);
        }

        public static Sql OrderByDescending<TColumn>(this Sql sql, Expression<Func<TColumn, object>> columnMember, ISqlSyntaxProvider sqlSyntax)
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

        [Obsolete("Use the overload specifying ISqlSyntaxProvider instead")]
        public static Sql GroupBy<TColumn>(this Sql sql, Expression<Func<TColumn, object>> columnMember)
        {
            return GroupBy<TColumn>(sql, columnMember, SqlSyntaxContext.SqlSyntaxProvider);
        }

        public static Sql GroupBy<TColumn>(this Sql sql, Expression<Func<TColumn, object>> columnMember, ISqlSyntaxProvider sqlProvider)
        {
            var column = ExpressionHelper.FindProperty(columnMember) as PropertyInfo;
            var columnName = column.FirstAttribute<ColumnAttribute>().Name;

            return sql.GroupBy(sqlProvider.GetQuotedColumnName(columnName));
        }

        [Obsolete("Use the overload specifying ISqlSyntaxProvider instead")]
        public static Sql.SqlJoinClause InnerJoin<T>(this Sql sql)
        {
            return InnerJoin<T>(sql, SqlSyntaxContext.SqlSyntaxProvider);
        }

        public static Sql.SqlJoinClause InnerJoin<T>(this Sql sql, ISqlSyntaxProvider sqlSyntax)
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.InnerJoin(sqlSyntax.GetQuotedTableName(tableName));
        }

        [Obsolete("Use the overload specifying ISqlSyntaxProvider instead")]
        public static Sql.SqlJoinClause LeftJoin<T>(this Sql sql)
        {
            return LeftJoin<T>(sql, SqlSyntaxContext.SqlSyntaxProvider);
        }

        public static Sql.SqlJoinClause LeftJoin<T>(this Sql sql, ISqlSyntaxProvider sqlSyntax)
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.LeftJoin(sqlSyntax.GetQuotedTableName(tableName));
        }

        [Obsolete("Use the overload specifying ISqlSyntaxProvider instead")]
        public static Sql.SqlJoinClause LeftOuterJoin<T>(this Sql sql)
        {
            return LeftOuterJoin<T>(sql, SqlSyntaxContext.SqlSyntaxProvider);
        }

        public static Sql.SqlJoinClause LeftOuterJoin<T>(this Sql sql, ISqlSyntaxProvider sqlSyntax)
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.LeftOuterJoin(sqlSyntax.GetQuotedTableName(tableName));
        }

        [Obsolete("Use the overload specifying ISqlSyntaxProvider instead")]
        public static Sql.SqlJoinClause RightJoin<T>(this Sql sql)
        {
            return RightJoin<T>(sql, SqlSyntaxContext.SqlSyntaxProvider);
        }

        public static Sql.SqlJoinClause RightJoin<T>(this Sql sql, ISqlSyntaxProvider sqlSyntax)
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.RightJoin(sqlSyntax.GetQuotedTableName(tableName));
        }

        [Obsolete("Use the overload specifying ISqlSyntaxProvider instead")]
        public static Sql On<TLeft, TRight>(this Sql.SqlJoinClause sql, Expression<Func<TLeft, object>> leftMember,
                                            Expression<Func<TRight, object>> rightMember, params object[] args)
        {
            return On<TLeft, TRight>(sql, SqlSyntaxContext.SqlSyntaxProvider, leftMember, rightMember, args);
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
    }
}