using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Extension methods adding strong types to PetaPoco's Sql Builder
    /// </summary>
    public static class PetaPocoSqlExtensions
    {
        /// <summary>
        /// Defines the column to select in the generated SQL query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">Sql object</param>
        /// <param name="sqlSyntax">Sql syntax</param>
        /// <param name="fields">Columns to select</param>
        /// <returns></returns>
        public static Sql Select<T>(this Sql sql, ISqlSyntaxProvider sqlSyntax, params Expression<Func<T, object>>[] fields)
        {
            return sql.Select(GetFieldNames(sqlSyntax, fields));
        }
        
        /// <summary>
        /// Adds another set of field to select. This method must be used with "Select" when fecthing fields from different tables.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">Sql object</param>
        /// <param name="sqlSyntax">Sql syntax</param>
        /// <param name="fields">Additional columns to select</param>
        /// <returns></returns>
        public static Sql AndSelect<T>(this Sql sql, ISqlSyntaxProvider sqlSyntax, params Expression<Func<T, object>>[] fields)
        {
            return sql.AndSelect(GetFieldNames(sqlSyntax, fields));
        }

        [Obsolete("Use the overload specifying ISqlSyntaxProvider instead")]
        public static Sql From<T>(this Sql sql)
        {
            return From<T>(sql, SqlSyntaxContext.SqlSyntaxProvider);
        }

        public static Sql From<T>(this Sql sql, ISqlSyntaxProvider sqlSyntax)
        {
            var type = typeof(T);
            var tableName = type.GetTableName();

            return sql.From(sqlSyntax.GetQuotedTableName(tableName));
        }

        [Obsolete("Use the overload specifying ISqlSyntaxProvider instead")]
        public static Sql Where<T>(this Sql sql, Expression<Func<T, bool>> predicate)
        {
            var expresionist = new PocoToSqlExpressionVisitor<T>();
            var whereExpression = expresionist.Visit(predicate);
            return sql.Where(whereExpression, expresionist.GetSqlParameters());
        }

        public static Sql Where<T>(this Sql sql, Expression<Func<T, bool>> predicate, ISqlSyntaxProvider sqlSyntax)
        {
            var expresionist = new PocoToSqlExpressionVisitor<T>(sqlSyntax);
            var whereExpression = expresionist.Visit(predicate);
            return sql.Where(whereExpression, expresionist.GetSqlParameters());
        }

        private static string GetFieldName<T>(Expression<Func<T, object>> fieldSelector, ISqlSyntaxProvider sqlSyntax)
        {
            var field = ExpressionHelper.FindProperty(fieldSelector) as PropertyInfo;
            var fieldName = field.GetColumnName();

            var type = typeof(T);
            var tableName = type.GetTableName();

            return sqlSyntax.GetQuotedTableName(tableName) + "." + sqlSyntax.GetQuotedColumnName(fieldName);
        }

        private static string[] GetFieldNames<T>(ISqlSyntaxProvider sqlSyntax, params Expression<Func<T, object>>[] fields)
        {
            if (fields.Length == 0)
            {
                return new[] { string.Format("{0}.*", sqlSyntax.GetQuotedTableName(typeof(T).GetTableName())) };
            }

            return fields.Select(field => GetFieldName(field, sqlSyntax)).ToArray();
        }

        [Obsolete("Use the overload specifying ISqlSyntaxProvider instead")]
        public static Sql WhereIn<T>(this Sql sql, Expression<Func<T, object>> fieldSelector, IEnumerable values)
        {
            return sql.WhereIn(fieldSelector, values, SqlSyntaxContext.SqlSyntaxProvider);
        }

        public static Sql WhereIn<T>(this Sql sql, Expression<Func<T, object>> fieldSelector, IEnumerable values, ISqlSyntaxProvider sqlSyntax)
        {
            var fieldName = GetFieldName(fieldSelector, sqlSyntax);
            return sql.Where(fieldName + " IN (@values)", new { values });
        }

        [Obsolete("Use the overload specifying ISqlSyntaxProvider instead")]
        public static Sql WhereAnyIn<TDto>(this Sql sql, Expression<Func<TDto, object>>[] fieldSelectors, IEnumerable values)
        {
            return sql.WhereAnyIn(fieldSelectors, values, SqlSyntaxContext.SqlSyntaxProvider);
        }

        public static Sql WhereAnyIn<TDto>(this Sql sql, Expression<Func<TDto, object>>[] fieldSelectors, IEnumerable values, ISqlSyntaxProvider sqlSyntax)
        {
            var fieldNames = fieldSelectors.Select(x => GetFieldName(x, sqlSyntax)).ToArray();
            var sb = new StringBuilder();
            sb.Append("(");
            for (var i = 0; i < fieldNames.Length; i++)
            {
                if (i > 0) sb.Append(" OR ");
                sb.Append(fieldNames[i] + " IN (@values)");
            }
            sb.Append(")");
            return sql.Where(sb.ToString(), new { values });
        }

        [Obsolete("Use the overload specifying ISqlSyntaxProvider instead")]
        public static Sql OrderBy<TColumn>(this Sql sql, Expression<Func<TColumn, object>> columnMember)
        {
            return OrderBy<TColumn>(sql, columnMember, SqlSyntaxContext.SqlSyntaxProvider);
        }

        public static Sql OrderBy<TColumn>(this Sql sql, Expression<Func<TColumn, object>> columnMember, ISqlSyntaxProvider sqlSyntax)
        {
            var syntax = "(" + GetFieldName(columnMember, sqlSyntax) + ")";
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
            var columnName = column.GetColumnName();

            var type = typeof(TColumn);
            var tableName = type.GetTableName();

            //need to ensure the order by is in brackets, see: https://github.com/toptensoftware/PetaPoco/issues/177
            var syntax = string.Format("({0}.{1}) DESC",
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
            var columnName = column.GetColumnName();

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
            var tableName = type.GetTableName();

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
            var tableName = type.GetTableName();

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
            var tableName = type.GetTableName();

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
            var tableName = type.GetTableName();

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
            var leftTableName = leftType.GetTableName();
            var rightTableName = rightType.GetTableName();

            var leftColumn = ExpressionHelper.FindProperty(leftMember) as PropertyInfo;
            var rightColumn = ExpressionHelper.FindProperty(rightMember) as PropertyInfo;

            var leftColumnName = leftColumn.GetColumnName();
            var rightColumnName = rightColumn.GetColumnName();

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

        private static string GetTableName(this Type type)
        {
            // todo: returning string.Empty for now
            // BUT the code bits that calls this method cannot deal with string.Empty so we
            // should either throw, or fix these code bits...
            var attr = type.FirstAttribute<TableNameAttribute>();
            return attr == null || string.IsNullOrWhiteSpace(attr.Value) ? string.Empty : attr.Value;
        }

        private static string GetColumnName(this PropertyInfo column)
        {
            var attr = column.FirstAttribute<ColumnAttribute>();
            return attr == null || string.IsNullOrWhiteSpace(attr.Name) ? column.Name : attr.Name;
        }
    }
}
