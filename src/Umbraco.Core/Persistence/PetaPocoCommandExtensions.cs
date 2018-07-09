using System;
using System.Data;
using Umbraco.Core.Persistence.FaultHandling;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Provides a set of extension methods adding retry capabilities into the standard <see cref="System.Data.IDbConnection"/> implementation, which is used in PetaPoco.
    /// </summary>
    public static class PetaPocoCommandExtensions
    {
        #region ExecuteNonQueryWithRetry method implementations
        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected. Uses the default retry policy when executing the command.
        /// </summary>
        /// <param name="command">The command object that is required as per extension method declaration.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQueryWithRetry(this IDbCommand command)
        {
            var connectionString = command.Connection.ConnectionString ?? string.Empty;
            return ExecuteNonQueryWithRetry(command, RetryPolicyFactory.GetDefaultSqlCommandRetryPolicyByConnectionString(connectionString));
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected. Uses the specified retry policy when executing the command.
        /// </summary>
        /// <param name="command">The command object that is required as per extension method declaration.</param>
        /// <param name="retryPolicy">The retry policy defining whether to retry a command if a connection fails while executing the command.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQueryWithRetry(this IDbCommand command, RetryPolicy retryPolicy)
        {
            var connectionString = command.Connection.ConnectionString ?? string.Empty;
            return ExecuteNonQueryWithRetry(command, retryPolicy, RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicyByConnectionString(connectionString));
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected. Uses the specified retry policy when executing the command.
        /// Uses a separate specified retry policy when establishing a connection.
        /// </summary>
        /// <param name="command">The command object that is required as per extension method declaration.</param>
        /// <param name="cmdRetryPolicy">The command retry policy defining whether to retry a command if it fails while executing.</param>
        /// <param name="conRetryPolicy">The connection retry policy defining whether to re-establish a connection if it drops while executing the command.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQueryWithRetry(this IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy)
        {
            //GuardConnectionIsNotNull(command);

            // Check if retry policy was specified, if not, use the default retry policy.
            return (cmdRetryPolicy ?? RetryPolicy.NoRetry).ExecuteAction(() =>
            {
                var hasOpenConnection = EnsureValidConnection(command, conRetryPolicy);

                try
                {
                    return command.ExecuteNonQuery();
                }
                finally
                {
                    if (hasOpenConnection && command.Connection != null && command.Connection.State == ConnectionState.Open)
                    {
                        //Connection is closed in PetaPoco, so no need to do it here (?)
                        //command.Connection.Close();
                    }
                }
            });
        }
        #endregion

        #region ExecuteReaderWithRetry method implementations
        /// <summary>
        /// Sends the specified command to the connection and builds a SqlDataReader object containing the results.
        /// Uses the default retry policy when executing the command.
        /// </summary>
        /// <param name="command">The command object that is required as per extension method declaration.</param>
        /// <returns>A System.Data.IDataReader object.</returns>
        public static IDataReader ExecuteReaderWithRetry(this IDbCommand command)
        {
            var connectionString = command.Connection.ConnectionString ?? string.Empty;
            return ExecuteReaderWithRetry(command, RetryPolicyFactory.GetDefaultSqlCommandRetryPolicyByConnectionString(connectionString));
        }

        /// <summary>
        /// Sends the specified command to the connection and builds a SqlDataReader object containing the results.
        /// Uses the specified retry policy when executing the command.
        /// </summary>
        /// <param name="command">The command object that is required as per extension method declaration.</param>
        /// <param name="retryPolicy">The retry policy defining whether to retry a command if a connection fails while executing the command.</param>
        /// <returns>A System.Data.IDataReader object.</returns>
        public static IDataReader ExecuteReaderWithRetry(this IDbCommand command, RetryPolicy retryPolicy)
        {
            var connectionString = command.Connection.ConnectionString ?? string.Empty;
            return ExecuteReaderWithRetry(command, retryPolicy, RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicyByConnectionString(connectionString));
        }

        /// <summary>
        /// Sends the specified command to the connection and builds a SqlDataReader object containing the results.
        /// Uses the specified retry policy when executing the command. Uses a separate specified retry policy when
        /// establishing a connection.
        /// </summary>
        /// <param name="command">The command object that is required as per extension method declaration.</param>
        /// <param name="cmdRetryPolicy">The command retry policy defining whether to retry a command if it fails while executing.</param>
        /// <param name="conRetryPolicy">The connection retry policy defining whether to re-establish a connection if it drops while executing the command.</param>
        /// <returns>A System.Data.IDataReader object.</returns>
        public static IDataReader ExecuteReaderWithRetry(this IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy)
        {
            //GuardConnectionIsNotNull(command);

            // Check if retry policy was specified, if not, use the default retry policy.
            return (cmdRetryPolicy ?? RetryPolicy.NoRetry).ExecuteAction(() =>
            {
                var hasOpenConnection = EnsureValidConnection(command, conRetryPolicy);

                try
                {
                    return command.ExecuteReader();
                }
                catch (Exception)
                {
                    if (hasOpenConnection && command.Connection != null && command.Connection.State == ConnectionState.Open)
                    {
                        //command.Connection.Close();
                    }

                    throw;
                }
            });
        }

        /// <summary>
        /// Sends the specified command to the connection and builds a SqlDataReader object using one of the 
        /// CommandBehavior values. Uses the default retry policy when executing the command.
        /// </summary>
        /// <param name="command">The command object that is required as per extension method declaration.</param>
        /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        /// <returns>A System.Data.IDataReader object.</returns>
        public static IDataReader ExecuteReaderWithRetry(this IDbCommand command, CommandBehavior behavior)
        {
            var connectionString = command.Connection.ConnectionString ?? string.Empty;
            return ExecuteReaderWithRetry(command, behavior, RetryPolicyFactory.GetDefaultSqlCommandRetryPolicyByConnectionString(connectionString));
        }

        /// <summary>
        /// Sends the specified command to the connection and builds a SqlDataReader object using one of the
        /// CommandBehavior values. Uses the specified retry policy when executing the command.
        /// </summary>
        /// <param name="command">The command object that is required as per extension method declaration.</param>
        /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        /// <param name="retryPolicy">The retry policy defining whether to retry a command if a connection fails while executing the command.</param>
        /// <returns>A System.Data.SqlClient.SqlDataReader object.</returns>
        public static IDataReader ExecuteReaderWithRetry(this IDbCommand command, CommandBehavior behavior, RetryPolicy retryPolicy)
        {
            var connectionString = command.Connection.ConnectionString ?? string.Empty;
            return ExecuteReaderWithRetry(command, behavior, retryPolicy, RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicyByConnectionString(connectionString));
        }

        /// <summary>
        /// Sends the specified command to the connection and builds a SqlDataReader object using one of the
        /// CommandBehavior values. Uses the specified retry policy when executing the command.
        /// Uses a separate specified retry policy when establishing a connection.
        /// </summary>
        /// <param name="command">The command object that is required as per extension method declaration.</param>
        /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        /// <param name="cmdRetryPolicy">The command retry policy defining whether to retry a command if it fails while executing.</param>
        /// <param name="conRetryPolicy">The connection retry policy defining whether to re-establish a connection if it drops while executing the command.</param>
        /// <returns>A System.Data.IDataReader object.</returns>
        public static IDataReader ExecuteReaderWithRetry(this IDbCommand command, CommandBehavior behavior, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy)
        {
            //GuardConnectionIsNotNull(command);

            // Check if retry policy was specified, if not, use the default retry policy.
            return (cmdRetryPolicy ?? RetryPolicy.NoRetry).ExecuteAction(() =>
            {
                var hasOpenConnection = EnsureValidConnection(command, conRetryPolicy);

                try
                {
                    return command.ExecuteReader(behavior);
                }
                catch (Exception)
                {
                    if (hasOpenConnection && command.Connection != null && command.Connection.State == ConnectionState.Open)
                    {
                        //command.Connection.Close();
                    }

                    throw;
                }
            });
        }
        #endregion

        #region ExecuteScalarWithRetry method implementations
        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
        /// Uses the default retry policy when executing the command.
        /// </summary>
        /// <param name="command">The command object that is required as per extension method declaration.</param>
        /// <returns> The first column of the first row in the result set, or a null reference if the result set is empty. Returns a maximum of 2033 characters.</returns>
        public static object ExecuteScalarWithRetry(this IDbCommand command)
        {
            var connectionString = command.Connection.ConnectionString ?? string.Empty;
            return ExecuteScalarWithRetry(command, RetryPolicyFactory.GetDefaultSqlCommandRetryPolicyByConnectionString(connectionString));
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
        /// Uses the specified retry policy when executing the command.
        /// </summary>
        /// <param name="command">The command object that is required as per extension method declaration.</param>
        /// <param name="retryPolicy">The retry policy defining whether to retry a command if a connection fails while executing the command.</param>
        /// <returns> The first column of the first row in the result set, or a null reference if the result set is empty. Returns a maximum of 2033 characters.</returns>
        public static object ExecuteScalarWithRetry(this IDbCommand command, RetryPolicy retryPolicy)
        {
            var connectionString = command.Connection.ConnectionString ?? string.Empty;
            return ExecuteScalarWithRetry(command, retryPolicy, RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicyByConnectionString(connectionString));
        }
        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
        /// Uses the specified retry policy when executing the command. Uses a separate specified retry policy when establishing a connection.
        /// </summary>
        /// <param name="command">The command object that is required as per extension method declaration.</param>
        /// <param name="cmdRetryPolicy">The command retry policy defining whether to retry a command if it fails while executing.</param>
        /// <param name="conRetryPolicy">The connection retry policy defining whether to re-establish a connection if it drops while executing the command.</param>
        /// <returns> The first column of the first row in the result set, or a null reference if the result set is empty. Returns a maximum of 2033 characters.</returns>
        public static object ExecuteScalarWithRetry(this IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy)
        {
            //GuardConnectionIsNotNull(command);

            // Check if retry policy was specified, if not, use the default retry policy.
            return (cmdRetryPolicy ?? RetryPolicy.NoRetry).ExecuteAction(() =>
            {
                var hasOpenConnection = EnsureValidConnection(command, conRetryPolicy);

                try
                {
                    return command.ExecuteScalar();
                }
                finally
                {
                    if (hasOpenConnection && command.Connection != null && command.Connection.State == ConnectionState.Open)
                    {
                        //Connection is closed in PetaPoco, so no need to do it here (?)
                        //command.Connection.Close();
                    }
                }
            });
        }
        #endregion

        /// <summary>
        /// Ensure a valid connection in case a connection hasn't been opened by PetaPoco (which shouldn't be possible by the way).
        /// </summary>
        /// <param name="command"></param>
        /// <param name="retryPolicy"></param>
        /// <returns></returns>
        private static bool EnsureValidConnection(IDbCommand command, RetryPolicy retryPolicy)
        {
            if (command != null)
            {
                //GuardConnectionIsNotNull(command);

                // Verify whether or not the connection is valid and is open. This code may be retried therefore
                // it is important to ensure that a connection is re-established should it have previously failed.
                if (command.Connection.State != ConnectionState.Open)
                {
                    // Attempt to open the connection using the retry policy that matches the policy for SQL commands.
                    command.Connection.OpenWithRetry(retryPolicy);

                    return true;
                }
            }

            return false;
        }
    }
}