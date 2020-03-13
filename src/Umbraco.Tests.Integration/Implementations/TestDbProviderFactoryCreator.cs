using System.Data.Common;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Integration.Implementations
{
    public class TestDbProviderFactoryCreator : IDbProviderFactoryCreator
    {
        public IBulkSqlInsertProvider CreateBulkSqlInsertProvider(string providerName)
        {
            throw new System.NotImplementedException();
        }

        public void CreateDatabase()
        {
            throw new System.NotImplementedException();
        }

        public DbProviderFactory CreateFactory()
        {
            throw new System.NotImplementedException();
        }

        public DbProviderFactory CreateFactory(string providerName)
        {
            throw new System.NotImplementedException();
        }

        public ISqlSyntaxProvider GetSqlSyntaxProvider(string providerName)
        {
            throw new System.NotImplementedException();
        }
    }
}
