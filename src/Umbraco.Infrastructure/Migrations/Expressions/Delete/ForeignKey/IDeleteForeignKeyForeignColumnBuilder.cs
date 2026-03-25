namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.ForeignKey;

/// <summary>
///     Builds a Delete expression.
/// </summary>
public interface IDeleteForeignKeyForeignColumnBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the foreign column.
    /// </summary>
    /// <param name="column">The name of the foreign column.</param>
    /// <returns>An <see cref="IDeleteForeignKeyToTableBuilder"/> to continue building the foreign key deletion expression.</returns>
    IDeleteForeignKeyToTableBuilder ForeignColumn(string column);

    /// <summary>
    ///     Specifies the foreign columns.
    /// </summary>
    IDeleteForeignKeyToTableBuilder ForeignColumns(params string[] columns);
}
