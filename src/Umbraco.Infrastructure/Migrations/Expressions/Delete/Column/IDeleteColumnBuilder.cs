using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Column;

/// <summary>
///     Builds a Delete Column expression.
/// </summary>
public interface IDeleteColumnBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the table that contains the column to be deleted.
    /// </summary>
    /// <param name="tableName">The name of the table containing the column to delete.</param>
    /// <returns>An <see cref="IExecutableBuilder"/> that can be used to execute the delete column operation.</returns>
    IExecutableBuilder FromTable(string tableName);

    /// <summary>
    /// Specifies the column to delete in the current delete operation.
    /// </summary>
    /// <param name="columnName">The name of the column to be deleted.</param>
    /// <returns>The current <see cref="IDeleteColumnBuilder"/> instance for method chaining.</returns>
    IDeleteColumnBuilder Column(string columnName);
}
