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
    ///     Specifies a column to alter.
    /// </summary>
    IAlterTableColumnTypeBuilder AlterColumn(string name);
}
