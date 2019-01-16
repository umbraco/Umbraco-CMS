using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
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
        private ComponentCollection _components;
        private IFactory _factory;
        private RuntimeState _state;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the profiler.
        /// </summary>
        protected IProfiler Profiler { get; private set; }

        /// <summary>
        /// Gets the profiling logger.
        /// </summary>
        protected IProfilingLogger ProfilingLogger { get; private set; }

        /// <inheritdoc />
        public IRuntimeState State => _state;

        /// <inheritdoc/>
        public virtual IFactory Boot(IRegister register)
        {
            // create and register the essential services
            // ie the bare minimum required to boot

            // loggers
            var logger = Logger = GetLogger();
            var profiler = Profiler = GetProfiler();
            var profilingLogger = ProfilingLogger = new ProfilingLogger(logger, profiler);

            // the boot loader boots using a container scope, so anything that is PerScope will
            // be disposed after the boot loader has booted, and anything else will remain.
            // note that this REQUIRES that perWebRequestScope has NOT been enabled yet, else
            // the container will fail to create a scope since there is no http context when
            // the application starts.
            // the boot loader is kept in the runtime for as long as Umbraco runs, and components
            // are NOT disposed - which is not a big deal as long as they remain lightweight
            // objects.

            using (var timer = profilingLogger.TraceDuration<CoreRuntime>(
                $"Booting Umbraco {UmbracoVersion.SemanticVersion.ToSemanticString()} on {NetworkHelper.MachineName}.",
                "Booted.",
                "Boot failed."))
            {
                logger.Debug<CoreRuntime>("Runtime: {Runtime}", GetType().FullName);

                // application environment
                ConfigureUnhandledException();
                ConfigureAssemblyResolve();
                ConfigureApplicationRootPath();

                Boot(register, timer);
            }

            return _factory;
        }

        /// <summary>
        /// Boots the runtime within a timer.
        /// </summary>
        protected virtual IFactory Boot(IRegister register, DisposableTimer timer)
        {
            Composition composition = null;

            try
            {
                // throws if not full-trust
                new AspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted).Demand();

                // application caches
                var appCaches = GetAppCaches();
                var runtimeCache = appCaches.RuntimeCache;

                // database factory
                var databaseFactory = GetDatabaseFactory();

                // configs
                var configs = GetConfigs();

                // type loader
                var localTempStorage = configs.Global().LocalTempStorageLocation;
                var typeLoader = new TypeLoader(runtimeCache, localTempStorage, ProfilingLogger);

                // runtime state
                // beware! must use '() => _factory.GetInstance<T>()' and NOT '_factory.GetInstance<T>'
                // as the second one captures the current value (null) and therefore fails
                _state = new RuntimeState(Logger,
                    configs.Settings(), configs.Global(),
                    new Lazy<IMainDom>(() => _factory.GetInstance<IMainDom>()),
                    new Lazy<IServerRegistrar>(() => _factory.GetInstance<IServerRegistrar>()))
                {
                    Level = RuntimeLevel.Boot
                };

                // main dom
                var mainDom = new MainDom(Logger);

                // create the composition
                composition = new Composition(register, typeLoader, ProfilingLogger, _state, configs);
                composition.RegisterEssentials(Logger, Profiler, ProfilingLogger, mainDom, appCaches, databaseFactory, typeLoader, _state);

                // register runtime-level services
                // there should be none, really - this is here "just in case"
                Compose(composition);

                // acquire the main domain, determine our runtime level
                AcquireMainDom(mainDom);
                DetermineRuntimeLevel(databaseFactory, ProfilingLogger);

                // get composers, and compose
                var composerTypes = ResolveComposerTypes(typeLoader);
                composition.WithCollectionBuilder<ComponentCollectionBuilder>();
                var composers = new Composers(composition, composerTypes, ProfilingLogger);
                composers.Compose();

                // create the factory
                _factory = Current.Factory = composition.CreateFactory();

                // create & initialize the components
                _components = _factory.GetInstance<ComponentCollection>();
                _components.Initialize();
            }
            catch (Exception e)
            {
                var bfe = e as BootFailedException ?? new BootFailedException("Boot failed.", e);

                if (_state != null)
                {
                    _state.Level = RuntimeLevel.BootFailed;
                    _state.BootFailedException = bfe;
                }

                timer.Fail(exception: bfe); // be sure to log the exception - even if we repeat ourselves

                // if something goes wrong above, we may end up with no factory
                // meaning nothing can get the runtime state, etc - so let's try
                // to make sure we have a factory
                if (_factory == null)
                {
                    try
                    {
                        _factory = Current.Factory = composition?.CreateFactory();
                    }
                    catch { /* yea */ }
                }

                Debugger.Break();

                // throwing here can cause w3wp to hard-crash and we want to avoid it.
                // instead, we're logging the exception and setting level to BootFailed.
                // various parts of Umbraco such as UmbracoModule and UmbracoDefaultOwinStartup
                // understand this and will nullify themselves, while UmbracoModule will
                // throw a BootFailedException for every requests.
            }

            return _factory;
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

        private void AcquireMainDom(MainDom mainDom)
        {
            using (var timer = ProfilingLogger.DebugDuration<CoreRuntime>("Acquiring MainDom.", "Acquired."))
            {
                try
                {
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
        internal void DetermineRuntimeLevel(IUmbracoDatabaseFactory databaseFactory, IProfilingLogger profilingLogger)
        {
            using (var timer = profilingLogger.DebugDuration<CoreRuntime>("Determining runtime level.", "Determined."))
            {
                try
                {
                    _state.DetermineRuntimeLevel(databaseFactory, profilingLogger);

                    profilingLogger.Debug<CoreRuntime>("Runtime level: {RuntimeLevel}", _state.Level);

                    if (_state.Level == RuntimeLevel.Upgrade)
                    {
                        profilingLogger.Debug<CoreRuntime>("Configure database factory for upgrades.");
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

        private IEnumerable<Type> ResolveComposerTypes(TypeLoader typeLoader)
        {
            using (var timer = ProfilingLogger.TraceDuration<CoreRuntime>("Resolving composer types.", "Resolved."))
            {
                try
                {
                    return GetComposerTypes(typeLoader);
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
            _components.Terminate();
        }

        /// <summary>
        /// Composes the runtime.
        /// </summary>
        public virtual void Compose(Composition composition)
        {
            // nothing
        }

        #region Getters

        // getters can be implemented by runtimes inheriting from CoreRuntime

        /// <summary>
        /// Gets all composer types.
        /// </summary>
        protected virtual IEnumerable<Type> GetComposerTypes(TypeLoader typeLoader)
            => typeLoader.GetTypes<IComposer>();

        /// <summary>
        /// Gets a logger.
        /// </summary>
        protected virtual ILogger GetLogger()
            => SerilogLogger.CreateWithDefaultConfiguration();

        /// <summary>
        /// Gets a profiler.
        /// </summary>
        protected virtual IProfiler GetProfiler()
            => new LogProfiler(Logger);

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

        /// <summary>
        /// Gets the database factory.
        /// </summary>
        /// <remarks>This is strictly internal, for tests only.</remarks>
        protected internal virtual IUmbracoDatabaseFactory GetDatabaseFactory()
            => new UmbracoDatabaseFactory(Logger, new Lazy<IMapperCollection>(() => _factory.GetInstance<IMapperCollection>()));

        /// <summary>
        /// Gets the configurations.
        /// </summary>
        protected virtual Configs GetConfigs()
        {
            var configs = new Configs();
            configs.AddCoreConfigs();
            return configs;
        }

        #endregion
    }
}
