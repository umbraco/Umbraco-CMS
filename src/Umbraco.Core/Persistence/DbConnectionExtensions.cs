using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
                    break;
                case DatabaseProviders.SqlServerCE:
                    factory = DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");
                    break;
                case DatabaseProviders.MySql:
                    factory = DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
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
                LogHelper.WarnWithException<IDbConnection>("Configured database is reporting as not being available!", exc);
                return false;
            }

            return true;
        }

        
    }
}
