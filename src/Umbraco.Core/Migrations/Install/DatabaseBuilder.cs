using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Umbraco.Core.Migrations.Install
{
    /// <summary>
    /// Supports building and configuring the database.
    /// </summary>
    public class DatabaseBuilder
    {
        private readonly IUmbracoDatabaseFactory _databaseFactory;
        private readonly IScopeProvider _scopeProvider;
        private readonly IGlobalSettings _globalSettings;
        private readonly IRuntimeState _runtime;
        private readonly IMigrationBuilder _migrationBuilder;
        private readonly IKeyValueService _keyValueService;
        private readonly PostMigrationCollection _postMigrations;
        private readonly ILogger _logger;

        private DatabaseSchemaResult _databaseSchemaValidationResult;

        public DatabaseBuilder(IScopeProvider scopeProvider, IGlobalSettings globalSettings, IUmbracoDatabaseFactory databaseFactory, IRuntimeState runtime, ILogger logger, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, PostMigrationCollection postMigrations)
        {
            _scopeProvider = scopeProvider;
            _globalSettings = globalSettings;
            _databaseFactory = databaseFactory;
            _runtime = runtime;
            _logger = logger;
            _migrationBuilder = migrationBuilder;
            _keyValueService = keyValueService;
            _postMigrations = postMigrations;
        }

        #region Status

        /// <summary>
        /// Gets a value indicating whether the database is configured. It does not necessarily
        /// mean that it is possible to connect, nor that Umbraco is installed, nor
        /// up-to-date.
        /// </summary>
        public bool IsDatabaseConfigured => _databaseFactory.Configured;

        /// <summary>
        /// Gets a value indicating whether it is possible to connect to the database.
        /// </summary>
        public bool CanConnect => _databaseFactory.CanConnect;

        // that method was originally created by Per in DatabaseHelper- tests the db connection for install
        // fixed by Shannon to not-ignore the provider
        // fixed by Stephan as part of the v8 persistence cleanup, now using provider names + SqlCe exception
        // moved by Stephan to DatabaseBuilder
        // probably needs to be cleaned up

        public bool CheckConnection(string databaseType, string connectionString, string server, string database, string login, string password, bool integratedAuth)
        {
            // we do not test SqlCE connection
            if (databaseType.InvariantContains("sqlce"))
                return true;

            string providerName;

            if (string.IsNullOrWhiteSpace(connectionString) == false)
            {
                providerName = DbConnectionExtensions.DetectProviderNameFromConnectionString(connectionString);
            }
            else if (integratedAuth)
            {
                // has to be Sql Server
                providerName = Constants.DbProviderNames.SqlServer;
                connectionString = GetIntegratedSecurityDatabaseConnectionString(server, database);
            }
            else
            {
                connectionString = GetDatabaseConnectionString(
                    server, database, login, password,
                    databaseType, out providerName);
            }

            return DbConnectionExtensions.IsConnectionAvailable(connectionString, providerName);
        }

        public bool HasSomeNonDefaultUser()
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                // look for the super user with default password
                var sql = scope.Database.SqlContext.Sql()
                    .SelectCount()
                    .From<UserDto>()
                    .Where<UserDto>(x => x.Id == Constants.Security.SuperUserId && x.Password == "default");
                var result = scope.Database.ExecuteScalar<int>(sql);
                var has = result != 1;
                if (has == false)
                {
                    // found only 1 user == the default user with default password
                    // however this always exists on uCloud, also need to check if there are other users too
                    result = scope.Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoUser");
                    has = result != 1;
                }
                scope.Complete();
                return has;
            }
        }

        #endregion

        #region Configure Connection String

        private const string EmbeddedDatabaseConnectionString = @"Data Source=|DataDirectory|\Umbraco.sdf;Flush Interval=1;";

        /// <summary>
        /// Configures a connection string for the embedded database.
        /// </summary>
        public void ConfigureEmbeddedDatabaseConnection()
        {
            ConfigureEmbeddedDatabaseConnection(_databaseFactory, _logger);
        }

        private static void ConfigureEmbeddedDatabaseConnection(IUmbracoDatabaseFactory factory, ILogger logger)
        {
            SaveConnectionString(EmbeddedDatabaseConnectionString, Constants.DbProviderNames.SqlCe, logger);

            var path = Path.Combine(IOHelper.GetRootDirectorySafe(), "App_Data", "Umbraco.sdf");
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
            _databaseFactory.Configure(connectionString, providerName);
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
            var connectionString = GetDatabaseConnectionString(server, databaseName, user, password, databaseProvider, out var providerName);

            SaveConnectionString(connectionString, providerName, _logger);
            _databaseFactory.Configure(connectionString, providerName);
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
        public static string GetDatabaseConnectionString(string server, string databaseName, string user, string password, string databaseProvider, out string providerName)
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
            _databaseFactory.Configure(connectionString, Constants.DbProviderNames.SqlServer);
        }

        /// <summary>
        /// Gets a connection string using Microsoft SQL Server integrated security.
        /// </summary>
        /// <param name="server">The name or address of the database server.</param>
        /// <param name="databaseName">The name of the database</param>
        /// <returns>A connection string.</returns>
        public static string GetIntegratedSecurityDatabaseConnectionString(string server, string databaseName)
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
        public static string GetAzureConnectionString(string server, string databaseName, string user, string password)
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
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullOrEmptyException(nameof(connectionString));
            if (string.IsNullOrWhiteSpace(providerName)) throw new ArgumentNullOrEmptyException(nameof(providerName));

            // set the connection string for the new datalayer
            var connectionStringSettings = new ConnectionStringSettings(Constants.System.UmbracoConnectionName, connectionString, providerName);

            var fileName = IOHelper.MapPath($"{SystemDirectories.Root}/web.config");
            var xml = XDocument.Load(fileName, LoadOptions.PreserveWhitespace);
            if (xml.Root == null) throw new Exception("Invalid web.config file.");
            var connectionStrings = xml.Root.DescendantsAndSelf("connectionStrings").FirstOrDefault();
            if (connectionStrings == null) throw new Exception("Invalid web.config file.");

            // honour configSource, if its set, change the xml file we are saving the configuration
            // to the one set in the configSource attribute
            if (connectionStrings.Attribute("configSource") != null)
            {
                var source = connectionStrings.Attribute("configSource").Value;
                var configFile = IOHelper.MapPath($"{SystemDirectories.Root}/{source}");
                logger.Info<DatabaseBuilder>("Storing ConnectionString in {ConfigFile}", configFile);
                if (File.Exists(configFile))
                {
                    xml = XDocument.Load(fileName, LoadOptions.PreserveWhitespace);
                    fileName = configFile;
                }
                connectionStrings = xml.Root.DescendantsAndSelf("connectionStrings").FirstOrDefault();
                if (connectionStrings == null) throw new Exception("Invalid web.config file.");
            }

            // update connectionString if it exists, or else create a new connectionString
            var setting = connectionStrings.Descendants("add").FirstOrDefault(s => s.Attribute("name").Value == Constants.System.UmbracoConnectionName);
            if (setting == null)
            {
                connectionStrings.Add(new XElement("add",
                    new XAttribute("name", Constants.System.UmbracoConnectionName),
                    new XAttribute("connectionString", connectionStringSettings),
                    new XAttribute("providerName", providerName)));
            }
            else
            {
                setting.Attribute("connectionString").Value = connectionString;
                setting.Attribute("providerName").Value = providerName;
            }

            xml.Save(fileName, SaveOptions.DisableFormatting);
            logger.Info<DatabaseBuilder>("Configured a new ConnectionString using the '{ProviderName}' provider.", providerName);
        }

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

        #endregion

        #region Utils

        internal static void GiveLegacyAChance(IUmbracoDatabaseFactory factory, ILogger logger)
        {
            // look for the legacy appSettings key
            var legacyConnString = ConfigurationManager.AppSettings[Constants.System.UmbracoConnectionName];
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
            GlobalSettings.RemoveSetting(Constants.System.UmbracoConnectionName);
        }

        #endregion

        #region Database Schema

        internal DatabaseSchemaResult ValidateDatabaseSchema()
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                var result = ValidateDatabaseSchema(scope);
                scope.Complete();
                return result;
            }
        }

        private DatabaseSchemaResult ValidateDatabaseSchema(IScope scope)
        {
            if (_databaseFactory.Configured == false)
                return new DatabaseSchemaResult(_databaseFactory.SqlContext.SqlSyntax);

            if (_databaseSchemaValidationResult != null)
                return _databaseSchemaValidationResult;

            var database = scope.Database;
            var dbSchema = new DatabaseSchemaCreator(database, _logger);
            _databaseSchemaValidationResult = dbSchema.ValidateSchema();
            scope.Complete();
            return _databaseSchemaValidationResult;
        }

        internal Result CreateDatabaseSchemaAndData()
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                var result = CreateDatabaseSchemaAndData(scope);
                scope.Complete();
                return result;
            }
        }

        private Result CreateDatabaseSchemaAndData(IScope scope)
        {
            try
            {
                var readyForInstall = CheckReadyForInstall();
                if (readyForInstall.Success == false)
                {
                    return readyForInstall.Result;
                }

                _logger.Info<DatabaseBuilder>("Database configuration status: Started");

                var database = scope.Database;

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
                if (string.IsNullOrEmpty(_globalSettings.ConfigurationStatus) && installedSchemaVersion.Equals(new Version(0, 0, 0)))
                {
                    if (_runtime.Level == RuntimeLevel.Run)
                        throw new Exception("Umbraco is already configured!");

                    var creator = new DatabaseSchemaCreator(database, _logger);
                    creator.InitializeDatabaseSchema();

                    message = message + "<p>Installation completed!</p>";

                    //now that everything is done, we need to determine the version of SQL server that is executing
                    _logger.Info<DatabaseBuilder>("Database configuration status: {DbConfigStatus}", message);
                    return new Result { Message = message, Success = true, Percentage = "100" };
                }

                //we need to do an upgrade so return a new status message and it will need to be done during the next step
                _logger.Info<DatabaseBuilder>("Database requires upgrade");
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

        // This assumes all of the previous checks are done!
        internal Result UpgradeSchemaAndData()
        {
            try
            {
                var readyForInstall = CheckReadyForInstall();
                if (readyForInstall.Success == false)
                {
                    return readyForInstall.Result;
                }

                _logger.Info<DatabaseBuilder>("Database upgrade started");

                //var database = scope.Database;
                //var supportsCaseInsensitiveQueries = SqlSyntax.SupportsCaseInsensitiveQueries(database);

                var message = GetResultMessageForMySql();

                // fixme - remove this code
                //var schemaResult = ValidateDatabaseSchema();
                //
                //var installedSchemaVersion = new SemVersion(schemaResult.DetermineInstalledVersion());
                //var installedMigrationVersion = schemaResult.DetermineInstalledVersionByMigrations(migrationEntryService);
                //var targetVersion = UmbracoVersion.Current;
                //
                ////In some cases - like upgrading from 7.2.6 -> 7.3, there will be no migration information in the database and therefore it will
                //// return a version of 0.0.0 and we don't necessarily want to run all migrations from 0 -> 7.3, so we'll just ensure that the
                //// migrations are run for the target version
                //if (installedMigrationVersion == new SemVersion(new Version(0, 0, 0)) && installedSchemaVersion > new SemVersion(new Version(0, 0, 0)))
                //{
                //    //set the installedMigrationVersion to be one less than the target so the latest migrations are guaranteed to execute
                //    installedMigrationVersion = new SemVersion(targetVersion.SubtractRevision());
                //}
                //
                ////Figure out what our current installed version is. If the web.config doesn't have a version listed, then we'll use the minimum
                //// version detected between the schema installed and the migrations listed in the migration table.
                //// If there is a version in the web.config, we'll take the minimum between the listed migration in the db and what
                //// is declared in the web.config.
                //
                //var currentInstalledVersion = string.IsNullOrEmpty(GlobalSettings.ConfigurationStatus)
                //    //Take the minimum version between the detected schema version and the installed migration version
                //    ? new[] { installedSchemaVersion, installedMigrationVersion }.Min()
                //    //Take the minimum version between the installed migration version and the version specified in the config
                //    : new[] { SemVersion.Parse(GlobalSettings.ConfigurationStatus), installedMigrationVersion }.Min();
                //
                ////Ok, another edge case here. If the current version is a pre-release,
                //// then we want to ensure all migrations for the current release are executed.
                //if (currentInstalledVersion.Prerelease.IsNullOrWhiteSpace() == false)
                //{
                //    currentInstalledVersion = new SemVersion(currentInstalledVersion.GetVersion().SubtractRevision());
                //}

                // upgrade
                var upgrader = new UmbracoUpgrader(_scopeProvider, _migrationBuilder, _keyValueService, _postMigrations, _logger);
                upgrader.Execute();

                // fixme remove this code
                //var runner = new MigrationRunner(_scopeProvider, builder, migrationEntryService, _logger, currentInstalledVersion, UmbracoVersion.SemanticVersion, Constants.System.UmbracoMigrationName);
                //var upgraded = runner.Execute(/*upgrade:true*/);
                //if (upgraded == false)
                //{
                //    throw new ApplicationException("Upgrading failed, either an error occurred during the upgrade process or an event canceled the upgrade process, see log for full details");
                //}

                message = message + "<p>Upgrade completed!</p>";

                //now that everything is done, we need to determine the version of SQL server that is executing

                _logger.Info<DatabaseBuilder>("Database configuration status: {DbConfigStatus}", message);

                return new Result { Message = message, Success = true, Percentage = "100" };
            }
            catch (Exception ex)
            {
                return HandleInstallException(ex);
            }
        }

        private string GetResultMessageForMySql()
        {
            if (_databaseFactory.GetType() == typeof(MySqlSyntaxProvider))
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
            if (_databaseFactory.Configured == false)
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
            _logger.Error<DatabaseBuilder>(ex, "Database configuration failed");

            if (_databaseSchemaValidationResult != null)
            {
                _logger.Info<DatabaseBuilder>("The database schema validation produced the following summary: {DbSchemaSummary}", _databaseSchemaValidationResult.GetSummary());
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
    }
}
