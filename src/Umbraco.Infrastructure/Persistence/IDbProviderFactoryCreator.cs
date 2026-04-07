using System.Data.Common;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Defines a service for creating instances of database provider factories.
/// </summary>
public interface IDbProviderFactoryCreator
{
    /// <summary>
    /// Creates a <see cref="System.Data.Common.DbProviderFactory"/> instance for the specified provider name.
    /// </summary>
    /// <param name="providerName">The name of the provider to create the factory for.</param>
    /// <returns>The <see cref="System.Data.Common.DbProviderFactory"/> instance if found; otherwise, null.</returns>
    DbProviderFactory? CreateFactory(string? providerName);

    /// <summary>
    /// Gets the SQL syntax provider for the specified database provider name.
    /// </summary>
    /// <param name="providerName">The name of the database provider.</param>
    /// <returns>An instance of <see cref="Umbraco.Cms.Infrastructure.Persistence.ISqlSyntaxProvider"/> corresponding to the provider name.</returns>
    ISqlSyntaxProvider GetSqlSyntaxProvider(string providerName);

    /// <summary>
    /// Creates a bulk SQL insert provider for the specified database provider.
    /// </summary>
    /// <param name="providerName">The name of the database provider.</param>
    /// <returns>An instance of <see cref="Umbraco.Cms.Infrastructure.Persistence.IBulkSqlInsertProvider" />.</returns>
    IBulkSqlInsertProvider CreateBulkSqlInsertProvider(string providerName);

    /// <summary>
    /// Creates a new database using the specified provider and connection string.
    /// </summary>
    /// <param name="providerName">The name of the database provider.</param>
    /// <param name="connectionString">The connection string to use for database creation.</param>
    void CreateDatabase(string providerName, string connectionString);

    /// <summary>
    /// Returns the collection of NPoco mappers specific to the specified database provider.
    /// </summary>
    /// <param name="providerName">The name of the database provider.</param>
    /// <returns>The NPoco mappers associated with the given provider.</returns>
    NPocoMapperCollection ProviderSpecificMappers(string providerName);

    /// <summary>
    /// Gets the provider-specific interceptors for the specified database provider name.
    /// </summary>
    /// <param name="providerName">The name of the database provider.</param>
    /// <returns>An enumerable of provider-specific interceptors for the given provider name; returns an empty enumerable if none are found.</returns>
    IEnumerable<IProviderSpecificInterceptor> GetProviderSpecificInterceptors(string providerName) =>
        Enumerable.Empty<IProviderSpecificInterceptor>();
}
