using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Column;

/// <summary>
///     Builds a Delete Column expression.
/// </summary>
public interface IDeleteColumnBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the table of the column to delete.
    /// </summary>
    IExecutableBuilder FromTable(string tableName);

    /// <summary>
    ///     Specifies the column to delete.
    /// </summary>
    IDeleteColumnBuilder Column(string columnName);
}
