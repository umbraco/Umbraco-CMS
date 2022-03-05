using System.Data.Common;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.FaultHandling;

namespace Umbraco.Cms.Infrastructure.Persistence;

public class RetryDbConnectionInterceptor : IConnectionInterceptor
{
    private readonly RetryPolicy _connectionRetryPolicy;
    private readonly RetryPolicy _commandRetryPolicy;

    public RetryDbConnectionInterceptor(RetryPolicy connectionRetryPolicy, RetryPolicy commandRetryPolicy)
    {
        _connectionRetryPolicy = connectionRetryPolicy;
        _commandRetryPolicy = commandRetryPolicy;
    }

    public DbConnection OnConnectionOpened(IDatabase database, DbConnection conn)
    {
        // wrap the connection with a retrying connection
        if (_connectionRetryPolicy != null || _commandRetryPolicy != null)
        {
            return new RetryDbConnection(conn, _connectionRetryPolicy, _commandRetryPolicy);
        }

        return conn;
    }

    public void OnConnectionClosing(IDatabase database, DbConnection conn)
    {
    }
}
