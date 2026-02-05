using System.Data.Common;
using NPoco;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace Umbraco.Cms.Persistence.SqlServer.Interceptors;

/// <summary>
/// Wraps SQL Server connections with MiniProfiler for performance profiling.
/// </summary>
public class SqlServerAddMiniProfilerInterceptor : SqlServerConnectionInterceptor
{
    /// <inheritdoc />
    public override DbConnection OnConnectionOpened(IDatabase database, DbConnection conn)
    {
        // Only wrap with ProfiledDbConnection when profiling is active.
        // When MiniProfiler.Current is null, wrapping can cause issues with
        // EF Core integration and the MiniProfilerMiddleware.
        MiniProfiler? profiler = MiniProfiler.Current;
        return profiler is not null ? new ProfiledDbConnection(conn, profiler) : conn;
    }
}
