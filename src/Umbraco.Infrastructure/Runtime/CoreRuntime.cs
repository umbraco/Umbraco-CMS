using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;

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
        private readonly RuntimeState _state;
        private readonly IUmbracoBootPermissionChecker _umbracoBootPermissionChecker;
        private readonly IRequestCache _requestCache;
        private readonly IGlobalSettings _globalSettings;
        private readonly IConnectionStrings _connectionStrings;

        public CoreRuntime(
            Configs configs,
            IUmbracoVersion umbracoVersion,
            IIOHelper ioHelper,
            ILogger logger,
            IProfiler profiler,
            IUmbracoBootPermissionChecker umbracoBootPermissionChecker,
            IHostingEnvironment hostingEnvironment,
            IBackOfficeInfo backOfficeInfo,
            IDbProviderFactoryCreator dbProviderFactoryCreator,
            IMainDom mainDom,
            ITypeFinder typeFinder,
            IRequestCache requestCache)
        {
            IOHelper = ioHelper;
            Configs = configs;
            UmbracoVersion = umbracoVersion ;
            Profiler = profiler;
            HostingEnvironment = hostingEnvironment;
            BackOfficeInfo = backOfficeInfo;
            DbProviderFactoryCreator = dbProviderFactoryCreator;

            _umbracoBootPermissionChecker = umbracoBootPermissionChecker;
            _requestCache = requestCache;

            Logger = logger;
            MainDom = mainDom;
            TypeFinder = typeFinder;

            _globalSettings = Configs.Global();
            _connectionStrings = configs.ConnectionStrings();


            // runtime state
            // beware! must use '() => _factory.GetInstance<T>()' and NOT '_factory.GetInstance<T>'
            // as the second one captures the current value (null) and therefore fails
           _state = new RuntimeState(Configs.Global(), UmbracoVersion)
            {
                Level = RuntimeLevel.Boot
            };
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; }

        protected IBackOfficeInfo BackOfficeInfo { get; }

        public IDbProviderFactoryCreator DbProviderFactoryCreator { get; }

        /// <summary>
        /// Gets the profiler.
        /// </summary>
        protected IProfiler Profiler { get; }

        /// <summary>
        /// Gets the profiling logger.
        /// </summary>
        protected IProfilingLogger ProfilingLogger { get; private set; }

        /// <summary>
        /// Gets the <see cref="ITypeFinder"/>
        /// </summary>
        protected ITypeFinder TypeFinder { get; }

        /// <summary>
        /// Gets the <see cref="IIOHelper"/>
        /// </summary>
        protected IIOHelper IOHelper { get; }
        protected IHostingEnvironment HostingEnvironment { get; }
        protected Configs Configs { get; }
        protected IUmbracoVersion UmbracoVersion { get; }

        /// <inheritdoc />
        public IRuntimeState State => _state;

        public IMainDom MainDom { get; }

        /// <inheritdoc/>
        public virtual IFactory Configure(IRegister register)
        {
            if (register is null) throw new ArgumentNullException(nameof(register));


            // create and register the essential services
            // ie the bare minimum required to boot

            // the boot loader boots using a container scope, so anything that is PerScope will
            // be disposed after the boot loader has booted, and anything else will remain.
            // note that this REQUIRES that perWebRequestScope has NOT been enabled yet, else
            // the container will fail to create a scope since there is no http context when
            // the application starts.
            // the boot loader is kept in the runtime for as long as Umbraco runs, and components
            // are NOT disposed - which is not a big deal as long as they remain lightweight
            // objects.

            var umbracoVersion = new UmbracoVersion();
            var profilingLogger = ProfilingLogger = new ProfilingLogger(Logger, Profiler);
            using (var timer = profilingLogger.TraceDuration<CoreRuntime>(
                $"Booting Umbraco {umbracoVersion.SemanticVersion.ToSemanticString()}.",
                "Booted.",
                "Boot failed."))
            {

                Logger.Info<CoreRuntime>("Booting site '{HostingSiteName}', app '{HostingApplicationId}', path '{HostingPhysicalPath}', server '{MachineName}'.",
                    HostingEnvironment?.SiteName,
                    HostingEnvironment?.ApplicationId,
                    HostingEnvironment?.ApplicationPhysicalPath,
                    NetworkHelper.MachineName);
                Logger.Debug<CoreRuntime>("Runtime: {Runtime}", GetType().FullName);

                AppDomain.CurrentDomain.SetData("DataDirectory", HostingEnvironment?.MapPathContentRoot(Constants.SystemDirectories.Data));

                // application environment
                ConfigureUnhandledException();
                _factory = Configure(register, timer);

                return _factory;
            }
        }

        /// <summary>
        /// Configure the runtime within a timer.
        /// </summary>
        private IFactory Configure(IRegister register, DisposableTimer timer)
        {
            if (register is null) throw new ArgumentNullException(nameof(register));
            if (timer is null) throw new ArgumentNullException(nameof(timer));

            Composition composition = null;
            IFactory factory = null;

            try
            {


                // run handlers
                RuntimeOptions.DoRuntimeBoot(ProfilingLogger);

                // application caches
                var appCaches = GetAppCaches();

                // database factory
                var databaseFactory = GetDatabaseFactory();

                // type finder/loader
                var typeLoader = new TypeLoader(TypeFinder, appCaches.RuntimeCache, new DirectoryInfo(HostingEnvironment.LocalTempPath), ProfilingLogger);

                // create the composition
                composition = new Composition(register, typeLoader, ProfilingLogger, _state, Configs, IOHelper, appCaches);
                composition.RegisterEssentials(Logger, Profiler, ProfilingLogger, MainDom, appCaches, databaseFactory, typeLoader, _state, TypeFinder, IOHelper, UmbracoVersion, DbProviderFactoryCreator, HostingEnvironment, BackOfficeInfo);

                // register ourselves (TODO: Should we put this in RegisterEssentials?)
                composition.Register<IRuntime>(_ => this, Lifetime.Singleton);

                try
                {
                    // determine our runtime level
                    DetermineRuntimeLevel(databaseFactory, ProfilingLogger);
                }
                finally
                {
                    // always run composers
                    RunComposers(typeLoader, composition);
                }

                 // create the factory
                 factory = composition.CreateFactory();
            }
            catch (Exception e)
            {
                var bfe = e as BootFailedException ?? new BootFailedException("Boot failed.", e);

                if (_state != null)
                {
                    _state.Level = RuntimeLevel.BootFailed;
                    _state.BootFailedException = bfe;
                }

                timer?.Fail(exception: bfe); // be sure to log the exception - even if we repeat ourselves

                // if something goes wrong above, we may end up with no factory
                // meaning nothing can get the runtime state, etc - so let's try
                // to make sure we have a factory
                if (factory == null)
                {
                    try
                    {
                        factory = composition?.CreateFactory();
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

            return factory;
        }

        public void Start()
        {
            if (_state.Level <= RuntimeLevel.BootFailed)
                throw new InvalidOperationException($"Cannot start the runtime if the runtime level is less than or equal to {RuntimeLevel.BootFailed}");

            // throws if not full-trust
            _umbracoBootPermissionChecker.ThrowIfNotPermissions();

            // run handlers
            RuntimeOptions.DoRuntimeEssentials(_factory);

            var hostingEnvironmentLifetime = _factory.TryGetInstance<IApplicationShutdownRegistry>();
            if (hostingEnvironmentLifetime == null)
                throw new InvalidOperationException($"An instance of {typeof(IApplicationShutdownRegistry)} could not be resolved from the container, ensure that one if registered in your runtime before calling {nameof(IRuntime)}.{nameof(Start)}");

            // acquire the main domain - if this fails then anything that should be registered with MainDom will not operate
            AcquireMainDom(MainDom, _factory.GetInstance<IApplicationShutdownRegistry>());

            // create & initialize the components
            _components = _factory.GetInstance<ComponentCollection>();
            _components.Initialize();


            // now (and only now) is the time to switch over to perWebRequest scopes.
            // up until that point we may not have a request, and scoped services would
            // fail to resolve - but we run Initialize within a factory scope - and then,
            // here, we switch the factory to bind scopes to requests
            _factory.EnablePerWebRequestScope();
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

        private void RunComposers(TypeLoader typeLoader, Composition composition)
        {
            // get composers, and compose
            var composerTypes = ResolveComposerTypes(typeLoader);

            IEnumerable<Attribute> enableDisableAttributes;
            using (ProfilingLogger.DebugDuration<CoreRuntime>("Scanning enable/disable composer attributes"))
            {
                enableDisableAttributes = typeLoader.GetAssemblyAttributes(typeof(EnableComposerAttribute), typeof(DisableComposerAttribute));
            }

            var composers = new Composers(composition, composerTypes, enableDisableAttributes, ProfilingLogger);
            composers.Compose();
        }

        private bool AcquireMainDom(IMainDom mainDom, IApplicationShutdownRegistry applicationShutdownRegistry)
        {
            using (var timer = ProfilingLogger.DebugDuration<CoreRuntime>("Acquiring MainDom.", "Acquired."))
            {
                try
                {
                    return mainDom.Acquire(applicationShutdownRegistry);
                }
                catch
                {
                    timer?.Fail();
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

                    profilingLogger.Debug<CoreRuntime>("Runtime level: {RuntimeLevel} - {RuntimeLevelReason}", _state.Level, _state.Reason);

                    if (_state.Level == RuntimeLevel.Upgrade)
                    {
                        profilingLogger.Debug<CoreRuntime>("Configure database factory for upgrades.");
                        databaseFactory.ConfigureForUpgrade();
                    }
                }
                catch
                {
                    _state.Level = RuntimeLevel.BootFailed;
                    _state.Reason = RuntimeLevelReason.BootFailedOnException;
                    timer?.Fail();
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
                    timer?.Fail();
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public virtual void Terminate()
        {
            _components?.Terminate();
        }


        #region Getters

        // getters can be implemented by runtimes inheriting from CoreRuntime

        /// <summary>
        /// Gets all composer types.
        /// </summary>
        protected virtual IEnumerable<Type> GetComposerTypes(TypeLoader typeLoader)
            => typeLoader.GetTypes<IComposer>();

        /// <summary>
        /// Gets the application caches.
        /// </summary>
        protected virtual AppCaches GetAppCaches()
        {
            // need the deep clone runtime cache provider to ensure entities are cached properly, ie
            // are cloned in and cloned out - no request-based cache here since no web-based context,
            // is overridden by the web runtime

            return new AppCaches(
                new DeepCloneAppCache(new ObjectCacheAppCache()),
                _requestCache,
                new IsolatedCaches(type => new DeepCloneAppCache(new ObjectCacheAppCache())));
        }

        /// <summary>
        /// Returns the application path of the site/solution
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// By default is null which means it's not running in any virtual folder. If the site is running in a virtual folder, this
        /// can be overridden and the virtual path returned (i.e. /mysite/)
        /// </remarks>
        protected virtual string GetApplicationRootPath()
            => null;

        /// <summary>
        /// Gets the database factory.
        /// </summary>
        /// <remarks>This is strictly internal, for tests only.</remarks>
        protected internal virtual IUmbracoDatabaseFactory GetDatabaseFactory()
            => new UmbracoDatabaseFactory(Logger, _globalSettings, _connectionStrings, new Lazy<IMapperCollection>(() => _factory.GetInstance<IMapperCollection>()), DbProviderFactoryCreator);


        #endregion

    }
}
