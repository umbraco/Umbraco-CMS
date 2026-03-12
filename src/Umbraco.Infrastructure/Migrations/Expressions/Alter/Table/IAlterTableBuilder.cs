namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table;

/// <summary>
///     Builds an Alter Table expression.
/// </summary>
public interface IAlterTableBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies a column to add.
    /// </summary>
    IAlterTableColumnTypeBuilder AddColumn(string name);

    /// <summary>
    ///     Begins the process of altering the definition of a specified column in the table.
    /// </summary>
    /// <param name="name">The name of the column to alter.</param>
    /// <returns>An <see cref="IAlterTableColumnTypeBuilder"/> that allows further specification of the new column type and properties.</returns>
    IAlterTableColumnTypeBuilder AlterColumn(string name);
}
