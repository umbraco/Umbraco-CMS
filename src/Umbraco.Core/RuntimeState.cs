using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Threading;
using System.Web;
using Semver;
using Umbraco.Core.Collections;
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
        private readonly ConcurrentHashSet<string> _applicationUrls = new ConcurrentHashSet<string>();
        private readonly Lazy<IMainDom> _mainDom;
        private readonly Lazy<IServerRegistrar> _serverRegistrar;

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
        public RuntimeLevel Level { get; internal set; } = RuntimeLevel.Unknown;

        /// <inheritdoc />
        public RuntimeLevelReason Reason { get; internal set; } = RuntimeLevelReason.Unknown;

        /// <summary>
        /// Ensures that the <see cref="ApplicationUrl"/> property has a value.
        /// </summary>
        /// <param name="request"></param>
        internal void EnsureApplicationUrl(HttpRequestBase request = null)
        {
            //Fixme: This causes problems with site swap on azure because azure pre-warms a site by calling into `localhost` and when it does that
            // it changes the URL to `localhost:80` which actually doesn't work for pinging itself, it only works internally in Azure. The ironic part
            // about this is that this is here specifically for the slot swap scenario https://issues.umbraco.org/issue/U4-10626


            // see U4-10626 - in some cases we want to reset the application URL
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

        /// <inheritdoc />
        public BootFailedException BootFailedException { get; internal set; }

        /// <summary>
        /// Determines the runtime level.
        /// </summary>
        public void DetermineRuntimeLevel(IUmbracoDatabaseFactory databaseFactory)
        {
            var localVersion = UmbracoVersion.LocalVersion; // the local, files, version
            var codeVersion = SemanticVersion; // the executing code version

            if (localVersion == null)
            {
                // there is no local version, we are not installed
                _logger.Debug<RuntimeState>("No local version, need to install Umbraco.");
                Level = RuntimeLevel.Install;
                Reason = RuntimeLevelReason.InstallNoVersion;
                return;
            }

            if (localVersion < codeVersion)
            {
                // there *is* a local version, but it does not match the code version
                // need to upgrade
                _logger.Debug<RuntimeState, SemVersion, SemVersion>("Local version '{LocalVersion}' < code version '{CodeVersion}', need to upgrade Umbraco.", localVersion, codeVersion);
                Level = RuntimeLevel.Upgrade;
                Reason = RuntimeLevelReason.UpgradeOldVersion;
            }
            else if (localVersion > codeVersion)
            {
                _logger.Warn<RuntimeState, SemVersion, SemVersion>("Local version '{LocalVersion}' > code version '{CodeVersion}', downgrading is not supported.", localVersion, codeVersion);

                // in fact, this is bad enough that we want to throw
                Reason = RuntimeLevelReason.BootFailedCannotDowngrade;
                throw new BootFailedException($"Local version \"{localVersion}\" > code version \"{codeVersion}\", downgrading is not supported.");
            }
            else if (databaseFactory.Configured == false)
            {
                // local version *does* match code version, but the database is not configured
                // install - may happen with Deploy/Cloud/etc
                _logger.Debug<RuntimeState>("Database is not configured, need to install Umbraco.");
                Level = RuntimeLevel.Install;
                Reason = RuntimeLevelReason.InstallNoDatabase;
                return;
            }

            // Check the database state, whether we can connect or if it's in an upgrade or empty state, etc...

            switch (GetUmbracoDatabaseState(databaseFactory))
            {
                case UmbracoDatabaseState.CannotConnect:
                    {
                        // cannot connect to configured database, this is bad, fail
                        _logger.Debug<RuntimeState>("Could not connect to database.");

                        if (RuntimeOptions.InstallMissingDatabase)
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
                        _logger.Debug<RuntimeState>("Has not reached the final upgrade step, need to upgrade Umbraco.");
                        Level = RuntimeOptions.UpgradeUnattended ? RuntimeLevel.Run : RuntimeLevel.Upgrade;
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
                    if (!database.IsUmbracoInstalled(_logger))
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
                _logger.Warn<RuntimeState>(e, "Could not check the upgrade state.");

                // else it is bad enough that we want to throw
                Reason = RuntimeLevelReason.BootFailedCannotCheckUpgradeState;
                throw new BootFailedException("Could not check the upgrade state.", e);
            }
        }

        private bool TryDbConnect(IUmbracoDatabaseFactory databaseFactory)
        {
            // anything other than install wants a database - see if we can connect
            // (since this is an already existing database, assume localdb is ready)
            bool canConnect;
            var tries = RuntimeOptions.InstallMissingDatabase ? 2 : 5;
            for (var i = 0; ;)
            {
                canConnect = databaseFactory.CanConnect;
                if (canConnect || ++i == tries) break;
                _logger.Debug<RuntimeState>("Could not immediately connect to database, trying again.");
                Thread.Sleep(1000);
            }

            return canConnect;
        }

        private bool DoesUmbracoRequireUpgrade(IUmbracoDatabase database)
        {
            var upgrader = new Upgrader(new UmbracoPlan());
            var stateValueKey = upgrader.StateValueKey;

            CurrentMigrationState = KeyValueService.GetValue(database, stateValueKey);
            FinalMigrationState = upgrader.Plan.FinalState;

            _logger.Debug<RuntimeState, string, string>("Final upgrade state is {FinalMigrationState}, database contains {DatabaseState}", FinalMigrationState, CurrentMigrationState ?? "<null>");

            return CurrentMigrationState != FinalMigrationState;
        }
    }
}
