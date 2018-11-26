using System;
using System.Collections.Generic;
using System.Reflection;
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
using Umbraco.Core.Logging.Serilog;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Sync;

namespace Umbraco.Core.Runtime
{
    /// <summary>
    /// Represents the Core Umbraco runtime.
    /// </summary>
    /// <remarks>Does not handle any of the web-related aspects of Umbraco (startup, etc). It
    /// should be possible to use this runtime in console apps.</remarks>
    public class CoreRuntime : IRuntime
    {
        private Components.Components _components;
        private RuntimeState _state;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the profiling logger.
        /// </summary>
        protected IProfilingLogger ProfilingLogger { get; private set; }

        /// <inheritdoc/>
        public virtual void Boot(IContainer container)
        {
            // assign current container
            Current.Container = container;

            // create and register the essential services
            // ie the bare minimum required to boot

            var composition = new Composition(container, RuntimeLevel.Boot);

            // loggers
            var logger = GetLogger();
            container.RegisterInstance(logger);
            Logger = logger;
            var profiler = GetProfiler();
            container.RegisterInstance(profiler);
            var profilingLogger = new ProfilingLogger(logger, profiler);
            container.RegisterInstance<IProfilingLogger>(profilingLogger);
            ProfilingLogger = profilingLogger;

            // application environment
            ConfigureUnhandledException();
            ConfigureAssemblyResolve();
            ConfigureApplicationRootPath();

            // application caches
            var appCaches = GetAppCaches();
            container.RegisterInstance(appCaches);
            var runtimeCache = appCaches.RuntimeCache;
            container.RegisterInstance(runtimeCache);

            // database factory
            var databaseFactory = new UmbracoDatabaseFactory(logger, new Lazy<IMapperCollection>(container.GetInstance<IMapperCollection>));
            container.RegisterSingleton(factory => factory.GetInstance<IUmbracoDatabaseFactory>().SqlContext);

            // type loader
            var globalSettings = UmbracoConfig.For.GlobalSettings();
            var typeLoader = new TypeLoader(runtimeCache, globalSettings, profilingLogger);
            container.RegisterInstance(typeLoader);

            // runtime state
            _state = new RuntimeState(logger,
                UmbracoConfig.For.UmbracoSettings(), UmbracoConfig.For.GlobalSettings(),
                new Lazy<MainDom>(container.GetInstance<MainDom>),
                new Lazy<IServerRegistrar>(container.GetInstance<IServerRegistrar>))
            {
                Level = RuntimeLevel.Boot
            };
            container.RegisterInstance(_state);

            Compose(composition);

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
                try
                {
                    // throws if not full-trust
                    new AspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted).Demand();

                    logger.Debug<CoreRuntime>("Runtime: {Runtime}", GetType().FullName);

                    var mainDom = AquireMainDom();
                    container.RegisterInstance(mainDom);

                    DetermineRuntimeLevel(databaseFactory);

                    var componentTypes = ResolveComponentTypes(typeLoader);
                    _components = new Components.Components(composition, componentTypes, profilingLogger);

                    _components.Compose();

                    // no Current.Container only Current.Factory?
                    //factory = register.Compile();

                    // fixme at that point we can start actually getting things from the container
                    // but, ideally, not before = need to detect everything we use!!

                    // at that point, getting things from the container is ok
                    // fixme split IRegistry vs IFactory

                    _components.Initialize();
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
        }

        protected virtual void ConfigureUnhandledException()
        {
            //take care of unhandled exceptions - there is nothing we can do to
            // prevent the launch process to go down but at least we can try
            // and log the exception
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var exception = (Exception)args.ExceptionObject;
                var isTerminating = args.IsTerminating; // always true?

                var msg = "Unhandled exception in AppDomain";
                if (isTerminating) msg += " (terminating)";
                msg += ".";
                Logger.Error<CoreRuntime>(exception, msg);
            };
        }

