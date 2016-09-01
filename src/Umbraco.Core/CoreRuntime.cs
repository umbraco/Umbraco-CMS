using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Threading;
using LightInject;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Configuration;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Plugins;
using Umbraco.Core.Services;

namespace Umbraco.Core
{

    /// <summary>
    /// Represents the Core Umbraco runtime.
    /// </summary>
    /// <remarks>Does not handle any of the web-related aspects of Umbraco (startup, etc). It
    /// should be possible to use this runtime in console apps.</remarks>
    public class CoreRuntime : IRuntime
    {
        private BootLoader _bootLoader;
        private RuntimeState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRuntime"/> class.
        /// </summary>
        /// <param name="umbracoApplication">The Umbraco HttpApplication.</param>
        public CoreRuntime(UmbracoApplicationBase umbracoApplication)
        {
            if (umbracoApplication == null) throw new ArgumentNullException(nameof(umbracoApplication));
            UmbracoApplication = umbracoApplication;
        }

        /// <inheritdoc/>
        public virtual void Boot(ServiceContainer container)
        {
            Compose(container);

            // prepare essential stuff

            _state = (RuntimeState) container.GetInstance<IRuntimeState>();
            _state.Level = RuntimeLevel.Boot;

            Logger = container.GetInstance<ILogger>();
            Profiler = container.GetInstance<IProfiler>();
            ProfilingLogger = container.GetInstance<ProfilingLogger>();

            // fixme - totally temp
            container.RegisterInstance(UmbracoApplication);

            // the boot loader boots using a container scope, so anything that is PerScope will
            // be disposed after the boot loader has booted, and anything else will remain.
            // note that this REQUIRES that perWebRequestScope has NOT been enabled yet, else
            // the container will fail to create a scope since there is no http context when
            // the application starts.
            // the boot loader is kept in the runtime for as long as Umbraco runs, and components
            // are NOT disposed - which is not a big deal as long as they remain lightweight
            // objects.

            using (ProfilingLogger.TraceDuration<CoreRuntime>($"Booting Umbraco {UmbracoVersion.SemanticVersion.ToSemanticString()} on {NetworkHelper.MachineName}.", "Booted."))
            {
                Logger.Debug<CoreRuntime>($"Runtime: {GetType().FullName}");

                using (ProfilingLogger.DebugDuration<CoreRuntime>("Acquiring MainDom.", "Aquired."))
                {
                    // become the main domain - before anything else
                    var mainDom = container.GetInstance<MainDom>();
                    mainDom.Acquire();
                }

                using (ProfilingLogger.DebugDuration<CoreRuntime>("Determining runtime level.", "Determined."))
                {
                    var dbfactory = container.GetInstance<IDatabaseFactory>();
                    SetRuntimeStateLevel(_state, dbfactory, Logger);
                }

                Logger.Debug<CoreRuntime>($"Runtime level: {_state.Level}");

                IEnumerable<Type> componentTypes;
                using (ProfilingLogger.TraceDuration<CoreRuntime>("Resolving component types.", "Resolved."))
                {
                    componentTypes = GetComponentTypes();
                }

                // boot
                _bootLoader = new BootLoader(container);
                _bootLoader.Boot(componentTypes, _state.Level);
            }
        }

        /// <inheritdoc/>
        public virtual void Terminate()
        {
            _bootLoader?.Terminate();
        }

        /// <inheritdoc/>
        public virtual void Compose(ServiceContainer container)
        {
            container.RegisterSingleton<IProfiler, LogProfiler>();
            container.RegisterSingleton<ProfilingLogger>();
            container.RegisterSingleton<IRuntimeState, RuntimeState>();

            container.RegisterSingleton(_ => new CacheHelper(
                // we need to have the dep clone runtime cache provider to ensure
                // all entities are cached properly (cloned in and cloned out)
                new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider()),
                new StaticCacheProvider(),
                // we have no request based cache when not running in web-based context
                new NullCacheProvider(),
                new IsolatedRuntimeCache(type =>
                    // we need to have the dep clone runtime cache provider to ensure
                    // all entities are cached properly (cloned in and cloned out)
                    new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider()))));
            container.RegisterSingleton(factory => factory.GetInstance<CacheHelper>().RuntimeCache);

