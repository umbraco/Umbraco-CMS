using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Index;

/// <summary>
/// Provides a builder interface for specifying one or more columns when creating a database index as part of a migration.
/// </summary>
public interface ICreateIndexOnColumnBuilder : IFluentBuilder, IExecutableBuilder
{
    /// <summary>
    ///     Specifies the index column.
    /// </summary>
    ICreateIndexColumnOptionsBuilder OnColumn(string columnName);

    /// <summary>
    ///     Allows further configuration of index options for the current index creation operation.
    /// </summary>
    /// <returns>An <see cref="ICreateIndexOptionsBuilder" /> to specify index options.</returns>
    ICreateIndexOptionsBuilder WithOptions();
}
