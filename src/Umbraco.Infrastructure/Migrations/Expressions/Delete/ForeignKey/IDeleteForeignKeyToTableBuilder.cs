namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.ForeignKey;

/// <summary>
///     Builds a Delete expression.
/// </summary>
public interface IDeleteForeignKeyToTableBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the target table of the foreign key.
    /// </summary>
    IDeleteForeignKeyPrimaryColumnBuilder ToTable(string table);
}
