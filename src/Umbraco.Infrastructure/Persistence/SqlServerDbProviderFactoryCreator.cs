using System;
using System.Data.Common;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Persistence
{
    public class SqlServerDbProviderFactoryCreator : IDbProviderFactoryCreator
    {
        private readonly Func<string, DbProviderFactory> _getFactory;

        public SqlServerDbProviderFactoryCreator(Func<string, DbProviderFactory> getFactory)
        {
            _getFactory = getFactory;
        }

        public DbProviderFactory CreateFactory(string providerName)
        {
            if (string.IsNullOrEmpty(providerName)) return null;
            return _getFactory(providerName);
        }

        // gets the sql syntax provider that corresponds, from attribute
        public ISqlSyntaxProvider GetSqlSyntaxProvider(string providerName)
        {
            return providerName switch
            {
                Cms.Core.Constants.DbProviderNames.SqlCe => throw new NotSupportedException("SqlCe is not supported"),
                Cms.Core.Constants.DbProviderNames.SqlServer => new SqlServerSyntaxProvider(),
                _ => throw new InvalidOperationException($"Unknown provider name \"{providerName}\""),
            };
        }

        public IBulkSqlInsertProvider CreateBulkSqlInsertProvider(string providerName)
        {
            switch (providerName)
            {
                case Cms.Core.Constants.DbProviderNames.SqlCe:
                    throw new NotSupportedException("SqlCe is not supported");
                case Cms.Core.Constants.DbProviderNames.SqlServer:
                    return new SqlServerBulkSqlInsertProvider();
                default:
                    return new BasicBulkSqlInsertProvider();
            }
        }

        public void CreateDatabase(string providerName)
        {
            throw new NotSupportedException("Embedded databases are not supported");
        }
    }
}
