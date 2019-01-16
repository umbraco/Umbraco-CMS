using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using Semver;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Sync;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents the state of the Umbraco runtime.
    /// </summary>
    internal class RuntimeState : IRuntimeState
    {
        private readonly ILogger _logger;
        private readonly IUmbracoSettingsSection _settings;
        private readonly IGlobalSettings _globalSettings;
        private readonly HashSet<string> _applicationUrls = new HashSet<string>();
        private readonly Lazy<IMainDom> _mainDom;
        private readonly Lazy<IServerRegistrar> _serverRegistrar;
        private RuntimeLevel _level = RuntimeLevel.Unknown;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeState"/> class.
        /// </summary>
        public RuntimeState(ILogger logger, IUmbracoSettingsSection settings, IGlobalSettings globalSettings,
            Lazy<IMainDom> mainDom, Lazy<IServerRegistrar> serverRegistrar)
        {
            _logger = logger;
            _settings = settings;
            _globalSettings = globalSettings;
            _mainDom = mainDom;
            _serverRegistrar = serverRegistrar;
        }

        /// <summary>
        /// Gets the server registrar.
        /// </summary>
        /// <remarks>
        /// <para>This is NOT exposed in the interface.</para>
        /// </remarks>
        private IServerRegistrar ServerRegistrar => _serverRegistrar.Value;

        /// <summary>
        /// Gets the application MainDom.
        /// </summary>
        /// <remarks>
        /// <para>This is NOT exposed in the interface as MainDom is internal.</para>
        /// </remarks>
        public IMainDom MainDom => _mainDom.Value;

        /// <inheritdoc />
        public Version Version => UmbracoVersion.Current;

        /// <inheritdoc />
        public string VersionComment => UmbracoVersion.Comment;

        /// <inheritdoc />
        public SemVersion SemanticVersion => UmbracoVersion.SemanticVersion;

        /// <inheritdoc />
        public bool Debug { get; } = GlobalSettings.DebugMode;

        /// <inheritdoc />
        public bool IsMainDom => MainDom.IsMainDom;

        /// <inheritdoc />
        public ServerRole ServerRole => ServerRegistrar.GetCurrentServerRole();

        /// <inheritdoc />
        public Uri ApplicationUrl { get; private set; }

        /// <inheritdoc />
        public string ApplicationVirtualPath { get; } = HttpRuntime.AppDomainAppVirtualPath;

        /// <inheritdoc />
        public string CurrentMigrationState { get; internal set; }

        /// <inheritdoc />
        public string FinalMigrationState { get; internal set; }

        /// <inheritdoc />
        public RuntimeLevel Level
        {
            get => _level;
            internal set { _level = value; if (value == RuntimeLevel.Run) _runLevel.Set(); }
        }

        /// <inheritdoc />
        public RuntimeLevelReason Reason { get; internal set; }

        /// <summary>
        /// Ensures that the <see cref="ApplicationUrl"/> property has a value.
        /// </summary>
        /// <param name="request"></param>
        internal void EnsureApplicationUrl(HttpRequestBase request = null)
        {
            // see U4-10626 - in some cases we want to reset the application url
            // (this is a simplified version of what was in 7.x)
            // note: should this be optional? is it expensive?
            var url = request == null ? null : ApplicationUrlHelper.GetApplicationUrlFromCurrentRequest(request, _globalSettings);
            var change = url != null && !_applicationUrls.Contains(url);
            if (change)
            {
                _logger.Info(typeof(ApplicationUrlHelper), "New url {Url} detected, re-discovering application url.", url);
                _applicationUrls.Add(url);
            }

            if (ApplicationUrl != null && !change) return;
            ApplicationUrl = new Uri(ApplicationUrlHelper.GetApplicationUrl(_logger, _globalSettings, _settings, ServerRegistrar, request));
        }

        private readonly ManualResetEventSlim _runLevel = new ManualResetEventSlim(false);

        /// <summary>
        /// Waits for the runtime level to become RuntimeLevel.Run.
        /// </summary>
        /// <param name="timeout">A timeout.</param>
        /// <returns>True if the runtime level became RuntimeLevel.Run before the timeout, otherwise false.</returns>
        internal bool WaitForRunLevel(TimeSpan timeout)
        {
            return _runLevel.WaitHandle.WaitOne(timeout);
        }

        /// <inheritdoc />
        public BootFailedException BootFailedException { get; internal set; }

        /// <summary>
        /// Determines the runtime level.
        /// </summary>
        public void DetermineRuntimeLevel(IUmbracoDatabaseFactory databaseFactory, ILogger logger)
        {
            var localVersion = UmbracoVersion.LocalVersion; // the local, files, version
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
                // install (again? this is a weird situation...)
                logger.Debug<RuntimeState>("Database is not configured, need to install Umbraco.");
                Level = RuntimeLevel.Install;
                Reason = RuntimeLevelReason.InstallNoDatabase;
                return;
            }

            // else, keep going,
            // anything other than install wants a database - see if we can connect
            // (since this is an already existing database, assume localdb is ready)
            var tries = RuntimeStateOptions.InstallMissingDatabase ? 2 : 5;
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

                if (RuntimeStateOptions.InstallMissingDatabase)
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

                if (RuntimeStateOptions.InstallEmptyDatabase)
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

        protected virtual bool EnsureUmbracoUpgradeState(IUmbracoDatabaseFactory databaseFactory, ILogger logger)
        {
            var upgrader = new UmbracoUpgrader();
            var stateValueKey = upgrader.StateValueKey;

            // no scope, no service - just directly accessing the database
            using (var database = databaseFactory.CreateDatabase())
            {
                CurrentMigrationState = KeyValueService.GetValue(database, stateValueKey);
                FinalMigrationState = upgrader.Plan.FinalState;
            }

            logger.Debug<RuntimeState>("Final upgrade state is {FinalMigrationState}, database contains {DatabaseState}", CurrentMigrationState, FinalMigrationState ?? "<null>");

            return CurrentMigrationState == FinalMigrationState;
        }
    }
}
