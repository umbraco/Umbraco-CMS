using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Runtime
{

    /// <summary>
    /// Represents the state of the Umbraco runtime.
    /// </summary>
    public class RuntimeState : IRuntimeState
    {
        private readonly IOptions<GlobalSettings> _globalSettings;
        private readonly IOptions<UnattendedSettings> _unattendedSettings;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IUmbracoDatabaseFactory _databaseFactory;
        private readonly ILogger<RuntimeState> _logger;
        private readonly PackageMigrationPlanCollection _packageMigrationPlans;

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
            IOptions<UnattendedSettings> unattendedSettings,
            IUmbracoVersion umbracoVersion,
            IUmbracoDatabaseFactory databaseFactory,
            ILogger<RuntimeState> logger,
            PackageMigrationPlanCollection packageMigrationPlans)
        {
            _globalSettings = globalSettings;
            _unattendedSettings = unattendedSettings;
            _umbracoVersion = umbracoVersion;
            _databaseFactory = databaseFactory;
            _logger = logger;
            _packageMigrationPlans = packageMigrationPlans;
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

                    if (_globalSettings.Value.InstallMissingDatabase)
                    {
                        // ok to install on a configured but missing database
                        Level = RuntimeLevel.Install;
                        Reason = RuntimeLevelReason.InstallMissingDatabase;
                        return;
                    }

                    // else it is bad enough that we want to throw
                    Reason = RuntimeLevelReason.BootFailedCannotConnectToDatabase;
                    BootFailedException = new BootFailedException("A connection string is configured but Umbraco could not connect to the database.");
                    throw BootFailedException;
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
                    Level = _unattendedSettings.Value.UpgradeUnattended ? RuntimeLevel.Run : RuntimeLevel.Upgrade;
                    Reason = RuntimeLevelReason.UpgradeMigrations;
                }
                break;
                case UmbracoDatabaseState.NeedsPackageMigration:

                    _logger.LogDebug("Package migrations need to execute.");
                    Level = _unattendedSettings.Value.PackageMigrationsUnattended ? RuntimeLevel.Run : RuntimeLevel.PackageMigrations;
                    Reason = RuntimeLevelReason.UpgradePackageMigrations;

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
            NeedsUpgrade,
            NeedsPackageMigration
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

                    // Make ONE SQL call to determine Umbraco upgrade vs package migrations state.
                    // All will be prefixed with the same key.
                    IReadOnlyDictionary<string, string> keyValues = database.GetFromKeyValueTable(Constants.Conventions.Migrations.KeyValuePrefix);

                    // This could need both an upgrade AND package migrations to execute but
                    // we will process them one at a time, first the upgrade, then the package migrations.
                    if (DoesUmbracoRequireUpgrade(keyValues))
                    {
                        return UmbracoDatabaseState.NeedsUpgrade;
                    }

                    // TODO: We will need to scan for implicit migrations.

                    // TODO: Can we save the result of this since we'll need to re-use it?
                    IReadOnlyList<string> packagesRequiringMigration = DoesUmbracoRequirePackageMigrations(keyValues);
                    if (packagesRequiringMigration.Count > 0)
                    {
                        return UmbracoDatabaseState.NeedsPackageMigration;
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
                BootFailedException = new BootFailedException("Could not check the upgrade state.", e);
                throw BootFailedException;
            }
        }

        public void Configure(RuntimeLevel level, RuntimeLevelReason reason, Exception bootFailedException = null)
        {
            Level = level;
            Reason = reason;

            if (bootFailedException != null)
            {
                BootFailedException = new BootFailedException(bootFailedException.Message, bootFailedException);
            }
        }

        private bool DoesUmbracoRequireUpgrade(IReadOnlyDictionary<string, string> keyValues)
        {
            var upgrader = new Upgrader(new UmbracoPlan(_umbracoVersion));
            var stateValueKey = upgrader.StateValueKey;

            _ = keyValues.TryGetValue(stateValueKey, out var value);

            CurrentMigrationState = value;
            FinalMigrationState = upgrader.Plan.FinalState;

            _logger.LogDebug("Final upgrade state is {FinalMigrationState}, database contains {DatabaseState}", FinalMigrationState, CurrentMigrationState ?? "<null>");

            return CurrentMigrationState != FinalMigrationState;
        }

        private IReadOnlyList<string> DoesUmbracoRequirePackageMigrations(IReadOnlyDictionary<string, string> keyValues)
        {
            var packageMigrationPlans = _packageMigrationPlans.ToList();

            var result = new List<string>(packageMigrationPlans.Count);

            foreach (PackageMigrationPlan plan in packageMigrationPlans)
            {
                string currentMigrationState = null;
                var planKeyValueKey = Constants.Conventions.Migrations.KeyValuePrefix + plan.Name;
                if (keyValues.TryGetValue(planKeyValueKey, out var value))
                {
                    currentMigrationState = value;

                    if (plan.FinalState != value)
                    {
                        // Not equal so we need to run
                        result.Add(plan.Name);
                    }
                }
                else
                {
                    // If there is nothing in the DB then we need to run
                    result.Add(plan.Name);
                }

                _logger.LogDebug("Final package migration for {PackagePlan} state is {FinalMigrationState}, database contains {DatabaseState}",
                    plan.Name,
                    plan.FinalState,
                    currentMigrationState ?? "<null>");
            }

            return result;
        }

        private bool TryDbConnect(IUmbracoDatabaseFactory databaseFactory)
        {
            // anything other than install wants a database - see if we can connect
            // (since this is an already existing database, assume localdb is ready)
            bool canConnect;
            var tries = _globalSettings.Value.InstallMissingDatabase ? 2 : 5;
            for (var i = 0; ;)
            {
                canConnect = databaseFactory.CanConnect;
                if (canConnect || ++i == tries)
                    break;
                _logger.LogDebug("Could not immediately connect to database, trying again.");
                Thread.Sleep(1000);
            }

            return canConnect;
        }


    }
}
