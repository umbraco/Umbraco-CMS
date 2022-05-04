using System.Linq.Expressions;
using System.Reflection;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods to <see cref="ISqlSyntaxProvider" />.
/// </summary>
public static class SqlSyntaxExtensions
{
    /// <summary>
    ///     Gets a quoted table and field name.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <param name="sqlSyntax">An <see cref="ISqlSyntaxProvider" />.</param>
    /// <param name="fieldSelector">An expression specifying the field.</param>
    /// <param name="tableAlias">An optional table alias.</param>
    /// <returns></returns>
    public static string GetFieldName<TDto>(
        this ISqlSyntaxProvider sqlSyntax,
        Expression<Func<TDto, object?>> fieldSelector, string? tableAlias = null)
    {
        var field = ExpressionHelper.FindProperty(fieldSelector).Item1 as PropertyInfo;
        var fieldName = field?.GetColumnName();

        Type type = typeof(TDto);
        var tableName = tableAlias ?? type.GetTableName();

        return sqlSyntax.GetQuotedTableName(tableName) + "." + sqlSyntax.GetQuotedColumnName(fieldName);
    }

    private static string GetColumnName(this PropertyInfo column)
    {
        ColumnAttribute? attr = column.FirstAttribute<ColumnAttribute>();
        return string.IsNullOrWhiteSpace(attr?.Name) ? column.Name : attr.Name;
    }
}
