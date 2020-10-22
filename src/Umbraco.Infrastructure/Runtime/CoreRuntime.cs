using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Infrastructure.Composing;

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
        // runtime state, this instance will get replaced again once the essential services are available to run the check
        private RuntimeState _state = RuntimeState.Booting();
        private readonly IUmbracoBootPermissionChecker _umbracoBootPermissionChecker;
        private readonly GlobalSettings _globalSettings;
        private readonly ConnectionStrings _connectionStrings;

        public CoreRuntime(
            GlobalSettings globalSettings,
            ConnectionStrings connectionStrings,
            IUmbracoVersion umbracoVersion,
            IIOHelper ioHelper,
            ILoggerFactory loggerFactory,
            IProfiler profiler,
            IUmbracoBootPermissionChecker umbracoBootPermissionChecker,
            IHostingEnvironment hostingEnvironment,
            IBackOfficeInfo backOfficeInfo,
            IDbProviderFactoryCreator dbProviderFactoryCreator,
            IMainDom mainDom,
            ITypeFinder typeFinder,
            AppCaches appCaches)
        {
            _globalSettings = globalSettings;
            _connectionStrings = connectionStrings;

            IOHelper = ioHelper;
            AppCaches = appCaches;
            UmbracoVersion = umbracoVersion;
            Profiler = profiler;
            HostingEnvironment = hostingEnvironment;
            BackOfficeInfo = backOfficeInfo;
            DbProviderFactoryCreator = dbProviderFactoryCreator;

            RuntimeLoggerFactory = loggerFactory;
            _umbracoBootPermissionChecker = umbracoBootPermissionChecker;

            Logger = loggerFactory.CreateLogger<CoreRuntime>();
            MainDom = mainDom;
            TypeFinder = typeFinder;

            _globalSettings = globalSettings;
            _connectionStrings = connectionStrings;

        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger<CoreRuntime> Logger { get; }

        public ILoggerFactory RuntimeLoggerFactory { get; }

        protected IBackOfficeInfo BackOfficeInfo { get; }

        public IDbProviderFactoryCreator DbProviderFactoryCreator { get; }

        /// <summary>
        /// Gets the profiler.
        /// </summary>
        protected IProfiler Profiler { get; }

        /// <summary>
        /// Gets the profiling logger.
        /// </summary>
        public IProfilingLogger ProfilingLogger { get; private set; }

        /// <summary>
        /// Gets the <see cref="ITypeFinder"/>
        /// </summary>
        protected ITypeFinder TypeFinder { get; }

        /// <summary>
        /// Gets the <see cref="IIOHelper"/>
        /// </summary>
        protected IIOHelper IOHelper { get; }

        protected IHostingEnvironment HostingEnvironment { get; }
        public AppCaches AppCaches { get; }
        public IUmbracoVersion UmbracoVersion { get; }

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

                Logger.LogInformation("Booting site '{HostingSiteName}', app '{HostingApplicationId}', path '{HostingPhysicalPath}', server '{MachineName}'.",
                    HostingEnvironment?.SiteName,
                    HostingEnvironment?.ApplicationId,
                    HostingEnvironment?.ApplicationPhysicalPath,
                    NetworkHelper.MachineName);
                Logger.LogDebug("Runtime: {Runtime}", GetType().FullName);

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
                OnRuntimeBoot();

                // database factory
                var databaseFactory = CreateDatabaseFactory();

                // type finder/loader
                var typeLoader = new TypeLoader(TypeFinder, AppCaches.RuntimeCache, new DirectoryInfo(HostingEnvironment.LocalTempPath), RuntimeLoggerFactory.CreateLogger<TypeLoader>(), ProfilingLogger);

                // re-create the state object with the essential services
                _state = new RuntimeState(_globalSettings, UmbracoVersion, databaseFactory, RuntimeLoggerFactory.CreateLogger<RuntimeState>());

                // create the composition
                composition = new Composition(register, typeLoader, ProfilingLogger, _state, IOHelper, AppCaches);

                composition.RegisterEssentials(Logger, RuntimeLoggerFactory, Profiler, ProfilingLogger, MainDom, AppCaches, databaseFactory, typeLoader, _state, TypeFinder, IOHelper, UmbracoVersion, DbProviderFactoryCreator, HostingEnvironment, BackOfficeInfo);

                // register ourselves (TODO: Should we put this in RegisterEssentials?)
                composition.Register<IRuntime>(_ => this, Lifetime.Singleton);

                // run handlers
                OnRuntimeEssentials(composition, AppCaches, typeLoader, databaseFactory);

                try
                {
                    // determine our runtime level
                    DetermineRuntimeLevel(databaseFactory);
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
                Logger.LogError(exception, msg);
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

            var composers = new Composers(composition, composerTypes, enableDisableAttributes, RuntimeLoggerFactory.CreateLogger<Composers>(), ProfilingLogger);
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

        private void DetermineRuntimeLevel(IUmbracoDatabaseFactory databaseFactory)
        {
            using var timer = ProfilingLogger.DebugDuration<CoreRuntime>("Determining runtime level.", "Determined.");

            try
            {
                _state.DetermineRuntimeLevel();

                Logger.LogDebug("Runtime level: {RuntimeLevel} - {RuntimeLevelReason}", _state.Level, _state.Reason);

                if (_state.Level == RuntimeLevel.Upgrade)
                {
                    Logger.LogDebug("Configure database factory for upgrades.");
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

        public void ReplaceFactory(IServiceProvider serviceProvider)
        {
            _factory = ServiceProviderFactoryAdapter.Wrap(serviceProvider);
        }

        #region Getters

        // getters can be implemented by runtimes inheriting from CoreRuntime

        /// <summary>
        /// Gets all composer types.
        /// </summary>
        protected virtual IEnumerable<Type> GetComposerTypes(TypeLoader typeLoader)
            => typeLoader.GetTypes<IComposer>();

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
        /// Creates the database factory.
        /// </summary>
        /// <remarks>This is strictly internal, for tests only.</remarks>
        protected internal virtual IUmbracoDatabaseFactory CreateDatabaseFactory()
            => new UmbracoDatabaseFactory(RuntimeLoggerFactory.CreateLogger<UmbracoDatabaseFactory>(), RuntimeLoggerFactory, Options.Create(_globalSettings), Options.Create(_connectionStrings), new Lazy<IMapperCollection>(() => _factory.GetInstance<IMapperCollection>()), DbProviderFactoryCreator);


        #endregion

        #region Events

        protected void OnRuntimeBoot()
        {
            RuntimeOptions.DoRuntimeBoot(ProfilingLogger);
            RuntimeBooting?.Invoke(this, ProfilingLogger);
        }

        protected void OnRuntimeEssentials(Composition composition, AppCaches appCaches, TypeLoader typeLoader, IUmbracoDatabaseFactory databaseFactory)
        {
            RuntimeOptions.DoRuntimeEssentials(composition, appCaches, typeLoader, databaseFactory);
            RuntimeEssentials?.Invoke(this, new RuntimeEssentialsEventArgs(composition, appCaches, typeLoader, databaseFactory));
        }

        public event TypedEventHandler<CoreRuntime, IProfilingLogger> RuntimeBooting;
        public event TypedEventHandler<CoreRuntime, RuntimeEssentialsEventArgs> RuntimeEssentials;

        #endregion

    }
}
