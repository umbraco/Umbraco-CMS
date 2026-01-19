using System.Data.Common;
using NPoco;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace Umbraco.Cms.Persistence.Sqlite.Interceptors;

/// <summary>
/// Wraps SQLite connections with MiniProfiler for performance profiling.
/// </summary>
public class SqliteAddMiniProfilerInterceptor : SqliteConnectionInterceptor
{
    /// <inheritdoc />
    public override DbConnection OnConnectionOpened(IDatabase database, DbConnection conn)
        => new ProfiledDbConnection(conn, MiniProfiler.Current);
}
