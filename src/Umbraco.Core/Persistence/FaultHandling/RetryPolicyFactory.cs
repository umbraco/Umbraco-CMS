using Umbraco.Core.Persistence.FaultHandling.Strategies;

namespace Umbraco.Core.Persistence.FaultHandling
{
    /// <summary>
    /// Provides a factory class for instantiating application-specific retry policies.
    /// </summary>
    public static class RetryPolicyFactory
    {
        public static RetryPolicy GetDefaultSqlConnectionRetryPolicyByConnectionString(string connectionString)
        {
            //Is this really the best way to determine if the database is an Azure database?
            return connectionString.Contains("database.windows.net")
                       ? GetDefaultSqlAzureConnectionRetryPolicy()
                       : GetDefaultSqlConnectionRetryPolicy();
        }

        public static RetryPolicy GetDefaultSqlConnectionRetryPolicy()
        {
            var retryStrategy = RetryStrategy.DefaultExponential;
            var retryPolicy = new RetryPolicy(new NetworkConnectivityErrorDetectionStrategy(), retryStrategy);

            return retryPolicy;
        }

        public static RetryPolicy GetDefaultSqlAzureConnectionRetryPolicy()
        {
            var retryStrategy = RetryStrategy.DefaultExponential;
            var retryPolicy = new RetryPolicy(new SqlAzureTransientErrorDetectionStrategy(), retryStrategy);
            return retryPolicy;
        }

        public static RetryPolicy GetDefaultSqlCommandRetryPolicyByConnectionString(string connectionString)
        {
            //Is this really the best way to determine if the database is an Azure database?
            return connectionString.Contains("database.windows.net")
                       ? GetDefaultSqlAzureCommandRetryPolicy()
                       : GetDefaultSqlCommandRetryPolicy();
        }

        public static RetryPolicy GetDefaultSqlCommandRetryPolicy()
        {
            var retryStrategy = RetryStrategy.DefaultFixed;
            var retryPolicy = new RetryPolicy(new NetworkConnectivityErrorDetectionStrategy(), retryStrategy);

            return retryPolicy;
        }

        public static RetryPolicy GetDefaultSqlAzureCommandRetryPolicy()
        {
            var retryStrategy = RetryStrategy.DefaultFixed;
            var retryPolicy = new RetryPolicy(new SqlAzureTransientErrorDetectionStrategy(), retryStrategy);

            return retryPolicy;
        }
    }
}
