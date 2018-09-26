using System;
using System.Linq.Expressions;
using System.Reflection;
using NPoco;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Provides extension methods to <see cref="ISqlSyntaxProvider"/>.
    /// </summary>
    public static class SqlSyntaxExtensions
    {
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

        /// <summary>
        /// Gets a quoted table and field name.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO.</typeparam>
        /// <param name="sqlSyntax">An <see cref="ISqlSyntaxProvider"/>.</param>
        /// <param name="fieldSelector">An expression specifying the field.</param>
        /// <param name="tableAlias">An optional table alias.</param>
        /// <returns></returns>
        public static string GetFieldName<TDto>(this ISqlSyntaxProvider sqlSyntax, Expression<Func<TDto, object>> fieldSelector, string tableAlias = null)
        {
            var field = ExpressionHelper.FindProperty(fieldSelector).Item1 as PropertyInfo;
            var fieldName = field.GetColumnName();

            var type = typeof(TDto);
            var tableName = tableAlias ?? type.GetTableName();

            return sqlSyntax.GetQuotedTableName(tableName) + "." + sqlSyntax.GetQuotedColumnName(fieldName);
        }
    }
}
