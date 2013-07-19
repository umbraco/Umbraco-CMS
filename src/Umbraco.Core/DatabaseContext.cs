﻿using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Initial;
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
        private readonly IDatabaseFactory _factory;
        private bool _configured;
        private string _connectionString;
        private string _providerName;
        private DatabaseSchemaResult _result;

        internal DatabaseContext(IDatabaseFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Gets the <see cref="Database"/> object for doing CRUD operations
        /// against custom tables that resides in the Umbraco database.
        /// </summary>
        /// <remarks>
        /// This should not be used for CRUD operations or queries against the
        /// standard Umbraco tables! Use the Public services for that.
        /// </remarks>
        public UmbracoDatabase Database
        {
            get { return _factory.CreateDatabase(); }
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
        internal string ProviderName
        {
            get
            {
                if (string.IsNullOrEmpty(_providerName) == false)
                    return _providerName;

                _providerName = "System.Data.SqlClient";
                if (ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName] != null)
                {
                    if (!string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ProviderName))
                        _providerName = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ProviderName;
                }
                else
                {
                    throw new InvalidOperationException("Can't find a connection string with the name '" + GlobalSettings.UmbracoConnectionName + "'");
                }
                return _providerName;
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
        public void ConfigureEmbeddedDatabaseConnection()
        {
            const string providerName = "System.Data.SqlServerCe.4.0";
            const string connectionString = @"Data Source=|DataDirectory|\Umbraco.sdf";

            SaveConnectionString(connectionString, providerName);

            var path = Path.Combine(GlobalSettings.FullpathToRoot, "App_Data", "Umbraco.sdf");
            if (File.Exists(path) == false)
            {
                var engine = new SqlCeEngine(connectionString);
                engine.CreateDatabase();
            }

            Initialize(providerName);
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
            SaveConnectionString(connectionString, string.Empty);
            Initialize(string.Empty);
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
            string providerName = "System.Data.SqlClient";
            if (databaseProvider.ToLower().Contains("mysql"))
            {
                providerName = "MySql.Data.MySqlClient";
                connectionString = string.Format("Server={0}; Database={1};Uid={2};Pwd={3}", server, databaseName, user, password);
            }
            else if (databaseProvider.ToLower().Contains("azure"))
            {
                connectionString = BuildAzureConnectionString(server, databaseName, user, password);
            }
            else
            {
                connectionString = string.Format("server={0};database={1};user id={2};password={3}", server, databaseName, user, password);
            }

            SaveConnectionString(connectionString, providerName);
            Initialize(providerName);
        }

        /// <summary>
        /// Configures a ConnectionString for the Umbraco database that uses Microsoft SQL Server integrated security.
        /// </summary>
        /// <param name="server">Name or address of the database server</param>
        /// <param name="databaseName">Name of the database</param>
        public void ConfigureIntegratedSecurityDatabaseConnection(string server, string databaseName)
        {
            const string providerName = "System.Data.SqlClient";
            string connectionString = String.Format("Server={0};Database={1};Integrated Security=true", server, databaseName);

            SaveConnectionString(connectionString, providerName);
            Initialize(providerName);
        }

        internal string BuildAzureConnectionString(string server, string databaseName, string user, string password)
        {
            if (server.Contains(".") && ServerStartsWithTcp(server) == false)
                server = string.Format("tcp:{0}", server);
            
            if (server.Contains(".") == false && ServerStartsWithTcp(server))
            {
                string serverName = server.Contains(",") 
                                        ? server.Substring(0, server.IndexOf(",", StringComparison.Ordinal)) 
                                        : server;

                var portAddition = string.Empty;

                if (server.Contains(","))
                    portAddition = server.Substring(server.IndexOf(",", StringComparison.Ordinal));

                server = string.Format("{0}.database.windows.net{1}", serverName, portAddition);
            }
            
            if (ServerStartsWithTcp(server) == false)
                server = string.Format("tcp:{0}.database.windows.net", server);
            
            if (server.Contains(",") == false)
                server = string.Format("{0},1433", server);

            if (user.Contains("@") == false)
            {
                var userDomain = server;

                if (ServerStartsWithTcp(server))
                    userDomain = userDomain.Substring(userDomain.IndexOf(":", StringComparison.Ordinal) + 1);

                if (userDomain.Contains("."))
                    userDomain = userDomain.Substring(0, userDomain.IndexOf(".", StringComparison.Ordinal));

                user = string.Format("{0}@{1}", user, userDomain);
            }

            return string.Format("Server={0};Database={1};User ID={2};Password={3}", server, databaseName, user, password);
        }

        private static bool ServerStartsWithTcp(string server)
        {
            return server.ToLower().StartsWith("tcp:".ToLower());
        }

        /// <summary>
        /// Saves the connection string as a proper .net ConnectionString and the legacy AppSettings key/value.
        /// </summary>
        /// <remarks>
        /// Saves the ConnectionString in the very nasty 'medium trust'-supportive way.
        /// </remarks>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        private void SaveConnectionString(string connectionString, string providerName)
        {
            //Set the connection string for the new datalayer
            var connectionStringSettings = string.IsNullOrEmpty(providerName)
                                      ? new ConnectionStringSettings(GlobalSettings.UmbracoConnectionName,
                                                                     connectionString)
                                      : new ConnectionStringSettings(GlobalSettings.UmbracoConnectionName,
                                                                     connectionString, providerName);

            _connectionString = connectionString;
            _providerName = providerName;

            var webConfig = new WebConfigurationFileMap();
            var vDir = GlobalSettings.FullpathToRoot;
            foreach (VirtualDirectoryMapping v in webConfig.VirtualDirectories)
            {
                if (v.IsAppRoot)
                {
                    vDir = v.PhysicalDirectory;
                }
            }

            var fileName = Path.Combine(vDir, "web.config");
            var xml = XDocument.Load(fileName, LoadOptions.PreserveWhitespace);
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
                setting.Attribute("providerName").Value = providerName;
            }

            xml.Save(fileName, SaveOptions.DisableFormatting);

            LogHelper.Info<DatabaseContext>("Configured a new ConnectionString using the '" + providerName + "' provider.");
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
            var databaseSettings = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName];
            if (databaseSettings != null && string.IsNullOrWhiteSpace(databaseSettings.ConnectionString) == false && string.IsNullOrWhiteSpace(databaseSettings.ProviderName) == false)
            {
                var providerName = "System.Data.SqlClient";
                if (!string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ProviderName))
                {
                    providerName = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ProviderName;

                    _connectionString =
                        ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ConnectionString;

                }

                Initialize(providerName);
            }
            else if (ConfigurationManager.AppSettings.ContainsKey(GlobalSettings.UmbracoConnectionName) && string.IsNullOrEmpty(ConfigurationManager.AppSettings[GlobalSettings.UmbracoConnectionName]) == false)
            {
                //A valid connectionstring does not exist, but the legacy appSettings key was found, so we'll reconfigure the conn.string.
                var legacyConnString = ConfigurationManager.AppSettings[GlobalSettings.UmbracoConnectionName];
                if (legacyConnString.ToLowerInvariant().Contains("sqlce4umbraco"))
                {
                    ConfigureEmbeddedDatabaseConnection();
                }
                else if (legacyConnString.ToLowerInvariant().Contains("tcp:"))
                {
                    //Must be sql azure
                    SaveConnectionString(legacyConnString, "System.Data.SqlClient");
                    Initialize("System.Data.SqlClient");
                }
                else if (legacyConnString.ToLowerInvariant().Contains("datalayer=mysql"))
                {
                    //Must be MySql

                    //Need to strip the datalayer part off
                    var connectionStringWithoutDatalayer = string.Empty;
                    foreach (var variable in legacyConnString.Split(';').Where(x => x.ToLowerInvariant().StartsWith("datalayer") == false))
                        connectionStringWithoutDatalayer = string.Format("{0}{1};", connectionStringWithoutDatalayer, variable);

                    SaveConnectionString(connectionStringWithoutDatalayer, "MySql.Data.MySqlClient");
                    Initialize("MySql.Data.MySqlClient");
                }
                else
                {
                    //Must be sql
                    SaveConnectionString(legacyConnString, "System.Data.SqlClient");
                    Initialize("System.Data.SqlClient");
                }

                //Remove the legacy connection string, so we don't end up in a loop if something goes wrong.
                GlobalSettings.RemoveSetting(GlobalSettings.UmbracoConnectionName);
            }
            else
            {
                _configured = false;
            }
        }

        internal void Initialize(string providerName)
        {
            _providerName = providerName;

            try
            {
                SqlSyntaxContext.SqlSyntaxProvider =
                    SqlSyntaxProvidersResolver.Current.GetByProviderNameOrDefault(providerName);
                
                _configured = true;
            }
            catch (Exception e)
            {
                _configured = false;

                LogHelper.Info<DatabaseContext>("Initialization of the DatabaseContext failed with following error: " + e.Message);
                LogHelper.Info<DatabaseContext>(e.StackTrace);
            }
        }

        internal DatabaseSchemaResult ValidateDatabaseSchema()
        {
            if (_configured == false || (string.IsNullOrEmpty(_connectionString) || string.IsNullOrEmpty(ProviderName)))
                return new DatabaseSchemaResult();

            if (_result == null)
            {
                var database = new UmbracoDatabase(_connectionString, ProviderName);
                var dbSchema = new DatabaseSchemaCreation(database);
                _result = dbSchema.ValidateSchema();
            }
            return _result;
        }

        internal Result CreateDatabaseSchemaAndDataOrUpgrade()
        {
            if (_configured == false || (string.IsNullOrEmpty(_connectionString) || string.IsNullOrEmpty(ProviderName)))
            {
                return new Result
                           {
                               Message =
                                   "Database configuration is invalid. Please check that the entered database exists and that the provided username and password has write access to the database.",
                               Success = false,
                               Percentage = "10"
                           };
            }

            try
            {
                LogHelper.Info<DatabaseContext>("Database configuration status: Started");

                var message = string.Empty;

                var database = new UmbracoDatabase(_connectionString, ProviderName);
                var supportsCaseInsensitiveQueries = SqlSyntaxContext.SqlSyntaxProvider.SupportsCaseInsensitiveQueries(database);
                if (supportsCaseInsensitiveQueries  == false)
                {
                    message = "<p>&nbsp;</p><p>The database you're trying to use does not support case insensitive queries. <br />We currently do not support these types of databases.</p>" +
                              "<p>You can fix this by changing the following two settings in your my.ini file in your MySQL installation directory:</p>" +
                              "<pre>lower_case_table_names=1\nlower_case_file_system=1</pre><br />" +
                              "<p>Note: Make sure to check with your hosting provider if they support case insensitive queries as well.</p>" +
                              "<p>For more technical information on case sensitivity in MySQL, have a look at " +
                              "<a href='http://dev.mysql.com/doc/refman/5.0/en/identifier-case-sensitivity.html'>the documentation on the subject</a></p>";

                    return new Result { Message = message, Success = false, Percentage = "15" };
                }
                else if (supportsCaseInsensitiveQueries == null)
                {
                    message = "<p>&nbsp;</p><p>Warning! Could not check if your database type supports case insensitive queries. <br />We currently do not support these databases that do not support case insensitive queries.</p>" +
                              "<p>You can check this by looking for the following two settings in your my.ini file in your MySQL installation directory:</p>" +
                              "<pre>lower_case_table_names=1\nlower_case_file_system=1</pre><br />" +
                              "<p>Note: Make sure to check with your hosting provider if they support case insensitive queries as well.</p>" +
                              "<p>For more technical information on case sensitivity in MySQL, have a look at " +
                              "<a href='http://dev.mysql.com/doc/refman/5.0/en/identifier-case-sensitivity.html'>the documentation on the subject</a></p>";
                }
                else
                {
                    if (SqlSyntaxContext.SqlSyntaxProvider.GetType() == typeof(MySqlSyntaxProvider))
                    {
                        message = "<p>&nbsp;</p><p>Congratulations, the database step ran successfully!</p>" +
                                  "<p>Note: You're using MySQL and the database instance you're connecting to seems to support case insensitive queries.</p>" +
                                  "<p>However, your hosting provider may not support this option. Umbraco does not currently support MySQL installs that do not support case insensitive queries</p>" +
                                  "<p>Make sure to check with your hosting provider if they support case insensitive queries as well.</p>" +
                                  "<p>They can check this by looking for the following two settings in the my.ini file in their MySQL installation directory:</p>" +
                                  "<pre>lower_case_table_names=1\nlower_case_file_system=1</pre><br />" +
                                  "<p>For more technical information on case sensitivity in MySQL, have a look at " +
                                  "<a href='http://dev.mysql.com/doc/refman/5.0/en/identifier-case-sensitivity.html'>the documentation on the subject</a></p>";
                    }
                }

                var schemaResult = ValidateDatabaseSchema();
                var installedVersion = schemaResult.DetermineInstalledVersion();
                

                //If Configuration Status is empty and the determined version is "empty" its a new install - otherwise upgrade the existing
                if (string.IsNullOrEmpty(GlobalSettings.ConfigurationStatus) && installedVersion.Equals(new Version(0, 0, 0)))
                {
                    database.CreateDatabaseSchema();
                    message = message + "<p>Installation completed!</p>";
                }
                else
                {
                    var configuredVersion = string.IsNullOrEmpty(GlobalSettings.ConfigurationStatus)
                                                ? installedVersion
                                                : new Version(GlobalSettings.ConfigurationStatus);
                    var targetVersion = UmbracoVersion.Current;
                    var runner = new MigrationRunner(configuredVersion, targetVersion, GlobalSettings.UmbracoMigrationName);
                    var upgraded = runner.Execute(database, true);
                    message = message + "<p>Upgrade completed!</p>";
                }

                LogHelper.Info<DatabaseContext>("Database configuration status: " + message);

                return new Result { Message = message, Success = true, Percentage = "100" };
            }
            catch (Exception ex)
            {
                LogHelper.Info<DatabaseContext>("Database configuration failed with the following error and stack trace: " + ex.Message + "\n" + ex.StackTrace);

                if (_result != null)
                {
                    LogHelper.Info<DatabaseContext>("The database schema validation produced the following summary: \n" + _result.GetSummary());
                }

                return new Result
                           {
                               Message =
                                   "The database configuration failed with the following message: " + ex.Message +
                                   "\n Please check log file for additional information (can be found in '/App_Data/Logs/UmbracoTraceLog.txt')",
                               Success = false,
                               Percentage = "90"
                           };
            }
        }

        internal class Result
        {
            public string Message { get; set; }
            public bool Success { get; set; }
            public string Percentage { get; set; }
        }
    }
}