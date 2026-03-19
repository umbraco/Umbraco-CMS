namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Column;

/// <summary>
///     Builds a Rename Column expression.
/// </summary>
public interface IRenameColumnBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the table on which the column rename operation will be performed.
    /// </summary>
    /// <param name="tableName">The name of the table containing the column to rename.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Column.IRenameColumnToBuilder"/> to continue configuring the rename operation.</returns>
    IRenameColumnToBuilder OnTable(string tableName);
}
