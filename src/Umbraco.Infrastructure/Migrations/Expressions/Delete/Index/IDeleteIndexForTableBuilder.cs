using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Index;

/// <summary>
///     Builds a Delete expression.
/// </summary>
public interface IDeleteIndexForTableBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the table of the index to delete.
    /// </summary>
    /// <param name="tableName">The name of the table containing the index to delete.</param>
    /// <returns>An <see cref="IExecutableBuilder"/> that can be used to execute the delete index operation.</returns>
    IExecutableBuilder OnTable(string tableName);
}
