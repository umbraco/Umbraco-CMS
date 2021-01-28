using System;
using System.Data.Common;
using System.Data.SqlServerCe;
using Umbraco.Core;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Persistence.SqlCe;

namespace Umbraco.Web
{
    public class UmbracoDbProviderFactoryCreator : IDbProviderFactoryCreator
    {
        public UmbracoDbProviderFactoryCreator()
        {
        }


        public DbProviderFactory CreateFactory(string providerName)
        {
            if (string.IsNullOrEmpty(providerName)) return null;

            return DbProviderFactories.GetFactory(providerName);
        }

        // gets the sql syntax provider that corresponds, from attribute
        public ISqlSyntaxProvider GetSqlSyntaxProvider(string providerName)
        {
            switch (providerName)
            {
                case Constants.DbProviderNames.SqlCe:
                    return new SqlCeSyntaxProvider();
                case Constants.DbProviderNames.SqlServer:
                    return new SqlServerSyntaxProvider();
                default:
                    throw new InvalidOperationException($"Unknown provider name \"{providerName}\"");
            }
        }

        public IBulkSqlInsertProvider CreateBulkSqlInsertProvider(string providerName)
        {
            switch (providerName)
            {
                case Constants.DbProviderNames.SqlCe:
                    return new SqlCeBulkSqlInsertProvider();
                case Constants.DbProviderNames.SqlServer:
                    return new SqlServerBulkSqlInsertProvider();
                default:
                    return new BasicBulkSqlInsertProvider();
            }
        }

        public void CreateDatabase(string providerName)
        {
            var engine = new SqlCeEngine(DatabaseBuilder.EmbeddedDatabaseConnectionString);
            engine.CreateDatabase();
        }
    }
}
