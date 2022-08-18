namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.ForeignKey;

/// <summary>
///     Builds a Delete expression.
/// </summary>
public interface IDeleteForeignKeyFromTableBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the source table of the foreign key.
    /// </summary>
    IDeleteForeignKeyForeignColumnBuilder FromTable(string foreignTableName);
}
