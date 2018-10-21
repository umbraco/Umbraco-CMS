using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.Composers;
using Umbraco.Core.Configuration;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Runtime
{
    /// <summary>
    /// Represents the Core Umbraco runtime.
    /// </summary>
    /// <remarks>Does not handle any of the web-related aspects of Umbraco (startup, etc). It
    /// should be possible to use this runtime in console apps.</remarks>
    public class CoreRuntime : IRuntime
    {
        private readonly UmbracoApplicationBase _app;
        private BootLoader _bootLoader;
        private RuntimeState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRuntime"/> class.
        /// </summary>
        /// <param name="umbracoApplication">The Umbraco HttpApplication.</param>
        public CoreRuntime(UmbracoApplicationBase umbracoApplication)
        {
            _app = umbracoApplication ?? throw new ArgumentNullException(nameof(umbracoApplication));
        }

        /// <inheritdoc/>
        public virtual void Boot(IContainer container)
        {
            // some components may want to initialize with the UmbracoApplicationBase
            // well, they should not - we should not do this
            // TODO remove this eventually.
            container.RegisterInstance(_app);

            Compose(container);

            // prepare essential stuff

            var path = GetApplicationRootPath();
            if (string.IsNullOrWhiteSpace(path) == false)
                IOHelper.SetRootDirectory(path);

            _state = (RuntimeState) container.GetInstance<IRuntimeState>();
            _state.Level = RuntimeLevel.Boot;

            Logger = container.GetInstance<ILogger>();
            Profiler = container.GetInstance<IProfiler>();
            ProfilingLogger = container.GetInstance<ProfilingLogger>();

            // the boot loader boots using a container scope, so anything that is PerScope will
            // be disposed after the boot loader has booted, and anything else will remain.
            // note that this REQUIRES that perWebRequestScope has NOT been enabled yet, else
            // the container will fail to create a scope since there is no http context when
            // the application starts.
            // the boot loader is kept in the runtime for as long as Umbraco runs, and components
            // are NOT disposed - which is not a big deal as long as they remain lightweight
            // objects.

            using (var bootTimer = ProfilingLogger.TraceDuration<CoreRuntime>(
                $"Booting Umbraco {UmbracoVersion.SemanticVersion.ToSemanticString()} on {NetworkHelper.MachineName}.",
                "Booted.",
                "Boot failed."))
            {
                // throws if not full-trust
                new AspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted).Demand();

                try
                {
                    Logger.Debug<CoreRuntime>("Runtime: {Runtime}", GetType().FullName);

                    AquireMainDom(container);
                    DetermineRuntimeLevel(container);
                    var componentTypes = ResolveComponentTypes();
                    _bootLoader = new BootLoader(container);
                    _bootLoader.Boot(componentTypes, _state.Level);
                }
                catch (Exception e)
                {
                    _state.Level = RuntimeLevel.BootFailed;
                    var bfe = e as BootFailedException ?? new BootFailedException("Boot failed.", e);
                    _state.BootFailedException = bfe;
                    bootTimer.Fail(exception: bfe); // be sure to log the exception - even if we repeat ourselves

                    // throwing here can cause w3wp to hard-crash and we want to avoid it.
                    // instead, we're logging the exception and setting level to BootFailed.
                    // various parts of Umbraco such as UmbracoModule and UmbracoDefaultOwinStartup
                    // understand this and will nullify themselves, while UmbracoModule will
                    // throw a BootFailedException for every requests.
                }
            }

            //fixme
            // after Umbraco has started there is a scope in "context" and that context is
            // going to stay there and never get destroyed nor reused, so we have to ensure that
            // everything is cleared
            //var sa = container.GetInstance<IDatabaseScopeAccessor>();
            //sa.Scope?.Dispose();
        }

        private void AquireMainDom(IContainer container)
        {
            using (var timer = ProfilingLogger.DebugDuration<CoreRuntime>("Acquiring MainDom.", "Aquired."))
            {
                try
                {
                    var mainDom = container.GetInstance<MainDom>();
                    mainDom.Acquire();
                }
                catch
                {
                    timer.Fail();
                    throw;
                }
            }
        }

        // internal for tests
        internal void DetermineRuntimeLevel(IContainer container)
        {
            using (var timer = ProfilingLogger.DebugDuration<CoreRuntime>("Determining runtime level.", "Determined."))
            {
                try
                {
                    var dbfactory = container.GetInstance<IUmbracoDatabaseFactory>();
                    SetRuntimeStateLevel(dbfactory, Logger);

                    Logger.Debug<CoreRuntime>("Runtime level: {RuntimeLevel}", _state.Level);

                    if (_state.Level == RuntimeLevel.Upgrade)
                    {
                        Logger.Debug<CoreRuntime>("Configure database factory for upgrades.");
                        dbfactory.ConfigureForUpgrade();
                    }
                }
                catch
                {
                    timer.Fail();
                    throw;
                }
            }
        }

        private IEnumerable<Type> ResolveComponentTypes()
        {
            using (var timer = ProfilingLogger.TraceDuration<CoreRuntime>("Resolving component types.", "Resolved."))
            {
                try
                {
                    return GetComponentTypes();
                }
                catch
                {
                    timer.Fail();
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public virtual void Terminate()
        {
            using (ProfilingLogger.DebugDuration<CoreRuntime>("Terminating Umbraco.", "Terminated."))
            {
                _bootLoader?.Terminate();
            }
        }

        /// <summary>
        /// Composes the runtime.
        /// </summary>
        public virtual void Compose(IContainer container)
        {
            // compose the very essential things that are needed to bootstrap, before anything else,
            // and only these things - the rest should be composed in runtime components

            // register basic things
            container.RegisterSingleton<IProfiler, LogProfiler>();
            container.RegisterSingleton<ProfilingLogger>();
            container.RegisterSingleton<IRuntimeState, RuntimeState>();

            container.ComposeConfiguration();

            // register caches
            // need the deep clone runtime cache profiver to ensure entities are cached properly, ie
            // are cloned in and cloned out - no request-based cache here since no web-based context,
            // will be overriden later or
            container.RegisterSingleton(_ => new CacheHelper(
                new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider()),
                new StaticCacheProvider(),
                NullCacheProvider.Instance,
                new IsolatedRuntimeCache(type => new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider()))));
            container.RegisterSingleton(f => f.GetInstance<CacheHelper>().RuntimeCache);

            // register the plugin manager
            container.RegisterSingleton(f => new TypeLoader(f.GetInstance<IRuntimeCacheProvider>(), f.GetInstance<IGlobalSettings>(), f.GetInstance<ProfilingLogger>()));

            // register syntax providers - required by database factory
            container.Register<ISqlSyntaxProvider, MySqlSyntaxProvider>("MySqlSyntaxProvider");
            container.Register<ISqlSyntaxProvider, SqlCeSyntaxProvider>("SqlCeSyntaxProvider");
            container.Register<ISqlSyntaxProvider, SqlServerSyntaxProvider>("SqlServerSyntaxProvider");

            // register persistence mappers - required by database factory so needs to be done here
            // means the only place the collection can be modified is in a runtime - afterwards it
            // has been frozen and it is too late
            var mapperCollectionBuilder = container.RegisterCollectionBuilder<MapperCollectionBuilder>();
            ComposeMapperCollection(mapperCollectionBuilder);

            // register database factory - required to check for migrations
            // will be initialized with syntax providers and a logger, and will try to configure
            // from the default connection string name, if possible, else will remain non-configured
            // until properly configured (eg when installing)
            container.RegisterSingleton<IUmbracoDatabaseFactory, UmbracoDatabaseFactory>();
            container.RegisterSingleton(f => f.GetInstance<IUmbracoDatabaseFactory>().SqlContext);

            // register the scope provider
            container.RegisterSingleton<ScopeProvider>(); // implements both IScopeProvider and IScopeAccessor
            container.RegisterSingleton<IScopeProvider>(f => f.GetInstance<ScopeProvider>());
            container.RegisterSingleton<IScopeAccessor>(f => f.GetInstance<ScopeProvider>());

            // register MainDom
            container.RegisterSingleton<MainDom>();
        }

        protected virtual void ComposeMapperCollection(MapperCollectionBuilder builder)
        {
            builder.AddCore();
        }

        private void SetRuntimeStateLevel(IUmbracoDatabaseFactory databaseFactory, ILogger logger)
        {
            var localVersion = UmbracoVersion.Local; // the local, files, version
            var codeVersion = _state.SemanticVersion; // the executing code version
            var connect = false;

            // we don't know yet
            _state.Level = RuntimeLevel.Unknown;

            if (localVersion == null)
            {
                // there is no local version, we are not installed
                logger.Debug<CoreRuntime>("No local version, need to install Umbraco.");
                _state.Level = RuntimeLevel.Install;
            }
            else if (localVersion < codeVersion)
            {
                // there *is* a local version, but it does not match the code version
                // need to upgrade
                logger.Debug<CoreRuntime>("Local version '{LocalVersion}' < code version '{CodeVersion}', need to upgrade Umbraco.", localVersion, codeVersion);
                _state.Level = RuntimeLevel.Upgrade;
            }
            else if (localVersion > codeVersion)
            {
                logger.Warn<CoreRuntime>("Local version '{LocalVersion}' > code version '{CodeVersion}', downgrading is not supported.", localVersion, codeVersion);
                _state.Level = RuntimeLevel.BootFailed;

                // in fact, this is bad enough that we want to throw
                throw new BootFailedException($"Local version \"{localVersion}\" > code version \"{codeVersion}\", downgrading is not supported.");
            }
            else if (databaseFactory.Configured == false)
            {
                // local version *does* match code version, but the database is not configured
                // install (again? this is a weird situation...)
                logger.Debug<CoreRuntime>("Database is not configured, need to install Umbraco.");
                _state.Level = RuntimeLevel.Install;
            }

            // install? not going to test anything else
            if (_state.Level == RuntimeLevel.Install)
                return;

            // else, keep going,
            // anything other than install wants a database - see if we can connect
            // (since this is an already existing database, assume localdb is ready)
            for (var i = 0; i < 5; i++)
            {
                connect = databaseFactory.CanConnect;
                if (connect) break;
                logger.Debug<CoreRuntime>("Could not immediately connect to database, trying again.");
                Thread.Sleep(1000);
            }

            if (connect == false)
            {
                // cannot connect to configured database, this is bad, fail
                logger.Debug<CoreRuntime>("Could not connect to database.");
                _state.Level = RuntimeLevel.BootFailed;

                // in fact, this is bad enough that we want to throw
                throw new BootFailedException("A connection string is configured but Umbraco could not connect to the database.");
            }

            // if we already know we want to upgrade, no need to look for migrations...
            if (_state.Level == RuntimeLevel.Upgrade)
                return;

            // else
            // look for a matching migration entry - bypassing services entirely - they are not 'up' yet
            // fixme - in a LB scenario, ensure that the DB gets upgraded only once!
            bool exists;
            try
            {
                exists = EnsureUmbracoUpgradeState(databaseFactory, logger);
            }
            catch (Exception e)
            {
                // can connect to the database but cannot access the migration table... need to install
                logger.Warn<CoreRuntime>(e, "Could not check the upgrade state.");
                logger.Debug<CoreRuntime>("Could not check the upgrade state, need to install Umbraco.");
                _state.Level = RuntimeLevel.Install;
                return;
            }

            if (exists)
            {
                // the database version matches the code & files version, all clear, can run
                _state.Level = RuntimeLevel.Run;
                return;
            }

            // the db version does not match... but we do have a migration table
            // so, at least one valid table, so we quite probably are installed & need to upgrade

            // although the files version matches the code version, the database version does not
            // which means the local files have been upgraded but not the database - need to upgrade
            logger.Debug<CoreRuntime>("Has not reached the final upgrade step, need to upgrade Umbraco.");
            _state.Level = RuntimeLevel.Upgrade;
        }

        protected virtual bool EnsureUmbracoUpgradeState(IUmbracoDatabaseFactory databaseFactory, ILogger logger)
        {
            // no scope, no key value service - just directly accessing the database

            var umbracoPlan = new UmbracoPlan();
            var stateValueKey = Upgrader.GetStateValueKey(umbracoPlan);

            string state;
            using (var database = databaseFactory.CreateDatabase())
            {
                var sql = databaseFactory.SqlContext.Sql()
                    .Select<KeyValueDto>()
                    .From<KeyValueDto>()
                    .Where<KeyValueDto>(x => x.Key == stateValueKey);
                state = database.FirstOrDefault<KeyValueDto>(sql)?.Value;
            }

            _state.CurrentMigrationState = state;
            _state.FinalMigrationState = umbracoPlan.FinalState;

            logger.Debug<CoreRuntime>("Final upgrade state is {FinalMigrationState}, database contains {DatabaseState}", _state.FinalMigrationState, state ?? "<null>");

            return state == _state.FinalMigrationState;
        }

        #region Locals

        protected ILogger Logger { get; private set; }

        protected IProfiler Profiler { get; private set; }

        protected ProfilingLogger ProfilingLogger { get; private set; }

        #endregion

        #region Getters

        // getters can be implemented by runtimes inheriting from CoreRuntime

        // fixme - inject! no Current!
        protected virtual IEnumerable<Type> GetComponentTypes() => Current.TypeLoader.GetTypes<IUmbracoComponent>();

        // by default, returns null, meaning that Umbraco should auto-detect the application root path.
        // override and return the absolute path to the Umbraco site/solution, if needed
        protected virtual string GetApplicationRootPath() => null;

        #endregion
    }
}
