using Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Column;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Table;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename;

/// <summary>
///     Builds a Rename expression.
/// </summary>
public interface IRenameBuilder : IFluentBuilder
{
    /// <summary>
    ///     Begins a table rename operation by specifying the current name of the table.
    /// </summary>
    /// <param name="oldName">The current name of the table to rename.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.IRenameTableBuilder" /> to continue the rename operation.</returns>
    IRenameTableBuilder Table(string oldName);

    /// <summary>
    /// Specifies the column to rename in a database table.
    /// </summary>
    /// <param name="oldName">The current name of the column to be renamed.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.IRenameColumnBuilder" /> for specifying the new column name.</returns>
    IRenameColumnBuilder Column(string oldName);
}
