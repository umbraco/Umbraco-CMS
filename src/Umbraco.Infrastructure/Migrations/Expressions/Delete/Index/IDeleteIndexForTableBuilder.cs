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
    IExecutableBuilder OnTable(string tableName);
}
