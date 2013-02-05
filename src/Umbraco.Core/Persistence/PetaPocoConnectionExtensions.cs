using System.Data;
using Umbraco.Core.Persistence.FaultHandling;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Provides a set of extension methods adding retry capabilities into the standard <see cref="System.Data.IDbConnection"/> interface, which is used in PetaPoco.
    /// </summary>
    public static class PetaPocoConnectionExtensions
    {
        /// <summary>
        /// Opens a database connection with the connection settings specified in the ConnectionString property of the connection object.
        /// Uses the default retry policy when opening the connection.
        /// </summary>
        /// <param name="connection">The connection object that is required as per extension method declaration.</param>
        public static void OpenWithRetry(this IDbConnection connection)
        {
            var connectionString = connection.ConnectionString ?? string.Empty;
            var retryPolicy = RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicyByConnectionString(connectionString);
            OpenWithRetry(connection, retryPolicy);
        }

        /// <summary>
        /// Opens a database connection with the connection settings specified in the ConnectionString property of the connection object.
        /// Uses the specified retry policy when opening the connection.
        /// </summary>
        /// <param name="connection">The connection object that is required as per extension method declaration.</param>
        /// <param name="retryPolicy">The retry policy defining whether to retry a request if the connection fails to be opened.</param>
        public static void OpenWithRetry(this IDbConnection connection, RetryPolicy retryPolicy)
        {
            // Check if retry policy was specified, if not, use the default retry policy.
            (retryPolicy != null ? retryPolicy : RetryPolicy.NoRetry).ExecuteAction(connection.Open);
        }
    }
}