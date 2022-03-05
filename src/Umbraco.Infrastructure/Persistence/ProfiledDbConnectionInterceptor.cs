using System.Data.Common;
using NPoco;
using StackExchange.Profiling;

namespace Umbraco.Cms.Infrastructure.Persistence;

public class ProfiledDbConnectionInterceptor : IConnectionInterceptor
{
    public DbConnection OnConnectionOpened(IDatabase database, DbConnection conn)
        => new StackExchange.Profiling.Data.ProfiledDbConnection(conn, MiniProfiler.Current);

    public void OnConnectionClosing(IDatabase database, DbConnection conn)
    {
    }
}
