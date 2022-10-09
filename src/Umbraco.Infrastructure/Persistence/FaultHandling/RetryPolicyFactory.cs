using Umbraco.Cms.Infrastructure.Persistence.FaultHandling.Strategies;

namespace Umbraco.Cms.Infrastructure.Persistence.FaultHandling;

// TODO: These should move to Persistence.SqlServer

/// <summary>
///     Provides a factory class for instantiating application-specific retry policies.
/// </summary>
public static class RetryPolicyFactory
{
    public static RetryPolicy GetDefaultSqlConnectionRetryPolicyByConnectionString(string? connectionString) =>

        // Is this really the best way to determine if the database is an Azure database?
        connectionString?.Contains("database.windows.net") ?? false
            ? GetDefaultSqlAzureConnectionRetryPolicy()
            : GetDefaultSqlConnectionRetryPolicy();

    public static RetryPolicy GetDefaultSqlConnectionRetryPolicy()
    {
        RetryStrategy retryStrategy = RetryStrategy.DefaultExponential;
        var retryPolicy = new RetryPolicy(new NetworkConnectivityErrorDetectionStrategy(), retryStrategy);

        return retryPolicy;
    }

    public static RetryPolicy GetDefaultSqlAzureConnectionRetryPolicy()
    {
        RetryStrategy retryStrategy = RetryStrategy.DefaultExponential;
        var retryPolicy = new RetryPolicy(new SqlAzureTransientErrorDetectionStrategy(), retryStrategy);
        return retryPolicy;
    }

    public static RetryPolicy GetDefaultSqlCommandRetryPolicyByConnectionString(string? connectionString) =>

        // Is this really the best way to determine if the database is an Azure database?
        connectionString?.Contains("database.windows.net") ?? false
            ? GetDefaultSqlAzureCommandRetryPolicy()
            : GetDefaultSqlCommandRetryPolicy();

    public static RetryPolicy GetDefaultSqlCommandRetryPolicy()
    {
        RetryStrategy retryStrategy = RetryStrategy.DefaultFixed;
        var retryPolicy = new RetryPolicy(new NetworkConnectivityErrorDetectionStrategy(), retryStrategy);

        return retryPolicy;
    }

    public static RetryPolicy GetDefaultSqlAzureCommandRetryPolicy()
    {
        RetryStrategy retryStrategy = RetryStrategy.DefaultFixed;
        var retryPolicy = new RetryPolicy(new SqlAzureTransientErrorDetectionStrategy(), retryStrategy);

        return retryPolicy;
    }
}
