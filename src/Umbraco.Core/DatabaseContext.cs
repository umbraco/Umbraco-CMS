using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Xml.Linq;
using NPoco;
using Semver;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents the Umbraco database.
    /// </summary>
    /// <remarks>One per AppDomain. Ensures that the database is available.</remarks>
    public class DatabaseContext
    {
        private readonly IDatabaseFactory _factory;
        private readonly ILogger _logger;
        private DatabaseSchemaResult _databaseSchemaValidationResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContext"/> class with a database factory and a logger.
        /// </summary>
        /// <param name="factory">A database factory.</param>
        /// <param name="logger">A logger.</param>
        /// <remarks>The database factory will try to configure itself but may fail eg if the default
        /// Umbraco connection string is not available because we are installing. In which case this
        /// database context must sort things out and configure the database factory before it can be
        /// used.</remarks>
        public DatabaseContext(IDatabaseFactory factory, ILogger logger)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _factory = factory;
            _logger = logger;
        }

        /// <summary>
        /// Gets the QueryFactory
        /// </summary>
        public IQueryFactory QueryFactory => _factory.QueryFactory;

        /// <summary>
        /// Gets the database sql syntax.
        /// </summary>
        public ISqlSyntaxProvider SqlSyntax => _factory.QueryFactory.SqlSyntax;

        /// <summary>
        /// Gets the <see cref="Database"/> object for doing CRUD operations
        /// against custom tables that resides in the Umbraco database.
        /// </summary>
        /// <remarks>
        /// This should not be used for CRUD operations or queries against the
        /// standard Umbraco tables! Use the Public services for that.
        /// </remarks>
        public UmbracoDatabase Database => _factory.GetDatabase();

        /// <summary>
        /// Gets a value indicating whether the database is configured, ie whether it exists
        /// and can be reached. It does not necessarily mean that Umbraco is installed nor
        /// up-to-date.
        /// </summary>
        public bool IsDatabaseConfigured => _factory.Configured;

        /// <summary>
        /// Gets a value indicating whether it is possible to connect to the database.
        /// </summary>
        public bool CanConnect
        {
            get
            {
                if (IsDatabaseConfigured == false) return false;
                var canConnect = _factory.CanConnect;
                LogHelper.Info<DatabaseContext>("CanConnect = " + canConnect);
                return canConnect;
            }
        }

        #region Configure Connection String

        private const string EmbeddedDatabaseConnectionString = @"Data Source=|DataDirectory|\Umbraco.sdf;Flush Interval=1;";

        /// <summary>
        /// Configures a connection string for the embedded database.
        /// </summary>
        public void ConfigureEmbeddedDatabaseConnection()
        {
            ConfigureEmbeddedDatabaseConnection(_factory, _logger);
        }

        private static void ConfigureEmbeddedDatabaseConnection(IDatabaseFactory factory, ILogger logger)
        {
            SaveConnectionString(EmbeddedDatabaseConnectionString, Constants.DbProviderNames.SqlCe, logger);

            var path = Path.Combine(GlobalSettings.FullpathToRoot, "App_Data", "Umbraco.sdf");
            if (File.Exists(path) == false)
            {
                // this should probably be in a "using (new SqlCeEngine)" clause but not sure
                // of the side effects and it's been like this for quite some time now

                var engine = new SqlCeEngine(EmbeddedDatabaseConnectionString);
                engine.CreateDatabase();
            }

            factory.Configure(EmbeddedDatabaseConnectionString, Constants.DbProviderNames.SqlCe);
        }

        /// <summary>
        /// Configures a connection string that has been entered manually.
        /// </summary>
        /// <param name="connectionString">A connection string.</param>
        /// <remarks>Has to be either SQL Server or MySql</remarks>
        public void ConfigureDatabaseConnection(string connectionString)
        {
            var provider = DbConnectionExtensions.DetectProviderNameFromConnectionString(connectionString);
            var providerName = provider.ToString().ToLower().Contains("mysql")
                ? Constants.DbProviderNames.MySql
                : Constants.DbProviderNames.SqlServer;

            SaveConnectionString(connectionString, providerName, _logger);
            _factory.Configure(connectionString, providerName);
        }

        /// <summary>
        /// Configures a connection string from the installer.
        /// </summary>
        /// <param name="server">The name or address of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="user">The user name.</param>
        /// <param name="password">The user password.</param>
        /// <param name="databaseProvider">The name the provider (Sql, Sql Azure, Sql Ce, MySql).</param>
        public void ConfigureDatabaseConnection(string server, string databaseName, string user, string password, string databaseProvider)
        {
            string providerName;
            var connectionString = GetDatabaseConnectionString(server, databaseName, user, password, databaseProvider, out providerName);

            SaveConnectionString(connectionString, providerName, _logger);
            _factory.Configure(connectionString, providerName);
        }

        /// <summary>
        /// Gets a connection string from the installer.
        /// </summary>
        /// <param name="server">The name or address of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="user">The user name.</param>
        /// <param name="password">The user password.</param>
        /// <param name="databaseProvider">The name the provider (Sql, Sql Azure, Sql Ce, MySql).</param>
        /// <param name="providerName"></param>
        /// <returns>A connection string.</returns>
        public string GetDatabaseConnectionString(string server, string databaseName, string user, string password, string databaseProvider, out string providerName)
        {
            providerName = Constants.DbProviderNames.SqlServer;
            var test = databaseProvider.ToLower();
            if (test.Contains("mysql"))
            {
                providerName = Constants.DbProviderNames.MySql;
                return $"Server={server}; Database={databaseName};Uid={user};Pwd={password}";
            }
            if (test.Contains("azure"))
            {
                return GetAzureConnectionString(server, databaseName, user, password);
            }
            return $"server={server};database={databaseName};user id={user};password={password}";
        }

        /// <summary>
        /// Configures a connection string using Microsoft SQL Server integrated security.
        /// </summary>
        /// <param name="server">The name or address of the database server.</param>
        /// <param name="databaseName">The name of the database</param>
        public void ConfigureIntegratedSecurityDatabaseConnection(string server, string databaseName)
        {
            var connectionString = GetIntegratedSecurityDatabaseConnectionString(server, databaseName);
            SaveConnectionString(connectionString, Constants.DbProviderNames.SqlServer, _logger);
            _factory.Configure(connectionString, Constants.DbProviderNames.SqlServer);
        }

        /// <summary>
        /// Gets a connection string using Microsoft SQL Server integrated security.
        /// </summary>
        /// <param name="server">The name or address of the database server.</param>
        /// <param name="databaseName">The name of the database</param>
        /// <returns>A connection string.</returns>
        public string GetIntegratedSecurityDatabaseConnectionString(string server, string databaseName)
        {
            return $"Server={server};Database={databaseName};Integrated Security=true";
        }

        /// <summary>
        /// Gets an Azure connection string.
        /// </summary>
        /// <param name="server">The name or address of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="user">The user name.</param>
        /// <param name="password">The user password.</param>
        /// <returns>A connection string.</returns>
        public string GetAzureConnectionString(string server, string databaseName, string user, string password)
        {
            if (server.Contains(".") && ServerStartsWithTcp(server) == false)
                server = $"tcp:{server}";

            if (server.Contains(".") == false && ServerStartsWithTcp(server))
            {
                string serverName = server.Contains(",")
                                        ? server.Substring(0, server.IndexOf(",", StringComparison.Ordinal))
                                        : server;

                var portAddition = string.Empty;

                if (server.Contains(","))
                    portAddition = server.Substring(server.IndexOf(",", StringComparison.Ordinal));

                server = $"{serverName}.database.windows.net{portAddition}";
            }

            if (ServerStartsWithTcp(server) == false)
                server = $"tcp:{server}.database.windows.net";

            if (server.Contains(",") == false)
                server = $"{server},1433";

            if (user.Contains("@") == false)
            {
                var userDomain = server;

                if (ServerStartsWithTcp(server))
                    userDomain = userDomain.Substring(userDomain.IndexOf(":", StringComparison.Ordinal) + 1);

                if (userDomain.Contains("."))
                    userDomain = userDomain.Substring(0, userDomain.IndexOf(".", StringComparison.Ordinal));

                user = $"{user}@{userDomain}";
            }

            return $"Server={server};Database={databaseName};User ID={user};Password={password}";
        }

        private static bool ServerStartsWithTcp(string server)
        {
            return server.ToLower().StartsWith("tcp:".ToLower());
        }

        /// <summary>
        /// Saves the connection string as a proper .net connection string in web.config.
        /// </summary>
        /// <remarks>Saves the ConnectionString in the very nasty 'medium trust'-supportive way.</remarks>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="providerName">The provider name.</param>
        /// <param name="logger">A logger.</param>
        private static void SaveConnectionString(string connectionString, string providerName, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Value cannot be null nor empty.", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(providerName)) throw new ArgumentException("Value cannot be null nor empty.", nameof(providerName));

            // set the connection string for the new datalayer
            var connectionStringSettings = new ConnectionStringSettings(GlobalSettings.UmbracoConnectionName, connectionString, providerName);

            var fileName = IOHelper.MapPath($"{SystemDirectories.Root}/web.config");
            var xml = XDocument.Load(fileName, LoadOptions.PreserveWhitespace);
            if (xml.Root == null) throw new Exception("Invalid web.config file.");
            var connectionStrings = xml.Root.DescendantsAndSelf("connectionStrings").FirstOrDefault();
            if (connectionStrings == null) throw new Exception("Invalid web.config file.");

            // update connectionString if it exists, or else create a new connectionString
            var setting = connectionStrings.Descendants("add").FirstOrDefault(s => s.Attribute("name").Value == GlobalSettings.UmbracoConnectionName);
            if (setting == null)
            {
                connectionStrings.Add(new XElement("add",
                    new XAttribute("name", GlobalSettings.UmbracoConnectionName),
                    new XAttribute("connectionString", connectionStringSettings),
                    new XAttribute("providerName", providerName)));
            }
            else
            {
                setting.Attribute("connectionString").Value = connectionString;
                setting.Attribute("providerName").Value = providerName;
            }

            xml.Save(fileName, SaveOptions.DisableFormatting);
            logger.Info<DatabaseContext>("Configured a new ConnectionString using the '" + providerName + "' provider.");
        }

        #endregion

        #region Utils

        internal static void GiveLegacyAChance(IDatabaseFactory factory, ILogger logger)
        {
            // look for the legacy appSettings key
            var legacyConnString = ConfigurationManager.AppSettings[GlobalSettings.UmbracoConnectionName];
            if (string.IsNullOrWhiteSpace(legacyConnString)) return;

            var test = legacyConnString.ToLowerInvariant();
            if (test.Contains("sqlce4umbraco"))
            {
                // sql ce
                ConfigureEmbeddedDatabaseConnection(factory, logger);
            }
            else if (test.Contains("tcp:"))
            {
                // sql azure
                SaveConnectionString(legacyConnString, Constants.DbProviderNames.SqlServer, logger);
                factory.Configure(legacyConnString, Constants.DbProviderNames.SqlServer);
            }
            else if (test.Contains("datalayer=mysql"))
            {
                // mysql

                // strip the datalayer part off
                var connectionStringWithoutDatalayer = string.Empty;
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var variable in legacyConnString.Split(';').Where(x => x.ToLowerInvariant().StartsWith("datalayer") == false))
                    connectionStringWithoutDatalayer = $"{connectionStringWithoutDatalayer}{variable};";

                SaveConnectionString(connectionStringWithoutDatalayer, Constants.DbProviderNames.MySql, logger);
                factory.Configure(connectionStringWithoutDatalayer, Constants.DbProviderNames.MySql);
            }
            else
            {
                // sql server
                SaveConnectionString(legacyConnString, Constants.DbProviderNames.SqlServer, logger);
                factory.Configure(legacyConnString, Constants.DbProviderNames.SqlServer);
            }

            // remove the legacy connection string, so we don't end up in a loop if something goes wrong
            GlobalSettings.RemoveSetting(GlobalSettings.UmbracoConnectionName);
        }

        #endregion

        #region Database Schema

        internal DatabaseSchemaResult ValidateDatabaseSchema()
        {
            if (_factory.Configured == false)
                return new DatabaseSchemaResult(SqlSyntax);

            if (_databaseSchemaValidationResult != null)
                return _databaseSchemaValidationResult;

            var database = _factory.GetDatabase();
            var dbSchema = new DatabaseSchemaCreation(database, _logger);
            _databaseSchemaValidationResult = dbSchema.ValidateSchema();
            return _databaseSchemaValidationResult;
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

                var database = _factory.GetDatabase();

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

                var message = GetResultMessageForMySql();
                var schemaResult = ValidateDatabaseSchema();
                var installedSchemaVersion = schemaResult.DetermineInstalledVersion();

                //If Configuration Status is empty and the determined version is "empty" its a new install - otherwise upgrade the existing
                if (string.IsNullOrEmpty(GlobalSettings.ConfigurationStatus) && installedSchemaVersion.Equals(new Version(0, 0, 0)))
                {
                    var helper = new DatabaseSchemaHelper(database, _logger);
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
        internal Result UpgradeSchemaAndData(IMigrationEntryService migrationEntryService, IMigrationResolver migrationResolver)
        {
            try
            {
                var readyForInstall = CheckReadyForInstall();
                if (readyForInstall.Success == false)
                {
                    return readyForInstall.Result;
                }

                _logger.Info<DatabaseContext>("Database upgrade started");

                var database = _factory.GetDatabase();
                //var supportsCaseInsensitiveQueries = SqlSyntax.SupportsCaseInsensitiveQueries(database);

                var message = GetResultMessageForMySql();

                var schemaResult = ValidateDatabaseSchema();

                var installedSchemaVersion = new SemVersion(schemaResult.DetermineInstalledVersion());

                var installedMigrationVersion = new SemVersion(0);
                //we cannot check the migrations table if it doesn't exist, this will occur when upgrading to 7.3
                if (schemaResult.ValidTables.Any(x => x.InvariantEquals("umbracoMigration")))
                {
                    installedMigrationVersion = schemaResult.DetermineInstalledVersionByMigrations(migrationEntryService);
                }

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

                var runner = new MigrationRunner(migrationResolver, migrationEntryService, _logger, currentInstalledVersion, UmbracoVersion.GetSemanticVersion(), GlobalSettings.UmbracoMigrationName);

                var migrationContext = new MigrationContext(database, _logger);
                var upgraded = runner.Execute(migrationContext /*, true*/);

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
            if (_factory.Configured == false)
            {
                return Attempt.Fail(new Result
                {
                    Message = "Database configuration is invalid. Please check that the entered database exists and"
                        + " that the provided username and password has write access to the database.",
                    Success = false,
                    Percentage = "10"
                });
            }
            return Attempt<Result>.Succeed();
        }

        private Result HandleInstallException(Exception ex)
        {
            _logger.Error<DatabaseContext>("Database configuration failed", ex);

            if (_databaseSchemaValidationResult != null)
            {
                _logger.Info<DatabaseContext>("The database schema validation produced the following summary: \n" + _databaseSchemaValidationResult.GetSummary());
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

        #endregion

        internal bool IsConnectionStringConfigured(ConnectionStringSettings databaseSettings)
        {
            var dbIsSqlCe = false;
            if (databaseSettings?.ProviderName != null)
                dbIsSqlCe = databaseSettings.ProviderName == Constants.DbProviderNames.SqlCe;
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

        // fixme - kill!
        public Sql<SqlContext> Sql()
        {
            return Database.Sql();
        }
    }
}