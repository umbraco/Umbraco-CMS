using System.Data.Common;
using NPoco;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace Umbraco.Cms.Persistence.Sqlite.Interceptors;

public class SqliteAddMiniProfilerInterceptor : SqliteConnectionInterceptor
{
    public override DbConnection OnConnectionOpened(IDatabase database, DbConnection conn)
        => new ProfiledDbConnection(conn, MiniProfiler.Current);
}
