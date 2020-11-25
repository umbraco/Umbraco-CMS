using System;
using System.Threading;
using Semver;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Persistence;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents the state of the Umbraco runtime.
    /// </summary>
    [UmbracoVolatile]
    public class RuntimeState : IRuntimeState
    {
        private readonly GlobalSettings _globalSettings;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IUmbracoDatabaseFactory _databaseFactory;
        private readonly ILogger<RuntimeState> _logger;

        /// <summary>
        /// The initial <see cref="RuntimeState"/>
        /// </summary>
        public static RuntimeState Booting() => new RuntimeState() { Level = RuntimeLevel.Boot };

        private RuntimeState()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeState"/> class.
        /// </summary>
        public RuntimeState(IOptions<GlobalSettings> globalSettings, IUmbracoVersion umbracoVersion, IUmbracoDatabaseFactory databaseFactory, ILogger<RuntimeState> logger)
        {
            _globalSettings = globalSettings.Value;
            _umbracoVersion = umbracoVersion;
            _databaseFactory = databaseFactory;
            _logger = logger;
        }


        /// <inheritdoc />
        public Version Version => _umbracoVersion.Current;

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

            // else, keep going,
            // anything other than install wants a database - see if we can connect
            // (since this is an already existing database, assume localdb is ready)
            var connect = false;
            var tries = _globalSettings.InstallMissingDatabase ? 2 : 5;
            for (var i = 0;;)
            {
                connect = _databaseFactory.CanConnect;
                if (connect || ++i == tries) break;
                _logger.LogDebug("Could not immediately connect to database, trying again.");
                Thread.Sleep(1000);
            }

            if (connect == false)
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

            // if we already know we want to upgrade,
            // still run EnsureUmbracoUpgradeState to get the states
            // (v7 will just get a null state, that's ok)

            // else
            // look for a matching migration entry - bypassing services entirely - they are not 'up' yet
            bool noUpgrade;
            try
            {
                noUpgrade = EnsureUmbracoUpgradeState(_databaseFactory, _logger);
            }
            catch (Exception e)
            {
                // can connect to the database but cannot check the upgrade state... oops
                _logger.LogWarning(e, "Could not check the upgrade state.");

                if (_globalSettings.InstallEmptyDatabase)
                {
                    // ok to install on an empty database
                    Level = RuntimeLevel.Install;
                    Reason = RuntimeLevelReason.InstallEmptyDatabase;
                    return;
                }

                // else it is bad enough that we want to throw
                Reason = RuntimeLevelReason.BootFailedCannotCheckUpgradeState;
                throw new BootFailedException("Could not check the upgrade state.", e);
            }

            // if we already know we want to upgrade, exit here
            if (Level == RuntimeLevel.Upgrade)
                return;

            if (noUpgrade)
            {
                // the database version matches the code & files version, all clear, can run
                Level = RuntimeLevel.Run;
                Reason = RuntimeLevelReason.Run;
                return;
            }

            // the db version does not match... but we do have a migration table
            // so, at least one valid table, so we quite probably are installed & need to upgrade

            // although the files version matches the code version, the database version does not
            // which means the local files have been upgraded but not the database - need to upgrade
            _logger.LogDebug("Has not reached the final upgrade step, need to upgrade Umbraco.");
            Level = RuntimeLevel.Upgrade;
            Reason = RuntimeLevelReason.UpgradeMigrations;
        }

        public void Configure(RuntimeLevel level, RuntimeLevelReason reason)
        {
            Level = level;
            Reason = reason;
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
    }
}
