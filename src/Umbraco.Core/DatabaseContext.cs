using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.Linq;
using System.Web.Configuration;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core
{
    /// <summary>
    /// The Umbraco Database context
    /// </summary>
    /// <remarks>
    /// One per AppDomain, represents the Umbraco database
    /// </remarks>
    public class DatabaseContext
    {
        private bool _configured;

        #region Singleton
        private static readonly Lazy<DatabaseContext> lazy = new Lazy<DatabaseContext>(() => new DatabaseContext());
        private string _connectionString;

        /// <summary>
        /// Gets the current Database Context.
        /// </summary>
        public static DatabaseContext Current { get { return lazy.Value; } }

        private DatabaseContext()
        {
        }
        #endregion

        /// <summary>
        /// Gets the <see cref="Database"/> object for doing CRUD operations
        /// against custom tables that resides in the Umbraco database.
        /// </summary>
        /// <remarks>
        /// This should not be used for CRUD operations or queries against the
        /// standard Umbraco tables!
        /// </remarks>
        public Database Database
        {
            get { return DatabaseFactory.Current.Database; }
        }

        /// <summary>
        /// Boolean indicating whether the database has been configured
        /// </summary>
        public bool IsDatabaseConfigured
        {
            get { return _configured; }
        }

        /// <summary>
        /// Gets the configured umbraco db connection string.
        /// </summary>
        public string ConnectionString
        {
            get { return _connectionString; }
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
                string dbtype = Database.Connection == null ? ProviderName : Database.Connection.GetType().Name;

                if (dbtype.StartsWith("MySql")) return DatabaseProviders.MySql;
                if (dbtype.StartsWith("SqlCe") || dbtype.Contains("SqlServerCe")) return DatabaseProviders.SqlServerCE;
                if (dbtype.StartsWith("Npgsql")) return DatabaseProviders.PostgreSQL;
                if (dbtype.StartsWith("Oracle") || dbtype.Contains("OracleClient")) return DatabaseProviders.Oracle;
                if (dbtype.StartsWith("SQLite")) return DatabaseProviders.SQLite;
                if (dbtype.Contains("Azure")) return DatabaseProviders.SqlAzure;

                return DatabaseProviders.SqlServer;
            }
        }

        /// <summary>
        /// Configure a ConnectionString for the embedded database.
        /// </summary>
        public void ConfigureDatabaseConnection()
        {
            var providerName = "System.Data.SqlServerCe.4.0";
            var connectionString = "Datasource=|DataDirectory|Umbraco.sdf";
            var appSettingsConnection = @"datalayer=SQLCE4Umbraco.SqlCEHelper,SQLCE4Umbraco;data source=|DataDirectory|\Umbraco.sdf";

            SaveConnectionString(connectionString, appSettingsConnection, providerName);

            var engine = new SqlCeEngine(connectionString);
            engine.CreateDatabase();

            Initialize();
        }

        /// <summary>
        /// Configure a ConnectionString that has been entered manually.
        /// </summary>
        /// <remarks>
        /// Please note that we currently assume that the 'System.Data.SqlClient' provider can be used.
        /// </remarks>
        /// <param name="connectionString"></param>
        public void ConfigureDatabaseConnection(string connectionString)
        {
            SaveConnectionString(connectionString, connectionString, string.Empty);
            Initialize();
        }

        /// <summary>
        /// Configures a ConnectionString for the Umbraco database based on the passed in properties from the installer.
        /// </summary>
        /// <param name="server">Name or address of the database server</param>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="user">Database Username</param>
        /// <param name="password">Database Password</param>
        /// <param name="databaseProvider">Type of the provider to be used (Sql, Sql Azure, Sql Ce, MySql)</param>
        public void ConfigureDatabaseConnection(string server, string databaseName, string user, string password, string databaseProvider)
        {
            string connectionString;
            string appSettingsConnection;
            string providerName = "System.Data.SqlClient";
            if(databaseProvider.ToLower().Contains("mysql"))
            {
                providerName = "MySql.Data.MySqlClient";
                connectionString = string.Format("Server={0}; Database={1};Uid={2};Pwd={3}", server, databaseName, user, password);
                appSettingsConnection = connectionString;
            }
            else if(databaseProvider.ToLower().Contains("azure"))
            {
                connectionString = string.Format("Server=tcp:{0}.database.windows.net;Database={1};User ID={2}@{0};Password={3}", server, databaseName, user, password);
                appSettingsConnection = connectionString;
            }
            else
            {
                connectionString = string.Format("server={0};database={1};user id={2};password={3}", server, databaseName, user, password);
                appSettingsConnection = connectionString;
            }

            SaveConnectionString(connectionString, appSettingsConnection, providerName);
            Initialize();
        }

        /// <summary>
        /// Saves the connection string as a proper .net ConnectionString and the legacy AppSettings key/value.
        /// </summary>
        /// <remarks>
        /// Saves the ConnectionString in the very nasty 'medium trust'-supportive way.
        /// </remarks>
        /// <param name="connectionString"></param>
        /// <param name="appSettingsConnection"></param>
        /// <param name="providerName"></param>
        private void SaveConnectionString(string connectionString, string appSettingsConnection, string providerName)
        {
            //Set the connection string for the new datalayer
            var connectionStringSettings = string.IsNullOrEmpty(providerName)
                                      ? new ConnectionStringSettings(GlobalSettings.UmbracoConnectionName,
                                                                     connectionString)
                                      : new ConnectionStringSettings(GlobalSettings.UmbracoConnectionName,
                                                                     connectionString, providerName);

            //Set the connection string in appsettings used in the legacy datalayer
            GlobalSettings.DbDsn = appSettingsConnection;
            //ConfigurationManager.ConnectionStrings.Add(conectionString);

            var webConfig = new WebConfigurationFileMap();
            var vDir = GlobalSettings.FullpathToRoot;
            foreach (VirtualDirectoryMapping v in webConfig.VirtualDirectories)
            {
                if (v.IsAppRoot)
                {
                    vDir = v.PhysicalDirectory;
                }
            }

            string fileName = String.Concat(vDir, "web.config");
            var xml = XDocument.Load(fileName);
            var connectionstrings = xml.Root.Descendants("connectionStrings").Single();

            // Update connectionString if it exists, or else create a new appSetting for the given key and value
            var setting = connectionstrings.Descendants("add").FirstOrDefault(s => s.Attribute("name").Value == GlobalSettings.UmbracoConnectionName);
            if (setting == null)
                connectionstrings.Add(new XElement("add", 
                    new XAttribute("name", GlobalSettings.UmbracoConnectionName),
                    new XAttribute("connectionString", connectionStringSettings),
                    new XAttribute("providerName", providerName)));
            else
            {
                setting.Attribute("connectionString").Value = connectionString;
            }

            xml.Save(fileName);
            ConfigurationManager.RefreshSection("connectionStrings");
        }

        /// <summary>
        /// Internal method to initialize the database configuration.
        /// </summary>
        /// <remarks>
        /// If an Umbraco connectionstring exists the database can be configured on app startup,
        /// but if its a new install the entry doesn't exist and the db cannot be configured.
        /// So for new installs the Initialize() method should be called after the connectionstring
        /// has been added to the web.config.
        /// </remarks>
        internal void Initialize()
        {
            if (ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName] != null)
            {
                var providerName = "System.Data.SqlClient";
                if (!string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ProviderName))
                {
                    providerName = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ProviderName;

                    _connectionString =
                        ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ConnectionString;
                    
                }

                if (providerName.StartsWith("MySql"))
                {
                    SyntaxConfig.SqlSyntaxProvider = MySqlSyntax.Provider;
                }
                else if (providerName.Contains("SqlServerCe"))
                {
                    SyntaxConfig.SqlSyntaxProvider = SqlCeSyntax.Provider;
                }
                else
                {
                    SyntaxConfig.SqlSyntaxProvider = SqlServerSyntax.Provider;
                }

                _configured = true;
            }
            else
            {
                _configured = false;
            }
        }
    }
}