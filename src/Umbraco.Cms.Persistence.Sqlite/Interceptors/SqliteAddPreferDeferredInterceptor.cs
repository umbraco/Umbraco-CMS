using System.Data.Common;
using Microsoft.Data.Sqlite;
using NPoco;
using Umbraco.Cms.Persistence.Sqlite.Services;

namespace Umbraco.Cms.Persistence.Sqlite.Interceptors;

public class SqliteAddPreferDeferredInterceptor : SqliteConnectionInterceptor
{
    public override DbConnection OnConnectionOpened(IDatabase database, DbConnection conn)
        => new SqlitePreferDeferredTransactionsConnection(conn as SqliteConnection ??
                                                          throw new InvalidOperationException());
}