            container.RegisterSingleton(factory => new PluginManager(factory.GetInstance<IRuntimeCacheProvider>(), factory.GetInstance<ProfilingLogger>()));

            // register syntax providers
            container.Register<ISqlSyntaxProvider, MySqlSyntaxProvider>("MySqlSyntaxProvider");
            container.Register<ISqlSyntaxProvider, SqlCeSyntaxProvider>("SqlCeSyntaxProvider");
            container.Register<ISqlSyntaxProvider, SqlServerSyntaxProvider>("SqlServerSyntaxProvider");

            // register persistence mappers - means the only place the collection can be modified
            // is in a runtime - afterwards it has been frozen and it is too late
            MapperCollectionBuilder.Register(container)
                .AddProducer(f => f.GetInstance<PluginManager>().ResolveAssignedMapperTypes());

            // register database factory
            // will be initialized with syntax providers and a logger, and will try to configure
            // from the default connection string name, if possible, else will remain non-configured
            // until the database context configures it properly (eg when installing)
            container.RegisterSingleton<IDatabaseFactory, DefaultDatabaseFactory>();

            // register a database accessor - will be replaced
            // by HybridUmbracoDatabaseAccessor in the web runtime
            container.RegisterSingleton<IUmbracoDatabaseAccessor, ThreadStaticUmbracoDatabaseAccessor>();

