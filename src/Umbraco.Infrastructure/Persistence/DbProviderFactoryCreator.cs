using System.Data.Common;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Provides methods for creating instances of database provider factories used by Umbraco CMS.
/// </summary>
public class DbProviderFactoryCreator : IDbProviderFactoryCreator
{
    private readonly IDictionary<string, IBulkSqlInsertProvider> _bulkSqlInsertProviders;
    private readonly IDictionary<string, IDatabaseCreator> _databaseCreators;
    private readonly Func<string, DbProviderFactory> _getFactory;
    private readonly IEnumerable<IProviderSpecificInterceptor> _providerSpecificInterceptors;
    private readonly IDictionary<string, IProviderSpecificMapperFactory> _providerSpecificMapperFactories;
    private readonly IDictionary<string, ISqlSyntaxProvider> _syntaxProviders;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbProviderFactoryCreator"/> class.
    /// </summary>
    /// <param name="getFactory">A function that retrieves a <see cref="DbProviderFactory"/> based on the specified provider name.</param>
    /// <param name="syntaxProviders">A collection of <see cref="ISqlSyntaxProvider"/> instances representing SQL syntax providers for different database types.</param>
    /// <param name="bulkSqlInsertProviders">A collection of <see cref="IBulkSqlInsertProvider"/> instances used for bulk SQL insert operations.</param>
    /// <param name="databaseCreators">A collection of <see cref="IDatabaseCreator"/> instances responsible for creating databases for specific providers.</param>
    /// <param name="providerSpecificMapperFactories">A collection of <see cref="IProviderSpecificMapperFactory"/> instances for creating provider-specific mappers.</param>
    /// <param name="providerSpecificInterceptors">A collection of <see cref="IProviderSpecificInterceptor"/> instances for handling provider-specific interception logic.</param>
    public DbProviderFactoryCreator(
        Func<string, DbProviderFactory> getFactory,
        IEnumerable<ISqlSyntaxProvider> syntaxProviders,
        IEnumerable<IBulkSqlInsertProvider> bulkSqlInsertProviders,
        IEnumerable<IDatabaseCreator> databaseCreators,
        IEnumerable<IProviderSpecificMapperFactory> providerSpecificMapperFactories,
        IEnumerable<IProviderSpecificInterceptor> providerSpecificInterceptors)
    {
        _getFactory = getFactory;
        _providerSpecificInterceptors = providerSpecificInterceptors;
        _databaseCreators = databaseCreators.ToDictionary(x => x.ProviderName, StringComparer.InvariantCultureIgnoreCase);
        _syntaxProviders = syntaxProviders.ToDictionary(x => x.ProviderName, StringComparer.InvariantCultureIgnoreCase);
        _bulkSqlInsertProviders = bulkSqlInsertProviders.ToDictionary(x => x.ProviderName, StringComparer.InvariantCultureIgnoreCase);
        _providerSpecificMapperFactories = providerSpecificMapperFactories.ToDictionary(x => x.ProviderName, StringComparer.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Creates a <see cref="DbProviderFactory"/> instance for the specified provider name.
    /// </summary>
    /// <param name="providerName">The name of the provider for which to create the factory.</param>
    /// <returns>The <see cref="DbProviderFactory"/> instance if the provider name is valid; otherwise, <c>null</c>.</returns>
    public DbProviderFactory? CreateFactory(string? providerName)
    {
        if (string.IsNullOrEmpty(providerName))
        {
            return null;
        }

        return _getFactory(providerName);
    }

    /// <summary>
    /// Gets the SQL syntax provider that corresponds to the specified provider name.
    /// </summary>
    /// <remarks>gets the sql syntax provider that corresponds, from attribute</remarks>
    /// <param name="providerName">The name of the database provider.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Persistence.ISqlSyntaxProvider" /> instance for the specified provider.</returns>
    public ISqlSyntaxProvider GetSqlSyntaxProvider(string providerName)
    {
        if (!_syntaxProviders.TryGetValue(providerName, out ISqlSyntaxProvider? result))
        {
            throw new InvalidOperationException($"Unknown provider name \"{providerName}\"");
        }

        return result;
    }

    /// <summary>
    /// Creates an <see cref="Umbraco.Cms.Infrastructure.Persistence.IBulkSqlInsertProvider"/> instance for the specified provider name.
    /// </summary>
    /// <param name="providerName">The name of the database provider.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Persistence.IBulkSqlInsertProvider"/> corresponding to the provider name.</returns>
    public IBulkSqlInsertProvider CreateBulkSqlInsertProvider(string providerName)
    {
        if (!_bulkSqlInsertProviders.TryGetValue(providerName, out IBulkSqlInsertProvider? result))
        {
            throw new InvalidOperationException($"Unknown provider name \"{providerName}\"");
        }

        return result;
    }

    /// <summary>
    /// Creates a database using the specified provider name and connection string by delegating to the appropriate database creator.
    /// </summary>
    /// <param name="providerName">The name of the database provider to use for creation.</param>
    /// <param name="connectionString">The connection string to use when creating the database.</param>
    /// <remarks>
    /// If the specified provider is not registered, no action is taken.
    /// </remarks>
    public void CreateDatabase(string providerName, string connectionString)
    {
        if (_databaseCreators.TryGetValue(providerName, out IDatabaseCreator? creator))
        {
            creator.Create(connectionString);
        }
    }

    /// <summary>
    /// Returns the collection of NPoco mappers that are specific to the provided database provider name.
    /// </summary>
    /// <param name="providerName">The name of the database provider for which to retrieve NPoco mappers.</param>
    /// <returns>
    /// A <see cref="NPocoMapperCollection"/> containing the mappers associated with the specified provider,
    /// or an empty collection if no provider-specific mappers are registered.
    /// </returns>
    public NPocoMapperCollection ProviderSpecificMappers(string providerName)
    {
        if (_providerSpecificMapperFactories.TryGetValue(
            providerName,
            out IProviderSpecificMapperFactory? mapperFactory))
        {
            return mapperFactory.Mappers;
        }

        return new NPocoMapperCollection(() => Enumerable.Empty<IMapper>());
    }

    /// <summary>
    /// Returns the provider-specific interceptors associated with the specified provider name.
    /// </summary>
    /// <param name="providerName">The name of the database provider to match.</param>
    /// <returns>An enumerable collection of <see cref="IProviderSpecificInterceptor"/> instances for the given provider.</returns>
    public IEnumerable<IProviderSpecificInterceptor> GetProviderSpecificInterceptors(string providerName)
        => _providerSpecificInterceptors.Where(x => x.ProviderName.Equals(providerName, StringComparison.InvariantCultureIgnoreCase));
}
