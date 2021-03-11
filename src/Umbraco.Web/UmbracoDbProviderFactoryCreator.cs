using System;
using System.Data.Common;
using System.Data.SqlServerCe;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Persistence.SqlCe;
using Constants = Umbraco.Cms.Core.Constants;

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
                    return new SqlCeSyntaxProvider(Options.Create(new GlobalSettings()));
                case Constants.DbProviderNames.SqlServer:
                    return new SqlServerSyntaxProvider(Options.Create(new GlobalSettings()));
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
