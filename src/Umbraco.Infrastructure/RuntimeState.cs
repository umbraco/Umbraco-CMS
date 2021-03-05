using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Core
{
    /// <summary>
    /// Represents the state of the Umbraco runtime.
    /// </summary>
    public class RuntimeState : IRuntimeState
    {
        private readonly GlobalSettings _globalSettings;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IUmbracoDatabaseFactory _databaseFactory;
        private readonly ILogger<RuntimeState> _logger;
        private readonly DatabaseSchemaCreatorFactory _databaseSchemaCreatorFactory;

        /// <summary>
        /// The initial <see cref="RuntimeState"/>
        /// The initial <see cref="RuntimeState"/>
        /// </summary>
        public static RuntimeState Booting() => new RuntimeState() { Level = RuntimeLevel.Boot };

        private RuntimeState()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeState"/> class.
        /// </summary>
        public RuntimeState(
            IOptions<GlobalSettings> globalSettings,
            IUmbracoVersion umbracoVersion,
            IUmbracoDatabaseFactory databaseFactory,
            ILogger<RuntimeState> logger,
            DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory)
        {
            _globalSettings = globalSettings.Value;
            _umbracoVersion = umbracoVersion;
            _databaseFactory = databaseFactory;
            _logger = logger;
            _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory;
        }


        /// <inheritdoc />
        public Version Version => _umbracoVersion.Version;

        /// <inheritdoc />
        public string VersionComment => _umbracoVersion.Comment;

        /// <inheritdoc />
        public SemVersion SemanticVersion => _umbracoVersion.SemanticVersion;

        /// <inheritdoc />
        public string CurrentMigrationState { get; private set; }

        /// <inheritdoc />
        public string FinalMigrationState { get; private set; }

        /// <inheritdoc />
        public RuntimeLevel Level { get; internal set; } = RuntimeLevel.Unknown;

        /// <inheritdoc />
        public RuntimeLevelReason Reason { get; internal set; } = RuntimeLevelReason.Unknown;

        /// <inheritdoc />
        public BootFailedException BootFailedException { get; internal set; }

        /// <inheritdoc />
        public void DetermineRuntimeLevel()
        {
            if (_databaseFactory.Configured == false)
            {
                // local version *does* match code version, but the database is not configured
                // install - may happen with Deploy/Cloud/etc
                _logger.LogDebug("Database is not configured, need to install Umbraco.");
                Level = RuntimeLevel.Install;
                Reason = RuntimeLevelReason.InstallNoDatabase;
                return;
            }

            // Check the database state, whether we can connect or if it's in an upgrade or empty state, etc...

            switch (GetUmbracoDatabaseState(_databaseFactory))
            {
                     case UmbracoDatabaseState.CannotConnect:
                    {
                        // cannot connect to configured database, this is bad, fail
                        _logger.LogDebug("Could not connect to database.");

                        if (_globalSettings.InstallMissingDatabase)
                        {
                            // ok to install on a configured but missing database
                            Level = RuntimeLevel.Install;
                            Reason = RuntimeLevelReason.InstallMissingDatabase;
                            return;
                        }

                        // else it is bad enough that we want to throw
                        Reason = RuntimeLevelReason.BootFailedCannotConnectToDatabase;
                        throw new BootFailedException("A connection string is configured but Umbraco could not connect to the database.");
                    }
                case UmbracoDatabaseState.NotInstalled:
                    {
                        // ok to install on an empty database
                        Level = RuntimeLevel.Install;
                        Reason = RuntimeLevelReason.InstallEmptyDatabase;
                        return;
                    }
                case UmbracoDatabaseState.NeedsUpgrade:
                    {
                        // the db version does not match... but we do have a migration table
                        // so, at least one valid table, so we quite probably are installed & need to upgrade

                        // although the files version matches the code version, the database version does not
                        // which means the local files have been upgraded but not the database - need to upgrade
                        _logger.LogDebug("Has not reached the final upgrade step, need to upgrade Umbraco.");
                        Level = RuntimeLevel.Upgrade;
                        Reason = RuntimeLevelReason.UpgradeMigrations;
                    }
                    break;
                case UmbracoDatabaseState.Ok:
                default:
                    {
                        // if we already know we want to upgrade, exit here
                        if (Level == RuntimeLevel.Upgrade)
                            return;

                        // the database version matches the code & files version, all clear, can run
                        Level = RuntimeLevel.Run;
                        Reason = RuntimeLevelReason.Run;
                    }
                    break;
            }
        }

        private enum UmbracoDatabaseState
        {
            Ok,
            CannotConnect,
            NotInstalled,
            NeedsUpgrade
        }

        private UmbracoDatabaseState GetUmbracoDatabaseState(IUmbracoDatabaseFactory databaseFactory)
        {
            try
            {
                if (!TryDbConnect(databaseFactory))
                {
                    return UmbracoDatabaseState.CannotConnect;
                }

                // no scope, no service - just directly accessing the database
                using (var database = databaseFactory.CreateDatabase())
                {
                    if (!database.IsUmbracoInstalled())
                    {
                        return UmbracoDatabaseState.NotInstalled;
                    }

                    if (DoesUmbracoRequireUpgrade(database))
                    {
                        return UmbracoDatabaseState.NeedsUpgrade;
                    }
                }

                return UmbracoDatabaseState.Ok;
            }
            catch (Exception e)
            {
                // can connect to the database so cannot check the upgrade state... oops
                _logger.LogWarning(e, "Could not check the upgrade state.");

                // else it is bad enough that we want to throw
                Reason = RuntimeLevelReason.BootFailedCannotCheckUpgradeState;
                throw new BootFailedException("Could not check the upgrade state.", e);
            }
        }

        public void Configure(RuntimeLevel level, RuntimeLevelReason reason)
        {
            Level = level;
            Reason = reason;
        }

        public void DoUnattendedInstall()
        {
             // unattended install is not enabled
            if (_globalSettings.InstallUnattended == false) return;

            // no connection string set
            if (_databaseFactory.Configured == false) return;

            var connect = false;
            var tries = _globalSettings.InstallMissingDatabase ? 2 : 5;
            for (var i = 0;;)
            {
                connect = _databaseFactory.CanConnect;
                if (connect || ++i == tries) break;
                _logger.LogDebug("Could not immediately connect to database, trying again.");
                Thread.Sleep(1000);
            }

            // could not connect to the database
            if (connect == false) return;

            using (var database = _databaseFactory.CreateDatabase())
            {
                var hasUmbracoTables = database.IsUmbracoInstalled();

                // database has umbraco tables, assume Umbraco is already installed
                if (hasUmbracoTables) return;

                // all conditions fulfilled, do the install
                _logger.LogInformation("Starting unattended install.");

                try
                {
                    database.BeginTransaction();
                    var creator = _databaseSchemaCreatorFactory.Create(database);
                    creator.InitializeDatabaseSchema();
                    database.CompleteTransaction();
                    _logger.LogInformation("Unattended install completed.");
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "Error during unattended install.");
                    database.AbortTransaction();

                    throw new UnattendedInstallException(
                        "The database configuration failed with the following message: " + ex.Message
                        + "\n Please check log file for additional information (can be found in '/App_Data/Logs/')");
                }
            }
        }

        private bool EnsureUmbracoUpgradeState(IUmbracoDatabaseFactory databaseFactory, ILogger logger)
        {
            var upgrader = new Upgrader(new UmbracoPlan(_umbracoVersion));
            var stateValueKey = upgrader.StateValueKey;

            // no scope, no service - just directly accessing the database
            using (var database = databaseFactory.CreateDatabase())
            {
                CurrentMigrationState = database.GetFromKeyValueTable(stateValueKey);
                FinalMigrationState = upgrader.Plan.FinalState;
            }

            logger.LogDebug("Final upgrade state is {FinalMigrationState}, database contains {DatabaseState}", FinalMigrationState, CurrentMigrationState ?? "<null>");

            return CurrentMigrationState == FinalMigrationState;
        }
        private bool DoesUmbracoRequireUpgrade(IUmbracoDatabase database)
        {
            var upgrader = new Upgrader(new UmbracoPlan(_umbracoVersion));
            var stateValueKey = upgrader.StateValueKey;

            CurrentMigrationState = database.GetFromKeyValueTable(stateValueKey);
            FinalMigrationState = upgrader.Plan.FinalState;

            _logger.LogDebug("Final upgrade state is {FinalMigrationState}, database contains {DatabaseState}", FinalMigrationState, CurrentMigrationState ?? "<null>");

            return CurrentMigrationState != FinalMigrationState;
        }

        private bool TryDbConnect(IUmbracoDatabaseFactory databaseFactory)
        {
            // anything other than install wants a database - see if we can connect
            // (since this is an already existing database, assume localdb is ready)
            bool canConnect;
            var tries = _globalSettings.InstallMissingDatabase ? 2 : 5;
            for (var i = 0; ;)
            {
                canConnect = databaseFactory.CanConnect;
                if (canConnect || ++i == tries) break;
                _logger.LogDebug("Could not immediately connect to database, trying again.");
                Thread.Sleep(1000);
            }

            return canConnect;
        }


    }
}
