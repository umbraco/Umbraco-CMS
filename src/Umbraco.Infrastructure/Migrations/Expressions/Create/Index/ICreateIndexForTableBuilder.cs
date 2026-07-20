namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Index;

/// <summary>
/// Provides a fluent interface for building and configuring an index on a database table during a migration.
/// </summary>
public interface ICreateIndexForTableBuilder : IFluentBuilder
{
    /// <summary>
    /// Specifies the table on which the index will be created.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Index.ICreateIndexOnColumnBuilder" /> to define the index columns.</returns>
    ICreateIndexOnColumnBuilder OnTable(string tableName);
}
