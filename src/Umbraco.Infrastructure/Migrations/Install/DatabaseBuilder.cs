using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Install
{
    /// <summary>
    /// Supports building and configuring the database.
    /// </summary>
    public class DatabaseBuilder
    {
        private readonly IUmbracoDatabaseFactory _databaseFactory;
        private readonly IScopeProvider _scopeProvider;
        private readonly IRuntimeState _runtime;
        private readonly IMigrationBuilder _migrationBuilder;
        private readonly IKeyValueService _keyValueService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger<DatabaseBuilder> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IDbProviderFactoryCreator _dbProviderFactoryCreator;
        private readonly IConfigManipulator _configManipulator;
        private readonly DatabaseSchemaCreatorFactory _databaseSchemaCreatorFactory;

        private DatabaseSchemaResult _databaseSchemaValidationResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseBuilder"/> class.
        /// </summary>
        public DatabaseBuilder(
            IScopeProvider scopeProvider,
            IUmbracoDatabaseFactory databaseFactory,
            IRuntimeState runtime,
            ILogger<DatabaseBuilder> logger,
            ILoggerFactory loggerFactory,
            IMigrationBuilder migrationBuilder,
            IKeyValueService keyValueService,
            IHostingEnvironment hostingEnvironment,
            IDbProviderFactoryCreator dbProviderFactoryCreator,
            IConfigManipulator configManipulator,
            DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory)
        {
            _scopeProvider = scopeProvider;
            _databaseFactory = databaseFactory;
            _runtime = runtime;
            _logger = logger;
            _loggerFactory = loggerFactory;
            _migrationBuilder = migrationBuilder;
            _keyValueService = keyValueService;
            _hostingEnvironment = hostingEnvironment;
            _dbProviderFactoryCreator = dbProviderFactoryCreator;
            _configManipulator = configManipulator;
            _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory;
        }

        #region Status

        /// <summary>
        /// Gets a value indicating whether the database is configured. It does not necessarily
        /// mean that it is possible to connect, nor that Umbraco is installed, nor up-to-date.
        /// </summary>
        public bool IsDatabaseConfigured => _databaseFactory.Configured;

        /// <summary>
        /// Gets a value indicating whether it is possible to connect to the configured database.
        /// It does not necessarily mean that Umbraco is installed, nor up-to-date.
        /// </summary>
        public bool CanConnectToDatabase => _databaseFactory.CanConnect;

        /// <summary>
        /// Verifies whether a it is possible to connect to a database.
        /// </summary>
        public bool CanConnect(string databaseType, string connectionString, string server, string database, string login, string password, bool integratedAuth)
        {
            // we do not test SqlCE or SQLite connection
            if (databaseType.InvariantContains("sqlce") || databaseType.InvariantContains("sqlite"))
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

            var factory = _dbProviderFactoryCreator.CreateFactory(providerName);
            return DbConnectionExtensions.IsConnectionAvailable(connectionString, factory);
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

        internal bool IsUmbracoInstalled()
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                return scope.Database.IsUmbracoInstalled();
            }
        }

        #endregion

        #region Configure Connection String

        public const string EmbeddedDatabaseConnectionString = @"Data Source=|DataDirectory|\Umbraco.sdf;Flush Interval=1;";
        public const string EmbeddedSQLiteDatabaseConnectionString = @"Data Source=./Umbraco.db;";

        /// <summary>
        /// Configures a connection string for the embedded database.
        /// </summary>
        public void ConfigureEmbeddedDatabaseConnection()
        {
            ConfigureEmbeddedDatabaseConnection(_databaseFactory);
        }

        public void ConfigureEmbeddedSQLiteDatabaseConnection()
        {
            ConfigureEmbeddedSQLiteDatabaseConnection(_databaseFactory);
        }

        private void ConfigureEmbeddedDatabaseConnection(IUmbracoDatabaseFactory factory)
        {
            _configManipulator.SaveConnectionString(EmbeddedDatabaseConnectionString, Constants.DbProviderNames.SqlCe);

            var path = _hostingEnvironment.MapPathContentRoot(Path.Combine(Constants.SystemDirectories.Data, "Umbraco.sdf"));
            if (File.Exists(path) == false)
            {
                // this should probably be in a "using (new SqlCeEngine)" clause but not sure
                // of the side effects and it's been like this for quite some time now

                _dbProviderFactoryCreator.CreateDatabase(Constants.DbProviderNames.SqlCe);
            }

            factory.Configure(EmbeddedDatabaseConnectionString, Constants.DbProviderNames.SqlCe);
        }

        private void ConfigureEmbeddedSQLiteDatabaseConnection(IUmbracoDatabaseFactory factory)
        {
            _configManipulator.SaveConnectionString(EmbeddedSQLiteDatabaseConnectionString, Constants.DbProviderNames.SQLite);

            var path = _hostingEnvironment.MapPathContentRoot("Umbraco.db");
            if (File.Exists(path) == false)
            {
                _dbProviderFactoryCreator.CreateDatabase(Constants.DbProviderNames.SQLite);
            }

            factory.Configure(EmbeddedSQLiteDatabaseConnectionString, Constants.DbProviderNames.SQLite);
        }

        /// <summary>
        /// Configures a connection string that has been entered manually.
        /// </summary>
        /// <param name="connectionString">A connection string.</param>
        /// <remarks>Has to be SQL Server</remarks>
        public void ConfigureDatabaseConnection(string connectionString)
        {
            const string providerName = Constants.DbProviderNames.SqlServer;

            _configManipulator.SaveConnectionString(connectionString, providerName);
            _databaseFactory.Configure(connectionString, providerName);
        }

        /// <summary>
        /// Configures a connection string from the installer.
        /// </summary>
        /// <param name="server">The name or address of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="user">The user name.</param>
        /// <param name="password">The user password.</param>
        /// <param name="databaseProvider">The name of the provider (Sql, Sql Azure, Sql Ce).</param>
        public void ConfigureDatabaseConnection(string server, string databaseName, string user, string password, string databaseProvider)
        {
            var connectionString = GetDatabaseConnectionString(server, databaseName, user, password, databaseProvider, out var providerName);

            _configManipulator.SaveConnectionString(connectionString, providerName);
            _databaseFactory.Configure(connectionString, providerName);
        }

        /// <summary>
        /// Gets a connection string from the installer.
        /// </summary>
        /// <param name="server">The name or address of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="user">The user name.</param>
        /// <param name="password">The user password.</param>
        /// <param name="databaseProvider">The name of the provider (Sql, Sql Azure, Sql Ce).</param>
        /// <param name="providerName"></param>
        /// <returns>A connection string.</returns>
        public static string GetDatabaseConnectionString(string server, string databaseName, string user, string password, string databaseProvider, out string providerName)
        {
            providerName = Constants.DbProviderNames.SqlServer;
            var provider = databaseProvider.ToLower();
            if (provider.InvariantContains("azure"))
                return GetAzureConnectionString(server, databaseName, user, password);
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
            _configManipulator.SaveConnectionString(connectionString, Constants.DbProviderNames.SqlServer);
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




        #endregion

        #region Database Schema

        /// <summary>
        /// Validates the database schema.
        /// </summary>
        /// <remarks>
        /// <para>This assumes that the database exists and the connection string is
        /// configured and it is possible to connect to the database.</para>
        /// </remarks>
        public DatabaseSchemaResult ValidateSchema()
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                var result = ValidateSchema(scope);
                scope.Complete();
                return result;
            }
        }

        private DatabaseSchemaResult ValidateSchema(IScope scope)
        {
            if (_databaseFactory.Initialized == false)
                return new DatabaseSchemaResult();

            if (_databaseSchemaValidationResult != null)
                return _databaseSchemaValidationResult;

            _databaseSchemaValidationResult = scope.Database.ValidateSchema();

            scope.Complete();

            return _databaseSchemaValidationResult;
        }

        /// <summary>
        /// Creates the database schema and inserts initial data.
        /// </summary>
        /// <remarks>
        /// <para>This assumes that the database exists and the connection string is
        /// configured and it is possible to connect to the database.</para>
        /// </remarks>
        public Result CreateSchemaAndData()
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                var result = CreateSchemaAndData(scope);
                scope.Complete();
                return result;
            }
        }

        private Result CreateSchemaAndData(IScope scope)
        {
            try
            {
                var readyForInstall = CheckReadyForInstall();
                if (readyForInstall.Success == false)
                {
                    return readyForInstall.Result;
                }

                _logger.LogInformation("Database configuration status: Started");

                var database = scope.Database;

                var message = string.Empty;

                var schemaResult = ValidateSchema();
                var hasInstalledVersion = schemaResult.DetermineHasInstalledVersion();

                //If the determined version is "empty" its a new install - otherwise upgrade the existing
                if (!hasInstalledVersion)
                {
                    if (_runtime.Level == RuntimeLevel.Run)
                        throw new Exception("Umbraco is already configured!");

                    var creator = _databaseSchemaCreatorFactory.Create(database);
                    creator.InitializeDatabaseSchema();

                    message = message + "<p>Installation completed!</p>";

                    //now that everything is done, we need to determine the version of SQL server that is executing
                    _logger.LogInformation("Database configuration status: {DbConfigStatus}", message);
                    return new Result { Message = message, Success = true, Percentage = "100" };
                }

                //we need to do an upgrade so return a new status message and it will need to be done during the next step
                _logger.LogInformation("Database requires upgrade");
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
        /// Upgrades the database schema and data by running migrations.
        /// </summary>
        /// <remarks>
        /// <para>This assumes that the database exists and the connection string is
        /// configured and it is possible to connect to the database.</para>
        /// <para>Runs whichever migrations need to run.</para>
        /// </remarks>
        public Result UpgradeSchemaAndData(MigrationPlan plan)
        {
            try
            {
                var readyForInstall = CheckReadyForInstall();
                if (readyForInstall.Success == false)
                {
                    return readyForInstall.Result;
                }

                _logger.LogInformation("Database upgrade started");

                // upgrade
                var upgrader = new Upgrader(plan);
                upgrader.Execute(_scopeProvider, _migrationBuilder, _keyValueService, _loggerFactory.CreateLogger<Upgrader>(), _loggerFactory);

                var message = "<p>Upgrade completed!</p>";

                //now that everything is done, we need to determine the version of SQL server that is executing

                _logger.LogInformation("Database configuration status: {DbConfigStatus}", message);

                return new Result { Message = message, Success = true, Percentage = "100" };
            }
            catch (Exception ex)
            {
                return HandleInstallException(ex);
            }
        }

        private Attempt<Result> CheckReadyForInstall()
        {
            if (_databaseFactory.CanConnect == false)
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
            _logger.LogError(ex, "Database configuration failed");

            if (_databaseSchemaValidationResult != null)
            {
                _logger.LogInformation("The database schema validation produced the following summary: {DbSchemaSummary}", _databaseSchemaValidationResult.GetSummary());
            }

            return new Result
            {
                Message =
                    "The database configuration failed with the following message: " + ex.Message +
                    $"\n Please check log file for additional information (can be found in '{Constants.SystemDirectories.LogFiles}')",
                Success = false,
                Percentage = "90"
            };
        }

        /// <summary>
        /// Represents the result of a database creation or upgrade.
        /// </summary>
        public class Result
        {
            /// <summary>
            /// Gets or sets a value indicating whether an upgrade is required.
            /// </summary>
            public bool RequiresUpgrade { get; set; }

            /// <summary>
            /// Gets or sets the message returned by the operation.
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the operation succeeded.
            /// </summary>
            public bool Success { get; set; }

            /// <summary>
            /// Gets or sets an install progress pseudo-percentage.
            /// </summary>
            public string Percentage { get; set; }
        }

        #endregion
    }
}
