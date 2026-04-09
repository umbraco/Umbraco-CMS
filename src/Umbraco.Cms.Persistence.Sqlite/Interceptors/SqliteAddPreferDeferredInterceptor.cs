using System.Data.Common;
using Microsoft.Data.Sqlite;
using NPoco;
using Umbraco.Cms.Persistence.Sqlite.Services;

namespace Umbraco.Cms.Persistence.Sqlite.Interceptors;

/// <summary>
/// Wraps SQLite connections to use deferred transactions, preventing immediate write locks.
/// </summary>
public class SqliteAddPreferDeferredInterceptor : SqliteConnectionInterceptor
{
    /// <inheritdoc />
    public override DbConnection OnConnectionOpened(IDatabase database, DbConnection conn)
        => new SqlitePreferDeferredTransactionsConnection(conn as SqliteConnection ??
                                                          throw new InvalidOperationException());
}
