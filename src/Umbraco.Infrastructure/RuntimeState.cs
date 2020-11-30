using System;
using System.Threading;
using Semver;
using Umbraco.Core.Collections;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Sync;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents the state of the Umbraco runtime.
    /// </summary>
    public class RuntimeState : IRuntimeState
    {
        private readonly ILogger _logger;
        private readonly IGlobalSettings _globalSettings;
        private readonly ConcurrentHashSet<string> _applicationUrls = new ConcurrentHashSet<string>();
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IBackOfficeInfo _backOfficeInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeState"/> class.
        /// </summary>
        public RuntimeState(ILogger logger, IGlobalSettings globalSettings,
            IUmbracoVersion umbracoVersion,
            IBackOfficeInfo backOfficeInfo)
        {
            _logger = logger;
            _globalSettings = globalSettings;
            _umbracoVersion = umbracoVersion;
            _backOfficeInfo = backOfficeInfo;
        }


        /// <inheritdoc />
        public Version Version => _umbracoVersion.Current;

        /// <inheritdoc />
        public string VersionComment => _umbracoVersion.Comment;

        /// <inheritdoc />
        public SemVersion SemanticVersion => _umbracoVersion.SemanticVersion;

        /// <inheritdoc />
        public Uri ApplicationUrl { get; private set; }

        /// <inheritdoc />
        public string CurrentMigrationState { get; private set; }

        /// <inheritdoc />
        public string FinalMigrationState { get; private set; }

        /// <inheritdoc />
        public RuntimeLevel Level { get; internal set; } = RuntimeLevel.Unknown;

        /// <inheritdoc />
        public RuntimeLevelReason Reason { get; internal set; } = RuntimeLevelReason.Unknown;


        /// <summary>
        /// Ensures that the <see cref="ApplicationUrl"/> property has a value.
        /// </summary>
        public void EnsureApplicationUrl()
        {
            //Fixme: This causes problems with site swap on azure because azure pre-warms a site by calling into `localhost` and when it does that
            // it changes the URL to `localhost:80` which actually doesn't work for pinging itself, it only works internally in Azure. The ironic part
            // about this is that this is here specifically for the slot swap scenario https://issues.umbraco.org/issue/U4-10626


            // see U4-10626 - in some cases we want to reset the application URL
            // (this is a simplified version of what was in 7.x)
            // note: should this be optional? is it expensive?
            var url = _backOfficeInfo.GetAbsoluteUrl;

            var change = url != null && !_applicationUrls.Contains(url);

            if (change)
            {
                _logger.Info<RuntimeState>("New url {Url} detected, re-discovering application url.", url);
                _applicationUrls.Add(url);
                ApplicationUrl = new Uri(url);
            }
        }

        /// <inheritdoc />
        public BootFailedException BootFailedException { get; internal set; }

        /// <summary>
        /// Determines the runtime level.
        /// </summary>
        public void DetermineRuntimeLevel(IUmbracoDatabaseFactory databaseFactory, ILogger logger)
        {
            var localVersion = _umbracoVersion.LocalVersion; // the local, files, version
            var codeVersion = SemanticVersion; // the executing code version
            var connect = false;

            if (localVersion == null)
            {
                // there is no local version, we are not installed
                logger.Debug<RuntimeState>("No local version, need to install Umbraco.");
                Level = RuntimeLevel.Install;
                Reason = RuntimeLevelReason.InstallNoVersion;
                return;
            }

            if (localVersion < codeVersion)
            {
                // there *is* a local version, but it does not match the code version
                // need to upgrade
                logger.Debug<RuntimeState>("Local version '{LocalVersion}' < code version '{CodeVersion}', need to upgrade Umbraco.", localVersion, codeVersion);
                Level = RuntimeLevel.Upgrade;
                Reason = RuntimeLevelReason.UpgradeOldVersion;
            }
            else if (localVersion > codeVersion)
            {
                logger.Warn<RuntimeState>("Local version '{LocalVersion}' > code version '{CodeVersion}', downgrading is not supported.", localVersion, codeVersion);

                // in fact, this is bad enough that we want to throw
                Reason = RuntimeLevelReason.BootFailedCannotDowngrade;
                throw new BootFailedException($"Local version \"{localVersion}\" > code version \"{codeVersion}\", downgrading is not supported.");
            }
            else if (databaseFactory.Configured == false)
            {
                // local version *does* match code version, but the database is not configured
                // install - may happen with Deploy/Cloud/etc
                logger.Debug<RuntimeState>("Database is not configured, need to install Umbraco.");
                Level = RuntimeLevel.Install;
                Reason = RuntimeLevelReason.InstallNoDatabase;
                return;
            }

            // else, keep going,
            // anything other than install wants a database - see if we can connect
            // (since this is an already existing database, assume localdb is ready)
            var tries = _globalSettings.InstallMissingDatabase ? 2 : 5;
            for (var i = 0;;)
            {
                connect = databaseFactory.CanConnect;
                if (connect || ++i == tries) break;
                logger.Debug<RuntimeState>("Could not immediately connect to database, trying again.");
                Thread.Sleep(1000);
            }

            if (connect == false)
            {
                // cannot connect to configured database, this is bad, fail
                logger.Debug<RuntimeState>("Could not connect to database.");

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
                noUpgrade = EnsureUmbracoUpgradeState(databaseFactory, logger);
            }
            catch (Exception e)
            {
                // can connect to the database but cannot check the upgrade state... oops
                logger.Warn<RuntimeState>(e, "Could not check the upgrade state.");

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
            logger.Debug<RuntimeState>("Has not reached the final upgrade step, need to upgrade Umbraco.");
            Level = RuntimeLevel.Upgrade;
            Reason = RuntimeLevelReason.UpgradeMigrations;
        }

        private bool EnsureUmbracoUpgradeState(IUmbracoDatabaseFactory databaseFactory, ILogger logger)
        {
            var upgrader = new Upgrader(new UmbracoPlan(_umbracoVersion, _globalSettings));
            var stateValueKey = upgrader.StateValueKey;

            // no scope, no service - just directly accessing the database
            using (var database = databaseFactory.CreateDatabase())
            {
                CurrentMigrationState = database.GetFromKeyValueTable(stateValueKey);
                FinalMigrationState = upgrader.Plan.FinalState;
            }

            logger.Debug<RuntimeState>("Final upgrade state is {FinalMigrationState}, database contains {DatabaseState}", FinalMigrationState, CurrentMigrationState ?? "<null>");

            return CurrentMigrationState == FinalMigrationState;
        }
    }
}
