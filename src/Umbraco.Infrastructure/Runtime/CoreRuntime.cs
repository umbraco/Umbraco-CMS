﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
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
        // runtime state, this instance will get replaced again once the essential services are available to run the check
        private RuntimeState _state = RuntimeState.Booting();
        private readonly IUmbracoBootPermissionChecker _umbracoBootPermissionChecker;
        private readonly IGlobalSettings _globalSettings;
        private readonly IConnectionStrings _connectionStrings;

        /* TODO: MSDI This is hopefully a temporary measure
           CreateDatabaseFactory constructs a UmbracoDatabaseFactory providing a Lazy<IMapperCollection> that resolves from _serviceProvider.
         */
        private IServiceProvider _serviceProvider;

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
            AppCaches appCaches)
        {
            IOHelper = ioHelper;
            Configs = configs;
            AppCaches = appCaches;
            UmbracoVersion = umbracoVersion ;
            Profiler = profiler;
            HostingEnvironment = hostingEnvironment;
            BackOfficeInfo = backOfficeInfo;
            DbProviderFactoryCreator = dbProviderFactoryCreator;

            _umbracoBootPermissionChecker = umbracoBootPermissionChecker;
            
            Logger = logger;
            MainDom = mainDom;
            TypeFinder = typeFinder;

            _globalSettings = Configs.Global();
            _connectionStrings = configs.ConnectionStrings();

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
        public Configs Configs { get; }
        public AppCaches AppCaches { get; }
        public IUmbracoVersion UmbracoVersion { get; }

        /// <inheritdoc />
        public IRuntimeState State => _state;

        public IMainDom MainDom { get; }

        /// <inheritdoc/>
        public virtual void Configure(IServiceCollection services)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

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
                Configure(services, timer);
            }
        }

        /// <summary>
        /// Configure the runtime within a timer.
        /// </summary>
        private void Configure(IServiceCollection services, DisposableTimer timer)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (timer is null) throw new ArgumentNullException(nameof(timer));

            Composition composition = null;

            // TODO: MSDI Find a decent place for this, is it here?
            // NOTE: LightInject had this Lazy resolve setup by default
            // ServiceContext resolves a bunch of Lazy<T> which aren't registered
            services.AddTransient(typeof(Lazy<>), typeof(LazilyResolved<>));

            try
            {
                // run handlers
                OnRuntimeBoot();

                // database factory
                var databaseFactory = CreateDatabaseFactory();

                // type finder/loader
                var typeLoader = new TypeLoader(TypeFinder, AppCaches.RuntimeCache, new DirectoryInfo(HostingEnvironment.LocalTempPath), ProfilingLogger);

                // re-create the state object with the essential services
                _state = new RuntimeState(Configs.Global(), UmbracoVersion, databaseFactory, Logger);

                // create the composition
                composition = new Composition(services, typeLoader, ProfilingLogger, _state, Configs, IOHelper, AppCaches);

                composition.RegisterEssentials(Logger, Profiler, ProfilingLogger, MainDom, AppCaches, databaseFactory, typeLoader, _state, TypeFinder, IOHelper, UmbracoVersion, DbProviderFactoryCreator, HostingEnvironment, BackOfficeInfo);

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

                Debugger.Break();

                // throwing here can cause w3wp to hard-crash and we want to avoid it.
                // instead, we're logging the exception and setting level to BootFailed.
                // various parts of Umbraco such as UmbracoModule and UmbracoDefaultOwinStartup
                // understand this and will nullify themselves, while UmbracoModule will
                // throw a BootFailedException for every requests.
            }

            composition?.RegisterBuildersAndConfigs();
        }

        public void Start(IServiceProvider serviceProvider)
        {
            if (_state.Level <= RuntimeLevel.BootFailed)
                throw new InvalidOperationException($"Cannot start the runtime if the runtime level is less than or equal to {RuntimeLevel.BootFailed}");

            // throws if not full-trust
            _umbracoBootPermissionChecker.ThrowIfNotPermissions();

            var hostingEnvironmentLifetime = serviceProvider.TryGetInstance<IApplicationShutdownRegistry>();
            if (hostingEnvironmentLifetime == null)
                throw new InvalidOperationException($"An instance of {typeof(IApplicationShutdownRegistry)} could not be resolved from the container, ensure that one if registered in your runtime before calling {nameof(IRuntime)}.{nameof(Start)}");

            // acquire the main domain - if this fails then anything that should be registered with MainDom will not operate
            AcquireMainDom(MainDom, serviceProvider.GetInstance<IApplicationShutdownRegistry>());

            // create & initialize the components
            _components = serviceProvider.GetInstance<ComponentCollection>();
            _components.Initialize();

            // GROSS: See note on declaration of _serviceProvider above about CreateDatabaseFactory
            _serviceProvider = serviceProvider;

            // TODO: MSDI is this comment redundant now?
            // now (and only now) is the time to switch over to perWebRequest scopes.
            // up until that point we may not have a request, and scoped services would
            // fail to resolve - but we run Initialize within a factory scope - and then,
            // here, we switch the factory to bind scopes to requests
            //_serviceProvider.EnablePerWebRequestScope();
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

        private void DetermineRuntimeLevel(IUmbracoDatabaseFactory databaseFactory)
        {
            using var timer = ProfilingLogger.DebugDuration<CoreRuntime>("Determining runtime level.", "Determined.");

            try
            {
                _state.DetermineRuntimeLevel();

                ProfilingLogger.Debug<CoreRuntime>("Runtime level: {RuntimeLevel} - {RuntimeLevelReason}", _state.Level, _state.Reason);

                if (_state.Level == RuntimeLevel.Upgrade)
                {
                    ProfilingLogger.Debug<CoreRuntime>("Configure database factory for upgrades.");
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

        private class LazilyResolved<T> : Lazy<T>
        {
            public LazilyResolved(IServiceProvider serviceProvider)
                : base(serviceProvider.GetRequiredService<T>)
            {
            }
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
            => new UmbracoDatabaseFactory(Logger, _globalSettings, _connectionStrings, new Lazy<IMapperCollection>(() => _serviceProvider.GetInstance<IMapperCollection>()), DbProviderFactoryCreator);


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
