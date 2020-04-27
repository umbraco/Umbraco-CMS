using System;
using System.Collections.Generic;
using System.Data.Common;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    public class DbProviderFactoryCreator : IDbProviderFactoryCreator
    {
        private readonly string _defaultProviderName;
        private readonly Func<string, DbProviderFactory> _getFactory;
        private readonly IDictionary<string, ISqlSyntaxProvider> _syntaxProviders;
        private readonly IDictionary<string, IBulkSqlInsertProvider> _bulkSqlInsertProviders;
        private readonly Action _createDatabaseAction;

        public DbProviderFactoryCreator(string defaultProviderName,
            Func<string, DbProviderFactory> getFactory,
            IDictionary<string, ISqlSyntaxProvider> syntaxProviders,
            IDictionary<string,IBulkSqlInsertProvider> bulkSqlInsertProviders,
            Action createDatabaseAction)
        {
            _defaultProviderName = defaultProviderName;
            _getFactory = getFactory;
            _syntaxProviders = syntaxProviders;
            _bulkSqlInsertProviders = bulkSqlInsertProviders;
            _createDatabaseAction = createDatabaseAction;
        }

        public DbProviderFactory CreateFactory() => CreateFactory(_defaultProviderName);

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

        public void CreateDatabase()
        {
            _createDatabaseAction();
        }
    }
}
