using System;
using System.Configuration;
using System.Data.Common;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Provides access to the PetaPoco database as Singleton, so the database is created once in app lifetime.
    /// This is necessary for transactions to work properly
    /// </summary>
    public sealed class DatabaseFactory
    {
        #region Singleton

        private const string ConnectionStringName = "umbracoDbDSN";
        private static readonly Database _database = new Database(ConnectionStringName);
        private static readonly Lazy<DatabaseFactory> lazy = new Lazy<DatabaseFactory>(() => new DatabaseFactory());

        public static DatabaseFactory Current { get { return lazy.Value; } }

        private DatabaseFactory()
        {
        }

        #endregion

        /// <summary>
        /// Returns an instance of the PetaPoco database
        /// </summary>
        public Database Database
        {
            get { return _database; }
        }

        /// <summary>
        /// Returns the name of the dataprovider from the connectionstring setting in config
        /// </summary>
        public string ProviderName
        {
            get
            {
                var providerName = "System.Data.SqlClient";
                if (ConfigurationManager.ConnectionStrings[ConnectionStringName] != null)
                {
                    if (!string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[ConnectionStringName].ProviderName))
                        providerName = ConfigurationManager.ConnectionStrings[ConnectionStringName].ProviderName;
                }
                else
                {
                    throw new InvalidOperationException("Can't find a connection string with the name '" + ConnectionStringName + "'");
                }
                return providerName;
            }
        }

        /// <summary>
        /// Returns the Type of DatabaseProvider used
        /// </summary>
        public DatabaseProviders DatabaseProvider
        {
            get
            {
                var factory = DbProviderFactories.GetFactory(ProviderName);

                string dbtype = (factory.GetType()).Name;

                if (dbtype.StartsWith("MySql")) return DatabaseProviders.MySql;
                if (dbtype.StartsWith("SqlCe")) return DatabaseProviders.SqlServerCE;
                if (dbtype.StartsWith("Npgsql")) return DatabaseProviders.PostgreSQL;
                if (dbtype.StartsWith("Oracle")) return DatabaseProviders.Oracle;
                if (dbtype.StartsWith("SQLite")) return DatabaseProviders.SQLite;

                return DatabaseProviders.SqlServer;
            }
        }
    }
}