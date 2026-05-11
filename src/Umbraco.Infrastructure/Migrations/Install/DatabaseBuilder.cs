using System.Data.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Infrastructure.Migrations.Install
{
    /// <summary>
    /// Supports building and configuring the database.
    /// </summary>
    public class DatabaseBuilder
    {
        private readonly IUmbracoDatabaseFactory _databaseFactory;
        private readonly ICoreScopeProvider _scopeProvider;
        private readonly IScopeAccessor _scopeAccessor;
        private readonly IRuntimeState _runtimeState;
        private readonly IKeyValueService _keyValueService;
        private readonly ILogger<DatabaseBuilder> _logger;
        private readonly IDbProviderFactoryCreator _dbProviderFactoryCreator;
        private readonly IConfigManipulator _configManipulator;
        private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
        private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;
        private readonly IMigrationPlanExecutor _migrationPlanExecutor;
        private readonly DatabaseSchemaCreatorFactory _databaseSchemaCreatorFactory;
        private readonly IEnumerable<IDatabaseProviderMetadata> _databaseProviderMetadata;
        private readonly IEventAggregator _aggregator;

        private DatabaseSchemaResult? _databaseSchemaValidationResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseBuilder"/> class.
        /// </summary>
        /// <param name="scopeProvider">Provides core scoping functionality for database operations.</param>
        /// <param name="scopeAccessor">Accesses the current scope context.</param>
        /// <param name="databaseFactory">Factory for creating Umbraco database connections.</param>
        /// <param name="runtimeState">Provides information about the current runtime state of the application.</param>
        /// <param name="loggerFactory">Factory for creating logger instances.</param>
        /// <param name="keyValueService">Service for accessing key-value storage.</param>
        /// <param name="dbProviderFactoryCreator">Creates database provider factories for different database types.</param>
        /// <param name="configManipulator">Handles manipulation of configuration files.</param>
        /// <param name="globalSettings">Monitors and provides global settings options.</param>
        /// <param name="connectionStrings">Monitors and provides connection string options.</param>
        /// <param name="migrationPlanExecutor">Executes migration plans for database schema changes.</param>
        /// <param name="databaseSchemaCreatorFactory">Factory for creating database schema creators.</param>
        /// <param name="databaseProviderMetadata">A collection of metadata describing available database providers.</param>
        /// <param name="aggregator">Aggregates and dispatches events within the system.</param>
        public DatabaseBuilder(
            ICoreScopeProvider scopeProvider,
            IScopeAccessor scopeAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            IRuntimeState runtimeState,
            ILoggerFactory loggerFactory,
            IKeyValueService keyValueService,
            IDbProviderFactoryCreator dbProviderFactoryCreator,
            IConfigManipulator configManipulator,
            IOptionsMonitor<GlobalSettings> globalSettings,
            IOptionsMonitor<ConnectionStrings> connectionStrings,
            IMigrationPlanExecutor migrationPlanExecutor,
            DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory,
            IEnumerable<IDatabaseProviderMetadata> databaseProviderMetadata,
            IEventAggregator aggregator)
        {
            _scopeProvider = scopeProvider;
            _scopeAccessor = scopeAccessor;
            _databaseFactory = databaseFactory;
            _runtimeState = runtimeState;
            _logger = loggerFactory.CreateLogger<DatabaseBuilder>();
            _keyValueService = keyValueService;
            _dbProviderFactoryCreator = dbProviderFactoryCreator;
            _configManipulator = configManipulator;
            _globalSettings = globalSettings;
            _connectionStrings = connectionStrings;
            _migrationPlanExecutor = migrationPlanExecutor;
            _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory;
            _databaseProviderMetadata = databaseProviderMetadata;
            _aggregator = aggregator;
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
        /// Determines whether it is possible to connect to a database using the specified connection string and provider.
        /// </summary>
        /// <param name="connectionString">The connection string to the database.</param>
        /// <param name="providerName">The name of the database provider.</param>
        /// <returns>True if a connection can be established; otherwise, false.</returns>
        public bool CanConnect(string? connectionString, string providerName)
        {
            DbProviderFactory? factory = _dbProviderFactoryCreator.CreateFactory(providerName);
            return DbConnectionExtensions.IsConnectionAvailable(connectionString, factory);
        }

        /// <summary>
        /// Determines whether there is at least one user in the database other than the default super user with the default password.
        /// </summary>
        /// <returns>
        /// <c>true</c> if there is at least one user who is not the default super user with the default password; otherwise, <c>false</c>.
        /// </returns>
        public bool HasSomeNonDefaultUser()
        {
            using (ICoreScope scope = _scopeProvider.CreateCoreScope())
            {
                IScope ambientScope = _scopeAccessor.AmbientScope ?? throw new InvalidOperationException("Cannot execute without a valid AmbientScope.");
                // look for the super user with default password
                NPoco.Sql<ISqlContext> sql = ambientScope.Database.SqlContext.Sql()
                    .SelectCount()
                    .From<UserDto>()
                    .Where<UserDto>(x => x.Id == Constants.Security.SuperUserId && x.Password == "default");
                var result = _scopeAccessor.AmbientScope.Database.ExecuteScalar<int>(sql);
                var has = result != 1;
                if (has == false)
                {
                    // found only 1 user == the default user with default password
                    // however this always exists on uCloud, also need to check if there are other users too
                    sql = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
                        .SelectCount()
                        .From<UserDto>();
                    result = _scopeAccessor.AmbientScope.Database.ExecuteScalar<int>(sql);
                    has = result != 1;
                }
                scope.Complete();
                return has;
            }
        }

        internal bool IsUmbracoInstalled()
        {
            using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
            {
                return _scopeAccessor.AmbientScope?.Database.IsUmbracoInstalled() ?? false;
            }
        }

        #endregion

        #region Configure Connection String

        /// <summary>
        /// Configures the database connection using the provided database settings.
        /// If <paramref name="databaseSettings"/> is null, attempts a quick install with default settings.
        /// Attempts to establish a connection and, if <paramref name="isTrialRun"/> is false, saves the connection string configuration.
        /// Throws an <see cref="InstallException"/> if the database provider configuration cannot be determined or if the configuration update fails.
        /// </summary>
        /// <param name="databaseSettings">The database settings model containing connection details, or <c>null</c> to use default settings.</param>
        /// <param name="isTrialRun">If <c>true</c>, performs a trial run without saving the connection string; otherwise, saves the configuration.</param>
        /// <returns><c>true</c> if the connection was successfully configured and tested; otherwise, <c>false</c>.</returns>
        public bool ConfigureDatabaseConnection(DatabaseModel databaseSettings, bool isTrialRun)
        {
            IDatabaseProviderMetadata? providerMeta;

            // if the database model is null then we will attempt quick install.
            if (databaseSettings == null)
            {
                providerMeta = _databaseProviderMetadata.GetAvailable(true).FirstOrDefault();
                databaseSettings = new DatabaseModel
                {
                    DatabaseName = providerMeta?.DefaultDatabaseName!
                };
            }
            else
            {
                providerMeta = _databaseProviderMetadata.FirstOrDefault(x => x.Id == databaseSettings.DatabaseProviderMetadataId);
            }

            if (providerMeta == null)
            {
                throw new InstallException("Unable to determine database provider configuration.");
            }

            var connectionString = providerMeta.GenerateConnectionString(databaseSettings);
            var providerName = databaseSettings.ProviderName ?? providerMeta.ProviderName;

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(providerName) ||
                (providerMeta.RequiresConnectionTest && !CanConnect(connectionString, providerName)))
            {
                return false;
            }

            if (!isTrialRun)
            {
                // File configuration providers use a delay before reloading and triggering changes, so wait
                using var isChanged = new ManualResetEvent(false);
                using IDisposable? onChange = _connectionStrings.OnChange((options, name) =>
                {
                    // Only watch default named option (CurrentValue)
                    if (name != Options.DefaultName)
                    {
                        return;
                    }

                    // Signal change
                    isChanged.Set();
                });

                // Update configuration and wait for change
                _configManipulator.SaveConnectionStringAsync(connectionString, providerName).GetAwaiter().GetResult();
                if (!isChanged.WaitOne(10_000))
                {
                    throw new InstallException("Didn't retrieve updated connection string within 10 seconds, try manual configuration instead.");
                }

                Configure(providerMeta.ForceCreateDatabase);
            }

            return true;
        }

        /// <summary>
        /// Asynchronously validates the ability to connect to a database using the specified settings.
        /// </summary>
        /// <param name="databaseSettings">The database settings to use for validation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{InstallOperationStatus}"/> indicating the outcome of the validation, such as success, unknown provider, missing connection string, missing provider name, or connection failure.
        /// </returns>
        public Task<Attempt<InstallOperationStatus>> ValidateDatabaseConnectionAsync(DatabaseModel databaseSettings)
        {
            IDatabaseProviderMetadata? providerMeta = _databaseProviderMetadata.FirstOrDefault(x => x.Id == databaseSettings.DatabaseProviderMetadataId);

            if (providerMeta is null)
            {
                return Task.FromResult(Attempt.Fail(InstallOperationStatus.UnknownDatabaseProvider));
            }

            var connectionString = providerMeta.GenerateConnectionString(databaseSettings);
            var providerName = databaseSettings.ProviderName ?? providerMeta.ProviderName;

            if (string.IsNullOrEmpty(connectionString))
            {
                return Task.FromResult(Attempt.Fail(InstallOperationStatus.MissingConnectionString));
            }

            if (string.IsNullOrEmpty(providerName))
            {
                return Task.FromResult(Attempt.Fail(InstallOperationStatus.MissingProviderName));
            }

            if (providerMeta.RequiresConnectionTest && CanConnect(connectionString, providerName) is false)
            {
                return Task.FromResult(Attempt.Fail(InstallOperationStatus.DatabaseConnectionFailed));
            }

            return Task.FromResult(Attempt.Succeed(InstallOperationStatus.Success));
        }

        private void Configure(bool installMissingDatabase)
        {
            _databaseFactory.Configure(_connectionStrings.CurrentValue);

            if (installMissingDatabase)
            {
                CreateDatabase();
            }
        }

        #endregion

        #region Database Schema

        /// <summary>
        /// Creates a new database using the configured database provider and connection string.
        /// This method delegates the creation to the underlying database provider factory.
        /// </summary>
        public void CreateDatabase() => _dbProviderFactoryCreator.CreateDatabase(_databaseFactory.ProviderName!, _databaseFactory.ConnectionString!);

        /// <summary>
        /// Validates the database schema.
        /// </summary>
        /// <remarks>
        /// <para>This assumes that the database exists, the connection string is configured, and a connection to the database can be established.</para>
        /// </remarks>
        /// <returns>
        /// A <see cref="DatabaseSchemaResult"/> containing the result of the schema validation, or <c>null</c> if validation could not be performed.
        /// </returns>
        public DatabaseSchemaResult? ValidateSchema()
        {
            using (ICoreScope scope = _scopeProvider.CreateCoreScope())
            {
                DatabaseSchemaResult? result = ValidateSchema(scope);
                scope.Complete();
                return result;
            }
        }

        private DatabaseSchemaResult? ValidateSchema(ICoreScope scope)
        {
            if (_databaseFactory.Initialized == false)
            {
                return new DatabaseSchemaResult();
            }

            if (_databaseSchemaValidationResult != null)
            {
                return _databaseSchemaValidationResult;
            }

            _databaseSchemaValidationResult = _scopeAccessor.AmbientScope?.Database.ValidateSchema();

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
        /// <returns>
        /// A <see cref="Result"/> object indicating whether the operation succeeded, failed, or requires an upgrade.
        /// Returns <c>null</c> if the operation could not be completed.
        /// </returns>
        public Result? CreateSchemaAndData()
        {
            using (ICoreScope scope = _scopeProvider.CreateCoreScope())
            {
                Result? result = CreateSchemaAndData(scope);
                if (result?.Success is true)
                {
                    scope.Notifications.Publish(new DatabaseSchemaAndDataCreatedNotification(result!.RequiresUpgrade));
                }
                scope.Complete();
                return result;
            }
        }

        private Result? CreateSchemaAndData(ICoreScope scope)
        {
            try
            {
                Attempt<Result?> readyForInstall = CheckReadyForInstall();
                if (readyForInstall.Success == false)
                {
                    return readyForInstall.Result;
                }

                _logger.LogInformation("Database configuration status: Started");

                IUmbracoDatabase? database = _scopeAccessor.AmbientScope?.Database;

                var message = string.Empty;

                DatabaseSchemaResult? schemaResult = ValidateSchema();
                var hasInstalledVersion = schemaResult?.DetermineHasInstalledVersion() ?? false;

                //If the determined version is "empty" its a new install - otherwise upgrade the existing
                if (!hasInstalledVersion)
                {
                    if (_runtimeState.Level == RuntimeLevel.Run)
                    {
                        throw new Exception("Umbraco is already configured!");
                    }

                    DatabaseSchemaCreator creator = _databaseSchemaCreatorFactory.Create(database);
                    creator.InitializeDatabaseSchema();

                    message += "<p>Installation completed!</p>";

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
        /// Upgrades the database schema and data using the specified Umbraco plan.
        /// </summary>
        /// <param name="plan">The Umbraco plan describing the upgrade steps.</param>
        /// <returns>A <see cref="Result"/> indicating the outcome of the upgrade, or <c>null</c> if no result is available.</returns>
        /// <remarks>
        /// This method is obsolete. Use <see cref="UpgradeSchemaAndDataAsync"/> instead.
        /// </remarks>
        [Obsolete("Use UpgradeSchemaAndDataAsync instead. Scheduled for removal in Umbraco 18.")]
        public Result? UpgradeSchemaAndData(UmbracoPlan plan) => UpgradeSchemaAndData((MigrationPlan)plan);

        /// <summary>
        /// Upgrades the database schema and data according to the specified migration plan.
        /// </summary>
        /// <param name="plan">The migration plan to apply for upgrading the schema and data.</param>
        /// <returns>A <see cref="Result"/> indicating the outcome of the upgrade operation, or <c>null</c> if the upgrade did not produce a result.</returns>
        /// <remarks>
        /// This method is obsolete. Use <see cref="UpgradeSchemaAndDataAsync"/> instead. Scheduled for removal in Umbraco 18.
        /// </remarks>
        [Obsolete("Use UpgradeSchemaAndDataAsync instead. Scheduled for removal in Umbraco 18.")]
        public Result? UpgradeSchemaAndData(MigrationPlan plan) => UpgradeSchemaAndDataAsync(plan).GetAwaiter().GetResult();

        /// <summary>
        /// Asynchronously upgrades the database schema and data based on the specified <see cref="UmbracoPlan"/>.
        /// </summary>
        /// <param name="plan">The <see cref="UmbracoPlan"/> that defines the migration steps to apply.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="Result"/> of the upgrade if successful; otherwise, <c>null</c> if no upgrade was performed.</returns>
        public async Task<Result?> UpgradeSchemaAndDataAsync(UmbracoPlan plan) => await UpgradeSchemaAndDataAsync((MigrationPlan)plan).ConfigureAwait(false);

        /// <summary>
        /// Upgrades the database schema and data by running migrations.
        /// </summary>
        /// <remarks>
        /// <para>This assumes that the database exists and the connection string is
        /// configured and it is possible to connect to the database.</para>
        /// <para>Runs whichever migrations need to run.</para>
        /// </remarks>
        public async Task<Result?> UpgradeSchemaAndDataAsync(MigrationPlan plan)
        {
            try
            {
                Attempt<Result?> readyForInstall = CheckReadyForInstall();
                if (readyForInstall.Success == false)
                {
                    return readyForInstall.Result;
                }

                _logger.LogInformation("Database upgrade started");

                // upgrade
                var upgrader = new Upgrader(plan);
                ExecutedMigrationPlan result = await upgrader.ExecuteAsync(_migrationPlanExecutor, _scopeProvider, _keyValueService).ConfigureAwait(false);

                _aggregator.Publish(new UmbracoPlanExecutedNotification { ExecutedPlan = result });

                // The migration may have failed, it this is the case, we throw the exception now that we've taken care of business.
                if (result.Successful is false && result.Exception is not null)
                {
                    return HandleInstallException(result.Exception);
                }

                var message = "<p>Upgrade completed!</p>";
                _logger.LogInformation("Database configuration status: {DbConfigStatus}", message);

                return new Result { Message = message, Success = true, Percentage = "100" };
            }
            catch (Exception ex)
            {
                return HandleInstallException(ex);
            }
        }

        private Attempt<Result?> CheckReadyForInstall()
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
            return Attempt<Result?>.Succeed();
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
                    $"\n Please check log file for additional information (can be found in '{nameof(LoggingSettings)}.{nameof(LoggingSettings.Directory)}')",
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
            public string? Message { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the operation succeeded.
            /// </summary>
            public bool Success { get; set; }

            /// <summary>
            /// Gets or sets an install progress pseudo-percentage.
            /// </summary>
            public string? Percentage { get; set; }
        }

        #endregion
    }
}
