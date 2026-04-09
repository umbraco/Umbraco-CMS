namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey;

/// <summary>
/// Represents a builder for specifying the source table when creating a foreign key.
/// </summary>
public interface ICreateForeignKeyFromTableBuilder : IFluentBuilder
{
    /// <summary>
    /// Sets the source table for the foreign key constraint.
    /// </summary>
    /// <param name="table">The name of the source table.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey.ICreateForeignKeyForeignColumnBuilder" /> for further configuration.</returns>
    ICreateForeignKeyForeignColumnBuilder FromTable(string table);
}
