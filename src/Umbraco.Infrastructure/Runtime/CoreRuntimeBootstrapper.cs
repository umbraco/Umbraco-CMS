using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
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

namespace Umbraco.Core.Runtime
{
    /// <summary>
    /// Bootstraps the Core Umbraco runtime.
    /// </summary>
    /// <remarks>Does not handle any of the web-related aspects of Umbraco (startup, etc). It
    /// should be possible to use this runtime in console apps.</remarks>
    public class CoreRuntimeBootstrapper
    {
        // runtime state, this instance will get replaced again once the essential services are available to run the check
        private RuntimeState _state = RuntimeState.Booting();
        private readonly IUmbracoBootPermissionChecker _umbracoBootPermissionChecker;
        private readonly GlobalSettings _globalSettings;
        private readonly ConnectionStrings _connectionStrings;
        
        public CoreRuntimeBootstrapper(
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

            Logger = loggerFactory.CreateLogger<CoreRuntimeBootstrapper>();
            MainDom = mainDom;
            TypeFinder = typeFinder;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger<CoreRuntimeBootstrapper> Logger { get; }

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
            using (var timer = profilingLogger.TraceDuration<CoreRuntimeBootstrapper>(
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

            try
            {
                // run handlers
                OnRuntimeBoot();

                // type finder/loader
                var typeLoader = new TypeLoader(TypeFinder, AppCaches.RuntimeCache,
                    new DirectoryInfo(HostingEnvironment.LocalTempPath),
                    RuntimeLoggerFactory.CreateLogger<TypeLoader>(), ProfilingLogger);

                services.AddUnique<ILogger>(Logger);
                services.AddUnique<ILoggerFactory>(RuntimeLoggerFactory);
                services.AddUnique<IUmbracoBootPermissionChecker>(_umbracoBootPermissionChecker);
                services.AddUnique<IProfiler>(Profiler);
                services.AddUnique<IProfilingLogger>(ProfilingLogger);
                services.AddUnique<IMainDom>(MainDom);
                services.AddUnique<AppCaches>(AppCaches);
                services.AddUnique<IRequestCache>(AppCaches.RequestCache);
                services.AddUnique<TypeLoader>(typeLoader);
                services.AddUnique<ITypeFinder>(TypeFinder);
                services.AddUnique<IIOHelper>(IOHelper);
                services.AddUnique<IUmbracoVersion>(UmbracoVersion);
                services.AddUnique<IDbProviderFactoryCreator>(DbProviderFactoryCreator);
                services.AddUnique<IHostingEnvironment>(HostingEnvironment);
                services.AddUnique<IBackOfficeInfo>(BackOfficeInfo);
                services.AddUnique<IRuntime, CoreRuntime>();

                // NOTE: This instance of IUmbracoDatabaseFactory is only used to determine runtime state.
                var bootstrapDatabaseFactory = CreateBootstrapDatabaseFactory();

                // after bootstrapping we let the container wire up for us.
                services.AddUnique<IUmbracoDatabaseFactory, UmbracoDatabaseFactory>();
                services.AddUnique<ISqlContext>(factory => factory.GetRequiredService<IUmbracoDatabaseFactory>().SqlContext);
                services.AddUnique<IBulkSqlInsertProvider>(factory => factory.GetRequiredService<IUmbracoDatabaseFactory>().BulkSqlInsertProvider);

                // re-create the state object with the essential services
                _state = new RuntimeState(_globalSettings, UmbracoVersion, bootstrapDatabaseFactory, RuntimeLoggerFactory.CreateLogger<RuntimeState>());
                services.AddUnique<IRuntimeState>(_state);


                // create the composition
                composition = new Composition(services, typeLoader, ProfilingLogger, _state, IOHelper, AppCaches);

                // run handlers
                OnRuntimeEssentials(composition, AppCaches, typeLoader, bootstrapDatabaseFactory);

                try
                {
                    // determine our runtime level
                    DetermineRuntimeLevel(bootstrapDatabaseFactory);
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
            finally
            {
                composition?.RegisterBuilders();
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
                Logger.LogError(exception, msg);
            };
        }

        private void RunComposers(TypeLoader typeLoader, Composition composition)
        {
            // get composers, and compose
            var composerTypes = ResolveComposerTypes(typeLoader);

            IEnumerable<Attribute> enableDisableAttributes;
            using (ProfilingLogger.DebugDuration<CoreRuntimeBootstrapper>("Scanning enable/disable composer attributes"))
            {
                enableDisableAttributes = typeLoader.GetAssemblyAttributes(typeof(EnableComposerAttribute), typeof(DisableComposerAttribute));
            }

            var composers = new Composers(composition, composerTypes, enableDisableAttributes, RuntimeLoggerFactory.CreateLogger<Composers>(), ProfilingLogger);
            composers.Compose();
        }


        private void DetermineRuntimeLevel(IUmbracoDatabaseFactory databaseFactory)
        {
            using var timer = ProfilingLogger.DebugDuration<CoreRuntimeBootstrapper>("Determining runtime level.", "Determined.");

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
            using (var timer = ProfilingLogger.TraceDuration<CoreRuntimeBootstrapper>("Resolving composer types.", "Resolved."))
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
        protected internal virtual IUmbracoDatabaseFactory CreateBootstrapDatabaseFactory()
            => new UmbracoDatabaseFactory(
                RuntimeLoggerFactory.CreateLogger<UmbracoDatabaseFactory>(),
                RuntimeLoggerFactory,
                Options.Create(_globalSettings),
                Options.Create(_connectionStrings),
                new Lazy<IMapperCollection>(() => new MapperCollection(Enumerable.Empty<BaseMapper>())),
                DbProviderFactoryCreator);


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

        public event TypedEventHandler<CoreRuntimeBootstrapper, IProfilingLogger> RuntimeBooting;
        public event TypedEventHandler<CoreRuntimeBootstrapper, RuntimeEssentialsEventArgs> RuntimeEssentials;

        #endregion

    }
}
