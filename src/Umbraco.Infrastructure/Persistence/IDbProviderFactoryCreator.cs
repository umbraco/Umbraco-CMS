using System.Data.Common;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Persistence;

public interface IDbProviderFactoryCreator
{
    DbProviderFactory? CreateFactory(string? providerName);

    ISqlSyntaxProvider GetSqlSyntaxProvider(string providerName);

    IBulkSqlInsertProvider CreateBulkSqlInsertProvider(string providerName);

    void CreateDatabase(string providerName, string connectionString);

    NPocoMapperCollection ProviderSpecificMappers(string providerName);

    IEnumerable<IProviderSpecificInterceptor> GetProviderSpecificInterceptors(string providerName) =>
        Enumerable.Empty<IProviderSpecificInterceptor>();
}
