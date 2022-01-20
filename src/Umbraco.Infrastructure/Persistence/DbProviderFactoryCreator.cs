using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Persistence
{
    public class DbProviderFactoryCreator : IDbProviderFactoryCreator
    {
        private readonly Func<string, DbProviderFactory> _getFactory;
        private readonly IDictionary<string, IDatabaseCreator> _databaseCreators;
        private readonly IDictionary<string, ISqlSyntaxProvider> _syntaxProviders;
        private readonly IDictionary<string, IBulkSqlInsertProvider> _bulkSqlInsertProviders;
        private readonly IDictionary<string, IProviderSpecificMapperFactory> _providerSpecificMapperFactories;

        public DbProviderFactoryCreator(
            Func<string, DbProviderFactory> getFactory,
            IEnumerable<ISqlSyntaxProvider> syntaxProviders,
            IEnumerable<IBulkSqlInsertProvider> bulkSqlInsertProviders,
            IEnumerable<IDatabaseCreator> databaseCreators,
            IEnumerable<IProviderSpecificMapperFactory> providerSpecificMapperFactories)
        {
            _getFactory = getFactory;
            _databaseCreators = databaseCreators.ToDictionary(x => x.ProviderName);
            _syntaxProviders = syntaxProviders.ToDictionary(x => x.ProviderName);
            _bulkSqlInsertProviders = bulkSqlInsertProviders.ToDictionary(x => x.ProviderName);
            _providerSpecificMapperFactories = providerSpecificMapperFactories.ToDictionary(x => x.ProviderName);
        }

        public DbProviderFactory CreateFactory(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
                return null;
            return _getFactory(providerName);
        }

        // gets the sql syntax provider that corresponds, from attribute
        public ISqlSyntaxProvider GetSqlSyntaxProvider(string providerName)
        {

            if (!_syntaxProviders.TryGetValue(providerName, out var result))
            {
                throw new InvalidOperationException($"Unknown provider name \"{providerName}\"");
            }

            return result;
        }

        public IBulkSqlInsertProvider CreateBulkSqlInsertProvider(string providerName)
        {

            if (!_bulkSqlInsertProviders.TryGetValue(providerName, out var result))
            {
                return new BasicBulkSqlInsertProvider();
            }

            return result;
        }

        public void CreateDatabase(string providerName, string connectionString)
        {
            if (_databaseCreators.TryGetValue(providerName, out var creator))
            {
                creator.Create(connectionString);
            }
        }

        public NPocoMapperCollection ProviderSpecificMappers(string providerName)
        {
            if (_providerSpecificMapperFactories.TryGetValue(providerName, out var mapperFactory))
            {
                return mapperFactory.Mappers;
            }

            return new NPocoMapperCollection(() => Enumerable.Empty<IMapper>());
        }
    }
}
