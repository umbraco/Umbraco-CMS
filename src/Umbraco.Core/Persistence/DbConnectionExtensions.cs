using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Umbraco.Core.Logging; 

namespace Umbraco.Core.Persistence
{
    internal static class DbConnectionExtensions
    {

        public static DatabaseProviders DetectProviderFromConnectionString(string connString)
        {
            var builder = new DbConnectionStringBuilder {ConnectionString = connString};
            var allKeys = builder.Keys.Cast<string>();

            var mySql = new[] {"Server", "Database", "Uid", "Pwd"};
            if (mySql.All(x => allKeys.InvariantContains(x)))
            {
                return DatabaseProviders.MySql;
            }

            if (allKeys.InvariantContains("Data Source") 
                //this dictionary is case insensitive
                && builder["Data source"].ToString().InvariantContains(".sdf"))
            {
                return DatabaseProviders.SqlServerCE;
            }

            return DatabaseProviders.SqlServer;
        }

        public static bool IsConnectionAvailable(string connString, DatabaseProviders provider)
        {
            DbProviderFactory factory;
            switch (provider)
            {
                case DatabaseProviders.SqlServer:
                case DatabaseProviders.SqlAzure:
                    factory = DbProviderFactories.GetFactory(Constants.DatabaseProviders.SqlServer);
                    break;
                case DatabaseProviders.SqlServerCE:
                    factory = DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");
                    break;
                case DatabaseProviders.MySql:
                    factory = DbProviderFactories.GetFactory(Constants.DatabaseProviders.MySql);
                    break;
                case DatabaseProviders.PostgreSQL:
                case DatabaseProviders.Oracle:
                case DatabaseProviders.SQLite:                    
                default:
                    throw new NotSupportedException("The provider " + provider + " is not supported");
            }

            var conn = factory.CreateConnection();
            if (conn == null)
            {
                throw new InvalidOperationException("Could not create a connection for provider " + provider);
            }
            conn.ConnectionString = connString;
            using (var connection = conn)
            {
                return connection.IsAvailable();
            }
        }

        public static string GetConnStringExSecurityInfo(this IDbConnection connection)
        {
            try
            {
                if (connection is SqlConnection)
                {
                    var builder = new SqlConnectionStringBuilder(connection.ConnectionString);
                    return string.Format("DataSource: {0}, InitialCatalog: {1}", builder.DataSource, builder.InitialCatalog);
                }

                if (connection is SqlCeConnection)
                {
                    var builder = new SqlCeConnectionStringBuilder(connection.ConnectionString);
                    return string.Format("DataSource: {0}", builder.DataSource);
                }

                if (connection is MySqlConnection)
                {
                    var builder = new MySqlConnectionStringBuilder(connection.ConnectionString);
                    return string.Format("Server: {0}, Database: {1}", builder.Server, builder.Database);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WarnWithException(typeof(DbConnectionExtensions),
                    "Could not resolve connection string parameters", ex);
                return "(Could not resolve)";
            }

            throw new ArgumentException(string.Format("The connection type {0} is not supported", connection.GetType()));
        }

        public static bool IsAvailable(this IDbConnection connection)
        {
            try
            {
                connection.Open();
                connection.Close();
            }
            catch (DbException exc)
            {
                // Don't swallow this error, the exception is super handy for knowing "why" its not available
                LogHelper.WarnWithException(typeof(DbConnectionExtensions),
                    "Configured database is reporting as not being available! {0}", exc, connection.GetConnStringExSecurityInfo);
                return false;
            }

            return true;
        }

        
    }
}
