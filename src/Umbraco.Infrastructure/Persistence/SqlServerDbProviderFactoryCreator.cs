using System;
using System.Data.Common;
using System.Linq;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Persistence
{
    [Obsolete("This is only used for integration tests and should be moved into a test project.")]
    public class SqlServerDbProviderFactoryCreator : IDbProviderFactoryCreator
    {
        private readonly Func<string, DbProviderFactory> _getFactory;
        private readonly IOptions<GlobalSettings> _globalSettings;

        public SqlServerDbProviderFactoryCreator(Func<string, DbProviderFactory> getFactory, IOptions<GlobalSettings> globalSettings)
        {
            _getFactory = getFactory;
            _globalSettings = globalSettings;
        }

        public DbProviderFactory CreateFactory(string providerName)
        {
            if (string.IsNullOrEmpty(providerName)) return null;

            return _getFactory(providerName);
        }

        // gets the sql syntax provider that corresponds, from attribute
        public ISqlSyntaxProvider GetSqlSyntaxProvider(string providerName)
            => providerName switch
            {
                Cms.Core.Constants.DbProviderNames.SqlCe => throw new NotSupportedException("SqlCe is not supported"),
                Cms.Core.Constants.DbProviderNames.SqlServer => new SqlServerSyntaxProvider(_globalSettings),
                _ => throw new InvalidOperationException($"Unknown provider name \"{providerName}\""),
            };

        public IBulkSqlInsertProvider CreateBulkSqlInsertProvider(string providerName)
            => providerName switch
            {
                Cms.Core.Constants.DbProviderNames.SqlCe => throw new NotSupportedException("SqlCe is not supported"),
                Cms.Core.Constants.DbProviderNames.SqlServer => new SqlServerBulkSqlInsertProvider(),
                _ => new BasicBulkSqlInsertProvider(),
            };

        public void CreateDatabase(string providerName, string connectionString)
            => throw new NotSupportedException("Embedded databases are not supported"); // TODO But LocalDB is?

        public NPocoMapperCollection ProviderSpecificMappers(string providerName)
            => new NPocoMapperCollection(() => Enumerable.Empty<IMapper>());
    }
}
