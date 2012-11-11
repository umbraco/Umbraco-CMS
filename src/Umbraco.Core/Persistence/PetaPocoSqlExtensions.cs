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

            return sql.From(SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(tableName));
        }

        public static Sql Where<T>(this Sql sql, Expression<Func<T, bool>> predicate)
        {
            var expresionist = new ExpressionHelper<T>();
            string whereExpression = expresionist.Visit(predicate);

            return sql.Where(whereExpression);
        }

        public static Sql OrderBy<TColumn>(this Sql sql, Expression<Func<TColumn, object>> columnMember)
        {
            var column = ExpressionHelper.FindProperty(columnMember) as PropertyInfo;
            var columnName = column.FirstAttribute<ColumnAttribute>().Name;

            return sql.OrderBy(SyntaxConfig.SqlSyntaxProvider.GetQuotedColumnName(columnName));
        }

        public static Sql GroupBy<TColumn>(this Sql sql, Expression<Func<TColumn, object>> columnMember)
        {
            var column = ExpressionHelper.FindProperty(columnMember) as PropertyInfo;
            var columnName = column.FirstAttribute<ColumnAttribute>().Name;

            return sql.GroupBy(SyntaxConfig.SqlSyntaxProvider.GetQuotedColumnName(columnName));
        }

        public static Sql.SqlJoinClause InnerJoin<T>(this Sql sql)
        {
            var type = typeof (T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.InnerJoin(SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(tableName));
        }

        public static Sql.SqlJoinClause LeftJoin<T>(this Sql sql)
        {
            var type = typeof (T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.LeftJoin(SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(tableName));
        }

        public static Sql.SqlJoinClause RightJoin<T>(this Sql sql)
        {
            var type = typeof (T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute == null ? string.Empty : tableNameAttribute.Value;

            return sql.RightJoin(SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(tableName));
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
                SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(leftTableName),
                SyntaxConfig.SqlSyntaxProvider.GetQuotedColumnName(leftColumnName),
                SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(rightTableName),
                SyntaxConfig.SqlSyntaxProvider.GetQuotedColumnName(rightColumnName));
            return sql.On(onClause);
        }
    }
}