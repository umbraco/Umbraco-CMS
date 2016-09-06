﻿using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Xml.Linq;
using Semver;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;

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
        private readonly ILogger _logger;
        private readonly SqlSyntaxProviders _syntaxProviders;
        private bool _configured;
        private readonly object _locker = new object();
        private string _connectionString;
        private string _providerName;
        private DatabaseSchemaResult _result;

        [Obsolete("Use the constructor specifying all dependencies instead")]
        public DatabaseContext(IDatabaseFactory factory)
            : this(factory, LoggerResolver.Current.Logger, new SqlSyntaxProviders(new ISqlSyntaxProvider[]
            {
                new MySqlSyntaxProvider(LoggerResolver.Current.Logger),
                new SqlCeSyntaxProvider(), 
                new SqlServerSyntaxProvider()
            }))
        {
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="logger"></param>
        /// <param name="syntaxProviders"></param>
        public DatabaseContext(IDatabaseFactory factory, ILogger logger, SqlSyntaxProviders syntaxProviders)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (logger == null) throw new ArgumentNullException("logger");
            if (syntaxProviders == null) throw new ArgumentNullException("syntaxProviders");

            _factory = factory;
            _logger = logger;
            _syntaxProviders = syntaxProviders;
        }

        /// <summary>
        /// Create a configured DatabaseContext
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="logger"></param>
        /// <param name="sqlSyntax"></param>
        /// <param name="providerName"></param>
        public DatabaseContext(IDatabaseFactory factory, ILogger logger, ISqlSyntaxProvider sqlSyntax, string providerName)
        {
            _providerName = providerName;
            SqlSyntax = sqlSyntax;
            SqlSyntaxContext.SqlSyntaxProvider = SqlSyntax;
            _factory = factory;
            _logger = logger;
            _configured = true;
        }

        public ISqlSyntaxProvider SqlSyntax { get; private set; }

        /// <summary>
        /// Gets the <see cref="Database"/> object for doing CRUD operations
        /// against custom tables that resides in the Umbraco database.
        /// </summary>
        /// <remarks>
        /// This should not be used for CRUD operations or queries against the
        /// standard Umbraco tables! Use the Public services for that.
        /// </remarks>
        public virtual UmbracoDatabase Database
        {
            get { return _factory.CreateDatabase(); }
        }

        /// <summary>
        /// Boolean indicating whether the database has been configured
        /// </summary>
        public virtual bool IsDatabaseConfigured
        {
            get { return _configured; }
        }

        /// <summary>
        /// Determines if the db can be connected to
        /// </summary>
        public virtual bool CanConnect
        {
            get
            {
                if (IsDatabaseConfigured == false) return false;
                var canConnect = DbConnectionExtensions.IsConnectionAvailable(ConnectionString, DatabaseProvider);
                LogHelper.Info<DatabaseContext>("CanConnect = " + canConnect);
                return canConnect;
            }
        }

        /// <summary>
        /// Gets the configured umbraco db connection string.
        /// </summary>
        public virtual string ConnectionString
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

                _providerName = Constants.DatabaseProviders.SqlServer;
                if (ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName] != null)
                {
                    if (string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ProviderName) == false)
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
        public virtual DatabaseProviders DatabaseProvider
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

            var connectionString = GetEmbeddedDatabaseConnectionString();
            SaveConnectionString(connectionString, providerName);

            var path = Path.Combine(GlobalSettings.FullpathToRoot, "App_Data", "Umbraco.sdf");
            if (File.Exists(path) == false)
            {
                var engine = new SqlCeEngine(connectionString);
                engine.CreateDatabase();

                // SD: Pretty sure this should be in a using clause but i don't want to cause unknown side-effects here
                // since it's been like this for quite some time
                //using (var engine = new SqlCeEngine(connectionString))
                //{
                //    engine.CreateDatabase();    
                //}
            }

            Initialize(providerName);
        }

        public string GetEmbeddedDatabaseConnectionString()
        {
            return @"Data Source=|DataDirectory|\Umbraco.sdf;Flush Interval=1;";
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
            var provider = DbConnectionExtensions.DetectProviderFromConnectionString(connectionString);
            var databaseProvider = provider.ToString();
            var providerName = Constants.DatabaseProviders.SqlServer;
            if (databaseProvider.ToLower().Contains("mysql"))
            {
                providerName = Constants.DatabaseProviders.MySql;
            }
            SaveConnectionString(connectionString, providerName);
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
            string providerName;
            var connectionString = GetDatabaseConnectionString(server, databaseName, user, password, databaseProvider, out providerName);

            SaveConnectionString(connectionString, providerName);
            Initialize(providerName);
        }
        
        public string GetDatabaseConnectionString(string server, string databaseName, string user, string password, string databaseProvider, out string providerName)
        {
            providerName = Constants.DatabaseProviders.SqlServer;
            if (databaseProvider.ToLower().Contains("mysql"))
            {
                providerName = Constants.DatabaseProviders.MySql;
                return string.Format("Server={0}; Database={1};Uid={2};Pwd={3}", server, databaseName, user, password);
            }
            if (databaseProvider.ToLower().Contains("azure"))
            {
                return BuildAzureConnectionString(server, databaseName, user, password);
            }
            return string.Format("server={0};database={1};user id={2};password={3}", server, databaseName, user, password);
        }

        /// <summary>
        /// Configures a ConnectionString for the Umbraco database that uses Microsoft SQL Server integrated security.
        /// </summary>
        /// <param name="server">Name or address of the database server</param>
        /// <param name="databaseName">Name of the database</param>
        public void ConfigureIntegratedSecurityDatabaseConnection(string server, string databaseName)
        {
            const string providerName = Constants.DatabaseProviders.SqlServer;
            var connectionString = GetIntegratedSecurityDatabaseConnectionString(server, databaseName);
            SaveConnectionString(connectionString, providerName);
            Initialize(providerName);
        }

        public string GetIntegratedSecurityDatabaseConnectionString(string server, string databaseName)
        {
            return String.Format("Server={0};Database={1};Integrated Security=true", server, databaseName);            
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
            
            var fileName = IOHelper.MapPath(string.Format("{0}/web.config", SystemDirectories.Root));
            var xml = XDocument.Load(fileName, LoadOptions.PreserveWhitespace);
            var connectionstrings = xml.Root.DescendantsAndSelf("connectionStrings").Single();

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

            _logger.Info<DatabaseContext>("Configured a new ConnectionString using the '" + providerName + "' provider.");
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
                var providerName = Constants.DatabaseProviders.SqlServer;
                string connString = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ProviderName))
                {
                    providerName = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ProviderName;
                    connString = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName].ConnectionString;
                }
                Initialize(providerName, connString);
                
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
                    SaveConnectionString(legacyConnString, Constants.DatabaseProviders.SqlServer);
                    Initialize(Constants.DatabaseProviders.SqlServer);
                }
                else if (legacyConnString.ToLowerInvariant().Contains("datalayer=mysql"))
                {
                    //Must be MySql

                    //Need to strip the datalayer part off
                    var connectionStringWithoutDatalayer = string.Empty;
                    foreach (var variable in legacyConnString.Split(';').Where(x => x.ToLowerInvariant().StartsWith("datalayer") == false))
                        connectionStringWithoutDatalayer = string.Format("{0}{1};", connectionStringWithoutDatalayer, variable);

                    SaveConnectionString(connectionStringWithoutDatalayer, Constants.DatabaseProviders.MySql);
                    Initialize(Constants.DatabaseProviders.MySql);
                }
                else
                {
                    //Must be sql
                    SaveConnectionString(legacyConnString, Constants.DatabaseProviders.SqlServer);
                    Initialize(Constants.DatabaseProviders.SqlServer);
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
            //only configure once!
            if (_configured == true) return;

            _providerName = providerName;

            try
            {
                if (_syntaxProviders != null)
                {
                    SqlSyntax = _syntaxProviders.GetByProviderNameOrDefault(providerName);    
                }
                else if (SqlSyntax == null)
                {
                    throw new InvalidOperationException("No " + typeof(ISqlSyntaxProvider) + " specified or no " + typeof(SqlSyntaxProviders) + " instance specified");
                }
                
                SqlSyntaxContext.SqlSyntaxProvider = SqlSyntax;
                
                _configured = true;
            }
            catch (Exception e)
            {
                _configured = false;

                _logger.Info<DatabaseContext>("Initialization of the DatabaseContext failed with following error: " + e.Message);
                _logger.Info<DatabaseContext>(e.StackTrace);
            }
        }

        internal void Initialize(string providerName, string connectionString)
        {
            _connectionString = connectionString;
            Initialize(providerName);
        }

        internal DatabaseSchemaResult ValidateDatabaseSchema()
        {
            if (_configured == false || (string.IsNullOrEmpty(_connectionString) || string.IsNullOrEmpty(ProviderName)))
                return new DatabaseSchemaResult();

            if (_result == null)
            {

                if (SystemUtilities.GetCurrentTrustLevel() != AspNetHostingPermissionLevel.Unrestricted
                    && ProviderName == Constants.DatabaseProviders.MySql)
                {
                    throw new InvalidOperationException("Cannot use MySql in Medium Trust configuration");
                }

                var database = new UmbracoDatabase(_connectionString, ProviderName, _logger);
                var dbSchema = new DatabaseSchemaCreation(database, _logger, SqlSyntax);
                _result = dbSchema.ValidateSchema();
            }
            return _result;
        }

        internal Result CreateDatabaseSchemaAndData(ApplicationContext applicationContext)
        {   
            try
            {
                var readyForInstall = CheckReadyForInstall();
                if (readyForInstall.Success == false)
                {
                    return readyForInstall.Result;
                }

                _logger.Info<DatabaseContext>("Database configuration status: Started");

                string message;

                var database = new UmbracoDatabase(_connectionString, ProviderName, _logger);

                // If MySQL, we're going to ensure that database calls are maintaining proper casing as to remove the necessity for checks
                // for case insensitive queries. In an ideal situation (which is what we're striving for), all calls would be case sensitive.

                /*                
                var supportsCaseInsensitiveQueries = SqlSyntax.SupportsCaseInsensitiveQueries(database);
                if (supportsCaseInsensitiveQueries  == false)
                {
                    message = "<p>&nbsp;</p><p>The database you're trying to use does not support case insensitive queries. <br />We currently do not support these types of databases.</p>" +
                              "<p>You can fix this by changing the following setting in your my.ini file in your MySQL installation directory:</p>" +
                              "<pre>lower_case_table_names=1</pre><br />" +
                              "<p>Note: Make sure to check with your hosting provider if they support case insensitive queries as well.</p>" +
                              "<p>For more technical information on case sensitivity in MySQL, have a look at " +
                              "<a href='http://dev.mysql.com/doc/refman/5.0/en/identifier-case-sensitivity.html'>the documentation on the subject</a></p>";

                    return new Result { Message = message, Success = false, Percentage = "15" };
                }
                */

                message = GetResultMessageForMySql();

                var schemaResult = ValidateDatabaseSchema();
                
                var installedSchemaVersion = schemaResult.DetermineInstalledVersion();
                
                //If Configuration Status is empty and the determined version is "empty" its a new install - otherwise upgrade the existing
                if (string.IsNullOrEmpty(GlobalSettings.ConfigurationStatus) && installedSchemaVersion.Equals(new Version(0, 0, 0)))
                {
                    var helper = new DatabaseSchemaHelper(database, _logger, SqlSyntax);
                    helper.CreateDatabaseSchema(true, applicationContext);

                    message = message + "<p>Installation completed!</p>";

                    //now that everything is done, we need to determine the version of SQL server that is executing
                    _logger.Info<DatabaseContext>("Database configuration status: " + message);
                    return new Result { Message = message, Success = true, Percentage = "100" };
                }

                //we need to do an upgrade so return a new status message and it will need to be done during the next step
                _logger.Info<DatabaseContext>("Database requires upgrade");
                message = "<p>Upgrading database, this may take some time...</p>";
                return new Result
                    {
                        RequiresUpgrade = true, 
                        Message = message, 
                        Success = true, 
                        Percentage = "30"
                    };
            }
            catch (Exception ex)
            {
                return HandleInstallException(ex);
            }
        }

        /// <summary>
        /// This assumes all of the previous checks are done!
        /// </summary>
        /// <returns></returns>
        internal Result UpgradeSchemaAndData(IMigrationEntryService migrationEntryService)
        {
            try
            {

                var readyForInstall = CheckReadyForInstall();
                if (readyForInstall.Success == false)
                {
                    return readyForInstall.Result;
                }

                _logger.Info<DatabaseContext>("Database upgrade started");

                var database = new UmbracoDatabase(_connectionString, ProviderName, _logger);
                //var supportsCaseInsensitiveQueries = SqlSyntax.SupportsCaseInsensitiveQueries(database);                

                var message = GetResultMessageForMySql();

                var schemaResult = ValidateDatabaseSchema();

                var installedSchemaVersion = new SemVersion(schemaResult.DetermineInstalledVersion());

                var installedMigrationVersion = schemaResult.DetermineInstalledVersionByMigrations(migrationEntryService);    
                
                var targetVersion = UmbracoVersion.Current;
                
                //In some cases - like upgrading from 7.2.6 -> 7.3, there will be no migration information in the database and therefore it will
                // return a version of 0.0.0 and we don't necessarily want to run all migrations from 0 -> 7.3, so we'll just ensure that the 
                // migrations are run for the target version
                if (installedMigrationVersion == new SemVersion(new Version(0, 0, 0)) && installedSchemaVersion > new SemVersion(new Version(0, 0, 0)))
                {
                    //set the installedMigrationVersion to be one less than the target so the latest migrations are guaranteed to execute
                    installedMigrationVersion = new SemVersion(targetVersion.SubtractRevision());
                }
                
                //Figure out what our current installed version is. If the web.config doesn't have a version listed, then we'll use the minimum
                // version detected between the schema installed and the migrations listed in the migration table. 
                // If there is a version in the web.config, we'll take the minimum between the listed migration in the db and what
                // is declared in the web.config.
                
                var currentInstalledVersion = string.IsNullOrEmpty(GlobalSettings.ConfigurationStatus)
                    //Take the minimum version between the detected schema version and the installed migration version
                    ? new[] {installedSchemaVersion, installedMigrationVersion}.Min()
                    //Take the minimum version between the installed migration version and the version specified in the config
                    : new[] { SemVersion.Parse(GlobalSettings.ConfigurationStatus), installedMigrationVersion }.Min();

                //Ok, another edge case here. If the current version is a pre-release, 
                // then we want to ensure all migrations for the current release are executed. 
                if (currentInstalledVersion.Prerelease.IsNullOrWhiteSpace() == false)
                {
                    currentInstalledVersion  = new SemVersion(currentInstalledVersion.GetVersion().SubtractRevision());
                }

                //DO the upgrade!

                var runner = new MigrationRunner(migrationEntryService, _logger, currentInstalledVersion, UmbracoVersion.GetSemanticVersion(), GlobalSettings.UmbracoMigrationName);

                var upgraded = runner.Execute(database, true);

                if (upgraded == false)
                {
                    throw new ApplicationException("Upgrading failed, either an error occurred during the upgrade process or an event canceled the upgrade process, see log for full details");
                }

                message = message + "<p>Upgrade completed!</p>";

                //now that everything is done, we need to determine the version of SQL server that is executing

                _logger.Info<DatabaseContext>("Database configuration status: " + message);

                return new Result { Message = message, Success = true, Percentage = "100" };
            }
            catch (Exception ex)
            {
                return HandleInstallException(ex);
            }
        }

        private string GetResultMessageForMySql()
        {
            if (SqlSyntax.GetType() == typeof(MySqlSyntaxProvider))
            {
                return "<p>&nbsp;</p><p>Congratulations, the database step ran successfully!</p>" +
                       "<p>Note: You're using MySQL and the database instance you're connecting to seems to support case insensitive queries.</p>" +
                       "<p>However, your hosting provider may not support this option. Umbraco does not currently support MySQL installs that do not support case insensitive queries</p>" +
                       "<p>Make sure to check with your hosting provider if they support case insensitive queries as well.</p>" +
                       "<p>They can check this by looking for the following setting in the my.ini file in their MySQL installation directory:</p>" +
                       "<pre>lower_case_table_names=1</pre><br />" +
                       "<p>For more technical information on case sensitivity in MySQL, have a look at " +
                       "<a href='http://dev.mysql.com/doc/refman/5.0/en/identifier-case-sensitivity.html'>the documentation on the subject</a></p>";
            }
            return string.Empty;
        }

        /*
        private string GetResultMessageForMySql(bool? supportsCaseInsensitiveQueries)
        {
            if (supportsCaseInsensitiveQueries == null)
            {
                return "<p>&nbsp;</p><p>Warning! Could not check if your database type supports case insensitive queries. <br />We currently do not support these databases that do not support case insensitive queries.</p>" +
                          "<p>You can check this by looking for the following setting in your my.ini file in your MySQL installation directory:</p>" +
                          "<pre>lower_case_table_names=1</pre><br />" +
                          "<p>Note: Make sure to check with your hosting provider if they support case insensitive queries as well.</p>" +
                          "<p>For more technical information on case sensitivity in MySQL, have a look at " +
                          "<a href='http://dev.mysql.com/doc/refman/5.0/en/identifier-case-sensitivity.html'>the documentation on the subject</a></p>";
            }
            if (SqlSyntax.GetType() == typeof(MySqlSyntaxProvider))
            {
                return "<p>&nbsp;</p><p>Congratulations, the database step ran successfully!</p>" +
                       "<p>Note: You're using MySQL and the database instance you're connecting to seems to support case insensitive queries.</p>" +
                       "<p>However, your hosting provider may not support this option. Umbraco does not currently support MySQL installs that do not support case insensitive queries</p>" +
                       "<p>Make sure to check with your hosting provider if they support case insensitive queries as well.</p>" +
                       "<p>They can check this by looking for the following setting in the my.ini file in their MySQL installation directory:</p>" +
                       "<pre>lower_case_table_names=1</pre><br />" +
                       "<p>For more technical information on case sensitivity in MySQL, have a look at " +
                       "<a href='http://dev.mysql.com/doc/refman/5.0/en/identifier-case-sensitivity.html'>the documentation on the subject</a></p>";
            }
            return string.Empty;
        }*/

        private Attempt<Result> CheckReadyForInstall()
        {
            if (SystemUtilities.GetCurrentTrustLevel() != AspNetHostingPermissionLevel.Unrestricted
                    && ProviderName == Constants.DatabaseProviders.MySql)
            {
                throw new InvalidOperationException("Cannot use MySql in Medium Trust configuration");
            }

            if (_configured == false || (string.IsNullOrEmpty(_connectionString) || string.IsNullOrEmpty(ProviderName)))
            {
                return Attempt.Fail(new Result
                {
                    Message =
                        "Database configuration is invalid. Please check that the entered database exists and that the provided username and password has write access to the database.",
                    Success = false,
                    Percentage = "10"
                });
            }
            return Attempt<Result>.Succeed();
        }

        private Result HandleInstallException(Exception ex)
        {
            _logger.Error<DatabaseContext>("Database configuration failed", ex);

            if (_result != null)
            {
                _logger.Info<DatabaseContext>("The database schema validation produced the following summary: \n" + _result.GetSummary());
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

        internal class Result
        {
            public bool RequiresUpgrade { get; set; }
            public string Message { get; set; }
            public bool Success { get; set; }
            public string Percentage { get; set; }
        }

        internal bool IsConnectionStringConfigured(ConnectionStringSettings databaseSettings)
        {
            var dbIsSqlCe = false;
            if (databaseSettings != null && databaseSettings.ProviderName != null)
                dbIsSqlCe = databaseSettings.ProviderName == Constants.DatabaseProviders.SqlCe;
            var sqlCeDatabaseExists = false;
            if (dbIsSqlCe)
            {
                var parts = databaseSettings.ConnectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var dataSourcePart = parts.FirstOrDefault(x => x.InvariantStartsWith("Data Source="));
                if (dataSourcePart != null)
                {
                    var datasource = dataSourcePart.Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString());
                    var filePath = datasource.Replace("Data Source=", string.Empty);
                    sqlCeDatabaseExists = File.Exists(filePath);                    
                }
            }

            // Either the connection details are not fully specified or it's a SQL CE database that doesn't exist yet
            if (databaseSettings == null
                || string.IsNullOrWhiteSpace(databaseSettings.ConnectionString) || string.IsNullOrWhiteSpace(databaseSettings.ProviderName)
                || (dbIsSqlCe && sqlCeDatabaseExists == false))
            {
                return false;
            }

            return true;
        }
    }
}