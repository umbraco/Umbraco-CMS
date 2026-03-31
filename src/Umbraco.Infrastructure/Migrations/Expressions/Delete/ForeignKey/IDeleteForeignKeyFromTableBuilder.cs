namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.ForeignKey;

/// <summary>
///     Builds a Delete expression.
/// </summary>
public interface IDeleteForeignKeyFromTableBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the table from which the foreign key will be deleted.
    /// </summary>
    /// <param name="foreignTableName">The name of the table containing the foreign key to be deleted.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.ForeignKey.IDeleteForeignKeyForeignColumnBuilder" /> to specify the foreign column.</returns>
    IDeleteForeignKeyForeignColumnBuilder FromTable(string foreignTableName);
}
