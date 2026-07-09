namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey;

/// <summary>
/// Provides a builder for specifying the primary column when creating a foreign key.
/// </summary>
public interface ICreateForeignKeyPrimaryColumnBuilder : IFluentBuilder
{
    /// <summary>Specifies the primary (referenced) column for the foreign key constraint.</summary>
    /// <param name="column">The name of the primary (referenced) column.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey.ICreateForeignKeyCascadeBuilder"/> to continue building the foreign key.</returns>
    ICreateForeignKeyCascadeBuilder PrimaryColumn(string column);

    /// <summary>
    /// Specifies the primary key columns for the foreign key.
    /// </summary>
    /// <param name="columns">The names of the primary key columns.</param>
    /// <returns>An <see cref="ICreateForeignKeyCascadeBuilder"/> to continue building the foreign key.</returns>
    ICreateForeignKeyCascadeBuilder PrimaryColumns(params string[] columns);
}
