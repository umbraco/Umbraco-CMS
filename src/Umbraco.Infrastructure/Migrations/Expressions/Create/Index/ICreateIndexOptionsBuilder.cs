namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Index;

/// <summary>
/// Provides a builder interface for specifying options when creating an index as part of a database migration.
/// </summary>
public interface ICreateIndexOptionsBuilder : IFluentBuilder
{
    /// <summary>
    /// Specifies that the index to be created should enforce uniqueness.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Index.ICreateIndexOnColumnBuilder" /> to continue building the index.</returns>
    ICreateIndexOnColumnBuilder Unique();

    /// <summary>
    /// Specifies that the index should be created as non-clustered.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Index.ICreateIndexOnColumnBuilder"/> to specify the columns for the index.</returns>
    ICreateIndexOnColumnBuilder NonClustered();

    /// <summary>
    /// Marks the index as clustered.
    /// </summary>
    /// <returns>The <see cref="ICreateIndexOnColumnBuilder"/> instance for further index configuration.</returns>
    ICreateIndexOnColumnBuilder Clustered();
}
