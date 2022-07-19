using System.Data.Common;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence.FaultHandling;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.SqlServer.Interceptors;

public class SqlServerAddRetryPolicyInterceptor : SqlServerConnectionInterceptor
{
    private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;

    public SqlServerAddRetryPolicyInterceptor(IOptionsMonitor<ConnectionStrings> connectionStrings)
        => _connectionStrings = connectionStrings;

    public override DbConnection OnConnectionOpened(IDatabase database, DbConnection conn)
    {
        if (!_connectionStrings.CurrentValue.IsConnectionStringConfigured())
        {
            return conn;
        }

        RetryPolicy? connectionRetryPolicy =
            RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicyByConnectionString(_connectionStrings.CurrentValue
                .ConnectionString);
        RetryPolicy? commandRetryPolicy =
            RetryPolicyFactory.GetDefaultSqlCommandRetryPolicyByConnectionString(_connectionStrings.CurrentValue
                .ConnectionString);

        if (connectionRetryPolicy == null && commandRetryPolicy == null)
        {
            return conn;
        }

        return new RetryDbConnection(conn, connectionRetryPolicy, commandRetryPolicy);
    }
}
