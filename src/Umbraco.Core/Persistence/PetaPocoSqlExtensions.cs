using System;
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
        public static Sql From<T>(this Sql sql)
        {
            var type = typeof (T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.From(SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(tableName));
        }

        public static Sql Where<T>(this Sql sql, Expression<Func<T, bool>> predicate)
        {
            var expresionist = new PocoToSqlExpressionHelper<T>();
            string whereExpression = expresionist.Visit(predicate);

            return sql.Where(whereExpression);
        }

        public static Sql OrderBy<TColumn>(this Sql sql, Expression<Func<TColumn, object>> columnMember)
        {
            var column = ExpressionHelper.FindProperty(columnMember) as PropertyInfo;
            var columnName = column.FirstAttribute<ColumnAttribute>().Name;

            var type = typeof(TColumn);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            //need to ensure the order by is in brackets, see: https://github.com/toptensoftware/PetaPoco/issues/177
            var syntax = string.Format("({0}.{1})",
                SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(tableName),
                SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName(columnName));

            return sql.OrderBy(syntax);
        }

        public static Sql OrderByDescending<TColumn>(this Sql sql, Expression<Func<TColumn, object>> columnMember)
        {
            var column = ExpressionHelper.FindProperty(columnMember) as PropertyInfo;
            var columnName = column.FirstAttribute<ColumnAttribute>().Name;

            var type = typeof(TColumn);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            var syntax = string.Format("{0}.{1} DESC",
                SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(tableName),
                SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName(columnName));

            return sql.OrderBy(syntax);
        }

        public static Sql GroupBy<TColumn>(this Sql sql, Expression<Func<TColumn, object>> columnMember)
        {
            var column = ExpressionHelper.FindProperty(columnMember) as PropertyInfo;
            var columnName = column.FirstAttribute<ColumnAttribute>().Name;

            return sql.GroupBy(SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName(columnName));
        }

        public static Sql.SqlJoinClause InnerJoin<T>(this Sql sql)
        {
            var type = typeof (T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.InnerJoin(SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(tableName));
        }

        public static Sql.SqlJoinClause LeftJoin<T>(this Sql sql)
        {
            var type = typeof (T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.LeftJoin(SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(tableName));
        }

        public static Sql.SqlJoinClause LeftOuterJoin<T>(this Sql sql)
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.LeftOuterJoin(SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(tableName));
        }

        public static Sql.SqlJoinClause RightJoin<T>(this Sql sql)
        {
            var type = typeof (T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.RightJoin(SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(tableName));
        }

        public static Sql On<TLeft, TRight>(this Sql.SqlJoinClause sql, Expression<Func<TLeft, object>> leftMember,
                                            Expression<Func<TRight, object>> rightMember, params object[] args)
        {
            var leftType = typeof (TLeft);
            var rightType = typeof (TRight);
            var leftTableName = leftType.FirstAttribute<TableNameAttribute>().Value;
            var rightTableName = rightType.FirstAttribute<TableNameAttribute>().Value;

            var left = ExpressionHelper.FindProperty(leftMember) as PropertyInfo;
            var right = ExpressionHelper.FindProperty(rightMember) as PropertyInfo;
            var leftColumnName = left.FirstAttribute<ColumnAttribute>().Name;
            var rightColumnName = right.FirstAttribute<ColumnAttribute>().Name;

            string onClause = string.Format("{0}.{1} = {2}.{3}", 
                SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(leftTableName),
                SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName(leftColumnName),
                SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(rightTableName),
                SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName(rightColumnName));
            return sql.On(onClause);
        }
    }
}