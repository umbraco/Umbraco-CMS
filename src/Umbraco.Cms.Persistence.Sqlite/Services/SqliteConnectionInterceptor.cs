using System.Data.Common;
using Microsoft.Data.Sqlite;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Persistence.Sqlite.Services;

public class SqliteConnectionInterceptor : IProviderSpecificConnectionInterceptor
{
    public string ProviderName => Constants.ProviderName;

    public DbConnection OnConnectionOpened(IDatabase database, DbConnection conn)
        => new SqlitePreferDeferredTransactionsConnection((SqliteConnection)conn);

    public void OnConnectionClosing(IDatabase database, DbConnection conn)
    {
    }
}