        protected virtual void ConfigureAssemblyResolve()
        {
            // When an assembly can't be resolved. In here we can do magic with the assembly name and try loading another.
            // This is used for loading a signed assembly of AutoMapper (v. 3.1+) without having to recompile old code.
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                // ensure the assembly is indeed AutoMapper and that the PublicKeyToken is null before trying to Load again
                // do NOT just replace this with 'return Assembly', as it will cause an infinite loop -> stack overflow
                if (args.Name.StartsWith("AutoMapper") && args.Name.EndsWith("PublicKeyToken=null"))
                    return Assembly.Load(args.Name.Replace(", PublicKeyToken=null", ", PublicKeyToken=be96cd2c38ef1005"));
                return null;
            };
        }

        protected virtual void ConfigureApplicationRootPath()
        {
            var path = GetApplicationRootPath();
            if (string.IsNullOrWhiteSpace(path) == false)
                IOHelper.SetRootDirectory(path);
        }

        private MainDom AquireMainDom()
        {
            using (var timer = ProfilingLogger.DebugDuration<CoreRuntime>("Acquiring MainDom.", "Acquired."))
            {
                try
                {
                    var mainDom = new MainDom(Logger);
                    mainDom.Acquire();
                    return mainDom;
                }
                catch
                {
                    timer.Fail();
                    throw;
                }
            }
        }

        // internal for tests
        internal void DetermineRuntimeLevel(IUmbracoDatabaseFactory databaseFactory)
        {
            using (var timer = ProfilingLogger.DebugDuration<CoreRuntime>("Determining runtime level.", "Determined."))
            {
                try
                {
                    _state.Level = DetermineRuntimeLevel2(databaseFactory);

                    ProfilingLogger.Debug<CoreRuntime>("Runtime level: {RuntimeLevel}", _state.Level);

                    if (_state.Level == RuntimeLevel.Upgrade)
                    {
                        ProfilingLogger.Debug<CoreRuntime>("Configure database factory for upgrades.");
                        databaseFactory.ConfigureForUpgrade();
                    }
                }
                catch
                {
                    _state.Level = RuntimeLevel.BootFailed;
                    timer.Fail();
                    throw;
                }
            }
        }

        private IEnumerable<Type> ResolveComponentTypes(TypeLoader typeLoader)
        {
            using (var timer = ProfilingLogger.TraceDuration<CoreRuntime>("Resolving component types.", "Resolved."))
            {
                try
                {
                    return GetComponentTypes(typeLoader);
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
                _components?.Terminate();
            }
        }

        /// <summary>
        /// Composes the runtime.
        /// </summary>
        public virtual void Compose(Composition composition)
        {
            var container = composition.Container;

            // compose the very essential things that are needed to bootstrap, before anything else,
            // and only these things - the rest should be composed in runtime components
            // FIXME should be essentially empty! move all to component!

            container.ComposeConfiguration();

            // register persistence mappers - required by database factory so needs to be done here
            // means the only place the collection can be modified is in a runtime - afterwards it
            // has been frozen and it is too late
            composition.GetCollectionBuilder<MapperCollectionBuilder>().AddCoreMappers();

            // register the scope provider
            container.RegisterSingleton<ScopeProvider>(); // implements both IScopeProvider and IScopeAccessor
            container.RegisterSingleton<IScopeProvider>(f => f.GetInstance<ScopeProvider>());
            container.RegisterSingleton<IScopeAccessor>(f => f.GetInstance<ScopeProvider>());
        }

        private RuntimeLevel DetermineRuntimeLevel2(IUmbracoDatabaseFactory databaseFactory)
        {
            var localVersion = UmbracoVersion.LocalVersion; // the local, files, version
            var codeVersion = _state.SemanticVersion; // the executing code version
            var connect = false;

            if (localVersion == null)
            {
                // there is no local version, we are not installed
                Logger.Debug<CoreRuntime>("No local version, need to install Umbraco.");
                return RuntimeLevel.Install;
            }

            if (localVersion < codeVersion)
            {
                // there *is* a local version, but it does not match the code version
                // need to upgrade
                Logger.Debug<CoreRuntime>("Local version '{LocalVersion}' < code version '{CodeVersion}', need to upgrade Umbraco.", localVersion, codeVersion);
            }
            else if (localVersion > codeVersion)
            {
                Logger.Warn<CoreRuntime>("Local version '{LocalVersion}' > code version '{CodeVersion}', downgrading is not supported.", localVersion, codeVersion);

                // in fact, this is bad enough that we want to throw
                throw new BootFailedException($"Local version \"{localVersion}\" > code version \"{codeVersion}\", downgrading is not supported.");
            }
            else if (databaseFactory.Configured == false)
            {
                // local version *does* match code version, but the database is not configured
                // install (again? this is a weird situation...)
                Logger.Debug<CoreRuntime>("Database is not configured, need to install Umbraco.");
                return RuntimeLevel.Install;
            }

            // else, keep going,
            // anything other than install wants a database - see if we can connect
            // (since this is an already existing database, assume localdb is ready)
            for (var i = 0; i < 5; i++)
            {
                connect = databaseFactory.CanConnect;
                if (connect) break;
                Logger.Debug<CoreRuntime>("Could not immediately connect to database, trying again.");
                Thread.Sleep(1000);
            }

            if (connect == false)
            {
                // cannot connect to configured database, this is bad, fail
                Logger.Debug<CoreRuntime>("Could not connect to database.");

                // in fact, this is bad enough that we want to throw
                throw new BootFailedException("A connection string is configured but Umbraco could not connect to the database.");
            }

            // if we already know we want to upgrade,
            // still run EnsureUmbracoUpgradeState to get the states
            // (v7 will just get a null state, that's ok)

            // else
            // look for a matching migration entry - bypassing services entirely - they are not 'up' yet
            // fixme - in a LB scenario, ensure that the DB gets upgraded only once!
            bool noUpgrade;
            try
            {
                noUpgrade = EnsureUmbracoUpgradeState(databaseFactory);
            }
            catch (Exception e)
            {
                // can connect to the database but cannot check the upgrade state... oops
                Logger.Warn<CoreRuntime>(e, "Could not check the upgrade state.");
                throw new BootFailedException("Could not check the upgrade state.", e);
            }

            if (noUpgrade)
            {
                // the database version matches the code & files version, all clear, can run
                return RuntimeLevel.Run;
            }

            // the db version does not match... but we do have a migration table
            // so, at least one valid table, so we quite probably are installed & need to upgrade

            // although the files version matches the code version, the database version does not
            // which means the local files have been upgraded but not the database - need to upgrade
            Logger.Debug<CoreRuntime>("Has not reached the final upgrade step, need to upgrade Umbraco.");
            return RuntimeLevel.Upgrade;
        }

        protected virtual bool EnsureUmbracoUpgradeState(IUmbracoDatabaseFactory databaseFactory)
        {
            var umbracoPlan = new UmbracoPlan();
            var stateValueKey = Upgrader.GetStateValueKey(umbracoPlan);

            // no scope, no service - just directly accessing the database
            using (var database = databaseFactory.CreateDatabase())
            {
                _state.CurrentMigrationState = KeyValueService.GetValue(database, stateValueKey);
                _state.FinalMigrationState = umbracoPlan.FinalState;
            }

            Logger.Debug<CoreRuntime>("Final upgrade state is {FinalMigrationState}, database contains {DatabaseState}", _state.FinalMigrationState, _state.CurrentMigrationState ?? "<null>");

            return _state.CurrentMigrationState == _state.FinalMigrationState;
        }

        #region Getters

        // getters can be implemented by runtimes inheriting from CoreRuntime

        /// <summary>
        /// Gets all component types.
        /// </summary>
        protected virtual IEnumerable<Type> GetComponentTypes(TypeLoader typeLoader)
            => typeLoader.GetTypes<IUmbracoComponent>();

        /// <summary>
        /// Gets a logger.
        /// </summary>
        protected virtual ILogger GetLogger()
            => SerilogLogger.CreateWithDefaultConfiguration();

        /// <summary>
        /// Gets a profiler.
        /// </summary>
        protected virtual IProfiler GetProfiler()
            => new LogProfiler(ProfilingLogger);

        /// <summary>
        /// Gets the application caches.
        /// </summary>
        protected virtual CacheHelper GetAppCaches()
        {
            // need the deep clone runtime cache provider to ensure entities are cached properly, ie
            // are cloned in and cloned out - no request-based cache here since no web-based context,
            // is overriden by the web runtime

            return new CacheHelper(
                new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider()),
                new StaticCacheProvider(),
                NullCacheProvider.Instance,
                new IsolatedRuntimeCache(type => new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider())));
        }

        // by default, returns null, meaning that Umbraco should auto-detect the application root path.
        // override and return the absolute path to the Umbraco site/solution, if needed
        protected virtual string GetApplicationRootPath()
            => null;

        #endregion
    }
}
