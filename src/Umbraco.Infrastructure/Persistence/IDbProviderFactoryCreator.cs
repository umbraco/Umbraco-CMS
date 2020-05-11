using System.Data.Common;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{

    public interface IDbProviderFactoryCreator
    {
        DbProviderFactory CreateFactory(string providerName);
        ISqlSyntaxProvider GetSqlSyntaxProvider(string providerName);
        IBulkSqlInsertProvider CreateBulkSqlInsertProvider(string providerName);
        void CreateDatabase(string providerName);
    }
}
