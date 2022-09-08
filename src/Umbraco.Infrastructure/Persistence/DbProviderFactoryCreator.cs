using System.Data.Common;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Persistence;

public class DbProviderFactoryCreator : IDbProviderFactoryCreator
{
    private readonly IDictionary<string, IBulkSqlInsertProvider> _bulkSqlInsertProviders;
    private readonly IDictionary<string, IDatabaseCreator> _databaseCreators;
    private readonly Func<string, DbProviderFactory> _getFactory;
    private readonly IEnumerable<IProviderSpecificInterceptor> _providerSpecificInterceptors;
    private readonly IDictionary<string, IProviderSpecificMapperFactory> _providerSpecificMapperFactories;
    private readonly IDictionary<string, ISqlSyntaxProvider> _syntaxProviders;

    [Obsolete("Please use an alternative constructor.")]
    public DbProviderFactoryCreator(
        Func<string, DbProviderFactory> getFactory,
        IEnumerable<ISqlSyntaxProvider> syntaxProviders,
        IEnumerable<IBulkSqlInsertProvider> bulkSqlInsertProviders,
        IEnumerable<IDatabaseCreator> databaseCreators,
        IEnumerable<IProviderSpecificMapperFactory> providerSpecificMapperFactories)
        : this(
            getFactory,
            syntaxProviders,
            bulkSqlInsertProviders,
            databaseCreators,
            providerSpecificMapperFactories,
            Enumerable.Empty<IProviderSpecificInterceptor>())
    {
    }

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

    public DbProviderFactory? CreateFactory(string? providerName)
    {
        if (string.IsNullOrEmpty(providerName))
        {
            return null;
        }

        return _getFactory(providerName);
    }

    // gets the sql syntax provider that corresponds, from attribute
    public ISqlSyntaxProvider GetSqlSyntaxProvider(string providerName)
    {
        if (!_syntaxProviders.TryGetValue(providerName, out ISqlSyntaxProvider? result))
        {
            throw new InvalidOperationException($"Unknown provider name \"{providerName}\"");
        }

        return result;
    }

    public IBulkSqlInsertProvider CreateBulkSqlInsertProvider(string providerName)
    {
        if (!_bulkSqlInsertProviders.TryGetValue(providerName, out IBulkSqlInsertProvider? result))
        {
            throw new InvalidOperationException($"Unknown provider name \"{providerName}\"");
        }

        return result;
    }

    public void CreateDatabase(string providerName, string connectionString)
    {
        if (_databaseCreators.TryGetValue(providerName, out IDatabaseCreator? creator))
        {
            creator.Create(connectionString);
        }
    }

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

    public IEnumerable<IProviderSpecificInterceptor> GetProviderSpecificInterceptors(string providerName)
        => _providerSpecificInterceptors.Where(x => x.ProviderName.Equals(providerName, StringComparison.InvariantCultureIgnoreCase));
}