            // register MainDom
            container.RegisterSingleton<MainDom>();
        }

        private static void SetRuntimeStateLevel(RuntimeState runtimeState, IDatabaseFactory databaseFactory, ILogger logger)
        {
            var localVersion = LocalVersion; // the local, files, version
            var codeVersion = runtimeState.SemanticVersion; // the executing code version
            var connect = false;

            if (string.IsNullOrWhiteSpace(localVersion))
            {
                // there is no local version, we are not installed
                logger.Debug<CoreRuntime>("No local version, need to install Umbraco.");
                runtimeState.Level = RuntimeLevel.Install;
            }
            else if (localVersion != codeVersion)
            {
                // there *is* a local version, but it does not match the code version
                // need to upgrade
                logger.Debug<CoreRuntime>($"Local version \"{localVersion}\" != code version, need to upgrade Umbraco.");
                runtimeState.Level = RuntimeLevel.Upgrade;
            }
            else if (databaseFactory.Configured == false)
            {
                // local version *does* match code version, but the database is not configured
                // install (again? this is a weird situation...)
                logger.Debug<CoreRuntime>("Database is not configured, need to install Umbraco.");
                runtimeState.Level = RuntimeLevel.Install;
            }

            // anything other than install wants a database - see if we can connect
            if (runtimeState.Level != RuntimeLevel.Install)
            {
                for (var i = 0; i < 5; i++)
                {
                    connect = databaseFactory.CanConnect;
                    if (connect) break;
                    logger.Debug<CoreRuntime>(i == 0
                        ? "Could not immediately connect to database, trying again."
                        : "Could not connect to database.");
                    Thread.Sleep(1000);
                }

                if (connect == false)
                {
                    // cannot connect to configured database, this is bad, fail
                    logger.Debug<CoreRuntime>("Could not connect to database.");
                    runtimeState.Level = RuntimeLevel.Failed;

                    // in fact, this is bad enough that we want to throw
                    throw new BootFailedException("A connection string is configured but Umbraco could not connect to the database.");
                }
            }

            // if we cannot connect, cannot do more
            if (connect == false) return;

            // else
            // look for a matching migration entry - bypassing services entirely - they are not 'up' yet
            // fixme - in a LB scenario, ensure that the DB gets upgraded only once!
            // fixme - eventually move to yol-style guid-based transitions
            var database = databaseFactory.GetDatabase();
            var codeVersionString = codeVersion.ToString();
            var sql = database.Sql()
                .Select<MigrationDto>()
                .From<MigrationDto>()
                .Where<MigrationDto>(x => x.Name.InvariantEquals(GlobalSettings.UmbracoMigrationName) && x.Version == codeVersionString);
            bool exists;
            try
            {
                exists = database.FirstOrDefault<MigrationDto>(sql) != null;
            }
            catch
            {
                // can connect to the database but cannot access the migration table... need to install
                logger.Debug<CoreRuntime>("Could not check migrations, need to install Umbraco.");
                runtimeState.Level = RuntimeLevel.Install;
                return;
            }

            if (exists)
            {
                // the database version matches the code & files version, all clear, can run
                runtimeState.Level = RuntimeLevel.Run;
                return;
            }

            // the db version does not match... but we do have a migration table
            // so, at least one valid table, so we quite probably are installed & need to upgrade

            // although the files version matches the code version, the database version does not
            // which means the local files have been upgraded but not the database - need to upgrade
            logger.Debug<CoreRuntime>("Database migrations have not executed, need to upgrade Umbraco.");
            runtimeState.Level = RuntimeLevel.Upgrade;
        }

        private static string LocalVersion
        {
            get
            {
                try
                {
                    // fixme - this should live in its own independent file! NOT web.config!
                    return ConfigurationManager.AppSettings["umbracoConfigurationStatus"];
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        #region Locals

        // fixme - we almost certainly DONT need these!

        protected ILogger Logger { get; private set; }

        protected IProfiler Profiler { get; private set; }

        protected ProfilingLogger ProfilingLogger { get; private set; }

        #endregion

        #region Getters

        // getters can be implemented by runtimes inheriting from CoreRuntime

        protected virtual IEnumerable<Type> GetComponentTypes() => Current.PluginManager.ResolveTypes<IUmbracoComponent>();

        //protected virtual IProfiler GetProfiler() => new LogProfiler(Logger);

        //protected virtual CacheHelper GetApplicationCache() => new CacheHelper(
        //        // we need to have the dep clone runtime cache provider to ensure
        //        // all entities are cached properly (cloned in and cloned out)
        //        new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider()),
        //        new StaticCacheProvider(),
        //        // we have no request based cache when not running in web-based context
        //        new NullCacheProvider(),
        //        new IsolatedRuntimeCache(type =>
        //            // we need to have the dep clone runtime cache provider to ensure
        //            // all entities are cached properly (cloned in and cloned out)
        //            new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider())));

        #endregion

        // fixme - kill everything below!

        private IServiceContainer _appStartupEvtContainer;
        private bool _isInitialized;
        private bool _isStarted;
        private bool _isComplete;

        protected UmbracoApplicationBase UmbracoApplication { get; }

        protected ServiceContainer Container => Current.Container;

        /// <summary>
        /// Special method to extend the use of Umbraco by enabling the consumer to overwrite
        /// the absolute path to the root of an Umbraco site/solution, which is used for stuff
        /// like Umbraco.Core.IO.IOHelper.MapPath etc.
        /// </summary>
        /// <param name="rootPath">Absolute Umbraco root path.</param>
        protected virtual void InitializeApplicationRootPath(string rootPath)
        {
            IOHelper.SetRootDirectory(rootPath);
        }

        public virtual IRuntime Initialize()
        {
            if (_isInitialized)
                throw new InvalidOperationException("The boot manager has already been initialized");

            //now we need to call the initialize methods
            //Create a 'child'container which is a copy of all of the current registrations and begin a sub scope for it
            // this child container will be used to manage the application event handler instances and the scope will be
            // completed at the end of the boot process to allow garbage collection
            // using (Container.BeginScope()) { } // fixme - throws
            _appStartupEvtContainer = Container.Clone(); // fixme - but WHY clone? because *then* it has its own scope?! bah ;-(
            _appStartupEvtContainer.BeginScope();
            _appStartupEvtContainer.RegisterCollection<PerScopeLifetime>(factory => factory.GetInstance<PluginManager>().ResolveApplicationStartupHandlers());

            // fixme - parallel? what about our dependencies?
            Parallel.ForEach(_appStartupEvtContainer.GetAllInstances<IApplicationEventHandler>(), x =>
            {
                try
                {
                    using (ProfilingLogger.DebugDuration<CoreRuntime>(
                        $"Executing {x.GetType()} in ApplicationInitialized",
                        $"Executed {x.GetType()} in ApplicationInitialized",
                        //only log if more than 150ms
                        150))
                    {
                        x.OnApplicationInitialized(UmbracoApplication);
                    }
                }
                catch (Exception ex)
                {
                    ProfilingLogger.Logger.Error<CoreRuntime>("An error occurred running OnApplicationInitialized for handler " + x.GetType(), ex);
                    throw;
                }
            });

            _isInitialized = true;

            return this;
        }


        /// <summary>
        /// Fires after initialization and calls the callback to allow for customizations to occur &
        /// Ensure that the OnApplicationStarting methods of the IApplicationEvents are called
        /// </summary>
        /// <param name="afterStartup"></param>
        /// <returns></returns>
        public virtual IRuntime Startup(Action afterStartup)
        {
            if (_isStarted)
                throw new InvalidOperationException("The boot manager has already been initialized");

            //call OnApplicationStarting of each application events handler
            Parallel.ForEach(_appStartupEvtContainer.GetAllInstances<IApplicationEventHandler>(), x =>
            {
                try
                {
                    using (ProfilingLogger.DebugDuration<CoreRuntime>(
                        $"Executing {x.GetType()} in ApplicationStarting",
                        $"Executed {x.GetType()} in ApplicationStarting",
                        //only log if more than 150ms
                        150))
                    {
                        x.OnApplicationStarting(UmbracoApplication);
                    }
                }
                catch (Exception ex)
                {
                    ProfilingLogger.Logger.Error<CoreRuntime>("An error occurred running OnApplicationStarting for handler " + x.GetType(), ex);
                    throw;
                }
            });

            if (afterStartup != null)
            {
                afterStartup();
            }

            _isStarted = true;

            return this;
        }

        /// <summary>
        /// Fires after startup and calls the callback once customizations are locked
        /// </summary>
        /// <param name="afterComplete"></param>
        /// <returns></returns>
        public virtual IRuntime Complete(Action afterComplete)
        {
            if (_isComplete)
                throw new InvalidOperationException("The boot manager has already been completed");

            //This is a special case for the user service, we need to tell it if it's an upgrade, if so we need to ensure that
            // exceptions are bubbled up if a user is attempted to be persisted during an upgrade (i.e. when they auth to login)
            // fixme - wtf? never going back to FALSE !
            ((UserService) Current.Services.UserService).IsUpgrading = true;

            //call OnApplicationStarting of each application events handler
            Parallel.ForEach(_appStartupEvtContainer.GetAllInstances<IApplicationEventHandler>(), x =>
            {
                try
                {
                    using (ProfilingLogger.DebugDuration<CoreRuntime>(
                        $"Executing {x.GetType()} in ApplicationStarted",
                        $"Executed {x.GetType()} in ApplicationStarted",
                        //only log if more than 150ms
                        150))
                    {
                        x.OnApplicationStarted(UmbracoApplication);
                    }
                }
                catch (Exception ex)
                {
                    ProfilingLogger.Logger.Error<CoreRuntime>("An error occurred running OnApplicationStarted for handler " + x.GetType(), ex);
                    throw;
                }
            });

            //end the current scope which was created to intantiate all of the startup handlers,
            //this will dispose them if they're IDisposable
            _appStartupEvtContainer.EndCurrentScope();
            //NOTE: DO NOT Dispose this cloned container since it will also dispose of any instances
            // resolved from the parent container
            _appStartupEvtContainer = null;

            if (afterComplete != null)
            {
                afterComplete();
            }

            _isComplete = true;

            return this;
		}
    }
}
