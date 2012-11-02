using System;
using System.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Provides access to the PetaPoco database as Singleton, so the database is created once in app lifetime.
    /// This is necessary for transactions to work properly
    /// </summary>
    public sealed class DatabaseFactory
    {
        #region Singleton

        private static readonly Database _database = new Database(GlobalSettings.UmbracoConnectionName);
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
                if (ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName] != null)
                {
                    if (!string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ProviderName))
                        providerName = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ProviderName;
                }
                else
                {
                    throw new InvalidOperationException("Can't find a connection string with the name '" + GlobalSettings.UmbracoConnectionName + "'");
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
                string dbtype = _database.Connection == null ? ProviderName : _database.Connection.GetType().Name;

                if (dbtype.StartsWith("MySql")) return DatabaseProviders.MySql;
                if (dbtype.StartsWith("SqlCe") || dbtype.Contains("SqlServerCe")) return DatabaseProviders.SqlServerCE;
                if (dbtype.StartsWith("Npgsql")) return DatabaseProviders.PostgreSQL;
                if (dbtype.StartsWith("Oracle")) return DatabaseProviders.Oracle;
                if (dbtype.StartsWith("SQLite")) return DatabaseProviders.SQLite;

                return DatabaseProviders.SqlServer;
            }
        }
    }
}