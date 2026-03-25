using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Table;

/// <summary>
/// Provides a builder interface for defining columns when creating a new table as part of a database migration.
/// </summary>
public interface ICreateTableWithColumnBuilder : IFluentBuilder, IExecutableBuilder
{
    /// <summary>
    /// Adds a new column with the specified name to the table being created.
    /// </summary>
    /// <param name="name">The name of the column to add.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Table.ICreateTableColumnAsTypeBuilder" /> to specify the column type and constraints.</returns>
    ICreateTableColumnAsTypeBuilder WithColumn(string name);
}
