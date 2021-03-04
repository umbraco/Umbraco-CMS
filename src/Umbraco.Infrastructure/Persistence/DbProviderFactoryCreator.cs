using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Persistence
{
    public class DbProviderFactoryCreator : IDbProviderFactoryCreator
    {
        private readonly Func<string, DbProviderFactory> _getFactory;
        private readonly IDictionary<string, IEmbeddedDatabaseCreator> _embeddedDatabaseCreators;
        private readonly IDictionary<string, ISqlSyntaxProvider> _syntaxProviders;
        private readonly IDictionary<string, IBulkSqlInsertProvider> _bulkSqlInsertProviders;

        public DbProviderFactoryCreator(
            Func<string, DbProviderFactory> getFactory,
            IEnumerable<ISqlSyntaxProvider> syntaxProviders,
            IEnumerable<IBulkSqlInsertProvider> bulkSqlInsertProviders,
            IEnumerable<IEmbeddedDatabaseCreator> embeddedDatabaseCreators)
        {
            _getFactory = getFactory;
            _embeddedDatabaseCreators = embeddedDatabaseCreators.ToDictionary(x=>x.ProviderName);
            _syntaxProviders = syntaxProviders.ToDictionary(x=>x.ProviderName);
            _bulkSqlInsertProviders = bulkSqlInsertProviders.ToDictionary(x=>x.ProviderName);
        }

        public DbProviderFactory CreateFactory(string providerName)
        {
            if (string.IsNullOrEmpty(providerName)) return null;
            return _getFactory(providerName);
        }

        // gets the sql syntax provider that corresponds, from attribute
        public ISqlSyntaxProvider GetSqlSyntaxProvider(string providerName)
        {

            if(!_syntaxProviders.TryGetValue(providerName, out var result))
            {
                throw new InvalidOperationException($"Unknown provider name \"{providerName}\"");
            }

            return result;
        }

        public IBulkSqlInsertProvider CreateBulkSqlInsertProvider(string providerName)
        {

            if(!_bulkSqlInsertProviders.TryGetValue(providerName, out var result))
            {
                return new BasicBulkSqlInsertProvider();
            }

            return result;
        }

        public void CreateDatabase(string providerName)
        {
            if(_embeddedDatabaseCreators.TryGetValue(providerName, out var creator))
            {
                creator.Create();
            }
        }
    }
}
