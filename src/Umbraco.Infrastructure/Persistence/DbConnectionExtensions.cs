using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling.Data;
using Umbraco.Core.Persistence.FaultHandling;

namespace Umbraco.Core.Persistence
{
    public static class DbConnectionExtensions
    {
        public static string DetectProviderNameFromConnectionString(string connectionString)
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
            var allKeys = builder.Keys.Cast<string>();

            if (allKeys.InvariantContains("Data Source")
                //this dictionary is case insensitive
                && builder["Data source"].ToString().InvariantContains(".sdf"))
            {
                return Constants.DbProviderNames.SqlCe;
            }

            return Constants.DbProviderNames.SqlServer;
        }

    public static bool IsConnectionAvailable(string connectionString, DbProviderFactory factory)
        {

            var connection = factory.CreateConnection();

            if (connection == null)
                throw new InvalidOperationException($"Could not create a connection for provider \"{factory}\".");

            connection.ConnectionString = connectionString;
            using (connection)
            {
                return connection.IsAvailable();
            }
        }


        public static bool IsAvailable(this IDbConnection connection)
        {
            try
            {
                connection.Open();
                connection.Close();
            }
            catch (DbException e)
            {
                // Don't swallow this error, the exception is super handy for knowing "why" its not available
                StaticApplicationLogging.Logger.LogWarning(e, "Configured database is reporting as not being available.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Unwraps a database connection.
        /// </summary>
        /// <remarks>UmbracoDatabase wraps the original database connection in various layers (see
        /// OnConnectionOpened); this unwraps and returns the original database connection.</remarks>
        internal static IDbConnection UnwrapUmbraco(this IDbConnection connection)
        {
            var unwrapped = connection;
            IDbConnection c;
            do
            {
                c = unwrapped;
                if (unwrapped is ProfiledDbConnection profiled) unwrapped = profiled.WrappedConnection;
                if (unwrapped is RetryDbConnection retrying) unwrapped = retrying.Inner;

            } while (c != unwrapped);

            return unwrapped;
        }

    }
}
