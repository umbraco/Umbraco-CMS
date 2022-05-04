namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.ForeignKey;

/// <summary>
///     Builds a Delete expression.
/// </summary>
public interface IDeleteForeignKeyForeignColumnBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the foreign column.
    /// </summary>
    IDeleteForeignKeyToTableBuilder ForeignColumn(string column);

    /// <summary>
    ///     Specifies the foreign columns.
    /// </summary>
    IDeleteForeignKeyToTableBuilder ForeignColumns(params string[] columns);
}
