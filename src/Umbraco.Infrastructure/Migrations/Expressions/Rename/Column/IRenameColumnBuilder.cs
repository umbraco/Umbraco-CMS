namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Column;

/// <summary>
///     Builds a Rename Column expression.
/// </summary>
public interface IRenameColumnBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the table name.
    /// </summary>
    IRenameColumnToBuilder OnTable(string tableName);
}
