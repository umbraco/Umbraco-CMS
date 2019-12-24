using System.Data.Common;
using StackExchange.Profiling.Internal;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    public interface IDbProviderFactoryCreator
    {
        DbProviderFactory CreateFactory();
        DbProviderFactory CreateFactory(string providerName);
        ISqlSyntaxProvider GetSqlSyntaxProvider(string providerName);
        void CreateDatabase();
    }
}
