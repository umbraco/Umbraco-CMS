using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
using MySql.Data.MySqlClient;
using StackExchange.Profiling.Data;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.FaultHandling;

namespace Umbraco.Core.Persistence
{
    internal static class DbConnectionExtensions
    {
        public static string DetectProviderNameFromConnectionString(string connectionString)
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
            var allKeys = builder.Keys.Cast<string>();

            var mySql = new[] { "Server", "Database", "Uid", "Pwd" };
            if (mySql.All(x => allKeys.InvariantContains(x)))
            {
                return Constants.DbProviderNames.MySql;
            }

            if (allKeys.InvariantContains("Data Source")
                //this dictionary is case insensitive
                && builder["Data source"].ToString().InvariantContains(".sdf"))
            {
                return Constants.DbProviderNames.SqlCe;
            }

            return Constants.DbProviderNames.SqlServer;
        }

        public static bool IsConnectionAvailable(string connectionString, string providerName)
        {
            if (providerName != Constants.DbProviderNames.SqlCe
                && providerName != Constants.DbProviderNames.MySql
                && providerName != Constants.DbProviderNames.SqlServer)
                throw new NotSupportedException($"Provider \"{providerName}\" is not supported.");

            var factory = DbProviderFactories.GetFactory(providerName);
            var connection = factory.CreateConnection();

            if (connection == null)
                throw new InvalidOperationException($"Could not create a connection for provider \"{providerName}\".");

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
                Current.Logger.Warn<IDbConnection>(e, "Configured database is reporting as not being available.");
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
                if (unwrapped is ProfiledDbConnection profiled) unwrapped = profiled.InnerConnection;
                if (unwrapped is RetryDbConnection retrying) unwrapped = retrying.Inner;

            } while (c != unwrapped);

            return unwrapped;
        }

        public static string GetConnStringExSecurityInfo(this IDbConnection connection)
        {
            try
            {
                switch (connection)
                {
                    case SqlConnection _:
                    {
                        var builder = new SqlConnectionStringBuilder(connection.ConnectionString);
                        return $"DataSource: {builder.DataSource}, InitialCatalog: {builder.InitialCatalog}";
                    }
                    case SqlCeConnection _:
                    {
                        var builder = new SqlCeConnectionStringBuilder(connection.ConnectionString);
                        return $"DataSource: {builder.DataSource}";
                    }
                    case MySqlConnection _:
                    {
                        var builder = new MySqlConnectionStringBuilder(connection.ConnectionString);
                        return $"Server: {builder.Server}, Database: {builder.Database}";
                    }
                }
            }
            catch (Exception ex)
            {
                Current.Logger.Warn(typeof(DbConnectionExtensions), ex, "Could not resolve connection string parameters");
                return "(Could not resolve)";
            }

            throw new ArgumentException($"The connection type {connection.GetType()} is not supported");
        }
    }
}
