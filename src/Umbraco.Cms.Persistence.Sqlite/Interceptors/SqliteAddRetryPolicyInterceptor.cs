using System.Data.Common;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.FaultHandling;
using Umbraco.Cms.Persistence.Sqlite.Services;

namespace Umbraco.Cms.Persistence.Sqlite.Interceptors;

public class SqliteAddRetryPolicyInterceptor : SqliteConnectionInterceptor
{
    public override DbConnection OnConnectionOpened(IDatabase database, DbConnection conn)
    {
        RetryStrategy retryStrategy = RetryStrategy.DefaultExponential;
        var commandRetryPolicy = new RetryPolicy(new SqliteTransientErrorDetectionStrategy(), retryStrategy);
        return new RetryDbConnection(conn, null, commandRetryPolicy);
    }
}
