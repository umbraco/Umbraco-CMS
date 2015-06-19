using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using AutoMapper;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixZeroOne;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Profiling;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Publishing;
using Umbraco.Core.Macros;
using Umbraco.Core.Manifest;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Core.Strings;
using MigrationsVersionFourNineZero = Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionFourNineZero;

namespace Umbraco.Core
{
    /// <summary>
    /// A bootstrapper for the Umbraco application which initializes all objects for the Core of the application 
    /// </summary>
    /// <remarks>
    /// This does not provide any startup functionality relating to web objects
    /// </remarks>
    public class CoreBootManager : IBootManager
    {
        private ProfilingLogger _profilingLogger;
        private DisposableTimer _timer;
        private bool _isInitialized = false;
        private bool _isStarted = false;
        private bool _isComplete = false;
        private readonly IServiceProvider _serviceProvider = new ActivatorServiceProvider();
        private readonly UmbracoApplicationBase _umbracoApplication;
        protected ApplicationContext ApplicationContext { get; set; }
        protected CacheHelper ApplicationCache { get; set; }
        protected PluginManager PluginManager { get; private set; }

        protected UmbracoApplicationBase UmbracoApplication
        {
            get { return _umbracoApplication; }
        }

        protected IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        public CoreBootManager(UmbracoApplicationBase umbracoApplication)
        {
            if (umbracoApplication == null) throw new ArgumentNullException("umbracoApplication");
            _umbracoApplication = umbracoApplication;
        }

        public virtual IBootManager Initialize()
        {
            if (_isInitialized)
                throw new InvalidOperationException("The boot manager has already been initialized");

            InitializeLoggerResolver();
            InitializeProfilerResolver();

            _profilingLogger = new ProfilingLogger(LoggerResolver.Current.Logger, ProfilerResolver.Current.Profiler);

            _timer = _profilingLogger.TraceDuration<CoreBootManager>("Umbraco application starting", "Umbraco application startup complete");

            CreateApplicationCache();

            //create and set the plugin manager (I'd much prefer to not use this singleton anymore but many things are using it unfortunately and
            // the way that it is setup, there must only ever be one per app so without IoC it would be hard to make this not a singleton)
            PluginManager = new PluginManager(ServiceProvider, ApplicationCache.RuntimeCache, _profilingLogger);
            PluginManager.Current = PluginManager;

            //Create the legacy prop-eds mapping
            LegacyPropertyEditorIdToAliasConverter.CreateMappingsForCoreEditors();
            LegacyParameterEditorAliasConverter.CreateMappingsForCoreEditors();

            //create database and service contexts for the app context
            var dbFactory = new DefaultDatabaseFactory(GlobalSettings.UmbracoConnectionName, LoggerResolver.Current.Logger);
            Database.Mapper = new PetaPocoMapper();

            var dbContext = new DatabaseContext(
                dbFactory,
                LoggerResolver.Current.Logger,
                SqlSyntaxProviders.CreateDefault(LoggerResolver.Current.Logger));

            //initialize the DatabaseContext
            dbContext.Initialize();
            
            var serviceContext = new ServiceContext(
                new RepositoryFactory(ApplicationCache, LoggerResolver.Current.Logger, dbContext.SqlSyntax, UmbracoConfig.For.UmbracoSettings()), 
                new PetaPocoUnitOfWorkProvider(dbFactory),
                new FileUnitOfWorkProvider(),
                new PublishingStrategy(),
                ApplicationCache,
                LoggerResolver.Current.Logger);

            CreateApplicationContext(dbContext, serviceContext);

            InitializeApplicationEventsResolver();

            InitializeResolvers();

            

            InitializeModelMappers();

            //now we need to call the initialize methods
            ApplicationEventsResolver.Current.ApplicationEventHandlers
                .ForEach(x => x.OnApplicationInitialized(UmbracoApplication, ApplicationContext));

            _isInitialized = true;

            return this;
        }

        /// <summary>
        /// Creates and assigns the application context singleton
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="serviceContext"></param>
        protected virtual void CreateApplicationContext(DatabaseContext dbContext, ServiceContext serviceContext)
        {
            //create the ApplicationContext
            ApplicationContext = ApplicationContext.Current = new ApplicationContext(dbContext, serviceContext, ApplicationCache, _profilingLogger);
        }

        /// <summary>
        /// Creates and assigns the ApplicationCache based on a new instance of System.Web.Caching.Cache
        /// </summary>
        protected virtual void CreateApplicationCache()
        {
            var cacheHelper = new CacheHelper(
                        new ObjectCacheRuntimeCacheProvider(),
                        new StaticCacheProvider(),
                //we have no request based cache when not running in web-based context
                        new NullCacheProvider());

            ApplicationCache = cacheHelper;
        }

        /// <summary>
        /// This method allows for configuration of model mappers
        /// </summary>
        /// <remarks>
        /// Model mappers MUST be defined on ApplicationEventHandler instances with the interface IMapperConfiguration.
        /// This allows us to search for less types on startup.
        /// </remarks>
        protected void InitializeModelMappers()
        {
            Mapper.Initialize(configuration =>
                {
                    foreach (var m in ApplicationEventsResolver.Current.ApplicationEventHandlers.OfType<IMapperConfiguration>())
                    {
                        m.ConfigureMappings(configuration, ApplicationContext);
                    }
                });
        }

        /// <summary>
        /// Special method to initialize the LoggerResolver
        /// </summary>
        protected virtual void InitializeLoggerResolver()
        {
            LoggerResolver.Current = new LoggerResolver(Logger.CreateWithDefaultLog4NetConfiguration())
            {
                //This is another special resolver that needs to be resolvable before resolution is frozen
                //since it is used for profiling the application startup
                CanResolveBeforeFrozen = true
            };
        }

        /// <summary>
        /// Special method to initialize the ProfilerResolver
        /// </summary>
        protected virtual void InitializeProfilerResolver()
        {
            //By default we'll initialize the Log profiler (in the web project, we'll override with the web profiler)
            ProfilerResolver.Current = new ProfilerResolver(new LogProfiler(LoggerResolver.Current.Logger))
            {
                //This is another special resolver that needs to be resolvable before resolution is frozen
                //since it is used for profiling the application startup
                CanResolveBeforeFrozen = true
            };
        }

        /// <summary>
        /// Special method to initialize the ApplicationEventsResolver and any modifications required for it such 
        /// as adding custom types to the resolver.
        /// </summary>
        protected virtual void InitializeApplicationEventsResolver()
        {
            //find and initialize the application startup handlers, we need to initialize this resolver here because
            //it is a special resolver where they need to be instantiated first before any other resolvers in order to bind to 
            //events and to call their events during bootup.
            //ApplicationStartupHandler.RegisterHandlers();
            //... and set the special flag to let us resolve before frozen resolution
            ApplicationEventsResolver.Current = new ApplicationEventsResolver(
                ServiceProvider, 
                LoggerResolver.Current.Logger,
                PluginManager.ResolveApplicationStartupHandlers())
            {
                CanResolveBeforeFrozen = true
            };
        }

        /// <summary>
        /// Special method to extend the use of Umbraco by enabling the consumer to overwrite
        /// the absolute path to the root of an Umbraco site/solution, which is used for stuff
        /// like Umbraco.Core.IO.IOHelper.MapPath etc.
        /// </summary>
        /// <param name="rootPath">Absolute</param>
        protected virtual void InitializeApplicationRootPath(string rootPath)
        {
            Umbraco.Core.IO.IOHelper.SetRootDirectory(rootPath);
        }

        /// <summary>
        /// Fires after initialization and calls the callback to allow for customizations to occur & 
        /// Ensure that the OnApplicationStarting methods of the IApplicationEvents are called
        /// </summary>
        /// <param name="afterStartup"></param>
        /// <returns></returns>
        public virtual IBootManager Startup(Action<ApplicationContext> afterStartup)
        {
            if (_isStarted)
                throw new InvalidOperationException("The boot manager has already been initialized");

            //call OnApplicationStarting of each application events handler
            ApplicationEventsResolver.Current.ApplicationEventHandlers
                .ForEach(x => x.OnApplicationStarting(UmbracoApplication, ApplicationContext));

            if (afterStartup != null)
            {
                afterStartup(ApplicationContext.Current);
            }

            _isStarted = true;

            return this;
        }

        /// <summary>
        /// Fires after startup and calls the callback once customizations are locked
        /// </summary>
        /// <param name="afterComplete"></param>
        /// <returns></returns>
        public virtual IBootManager Complete(Action<ApplicationContext> afterComplete)
        {
            if (_isComplete)
                throw new InvalidOperationException("The boot manager has already been completed");

            FreezeResolution();

            //call OnApplicationStarting of each application events handler
            ApplicationEventsResolver.Current.ApplicationEventHandlers
                .ForEach(x => x.OnApplicationStarted(UmbracoApplication, ApplicationContext));

            //Now, startup all of our legacy startup handler
            ApplicationEventsResolver.Current.InstantiateLegacyStartupHandlers();

            if (afterComplete != null)
            {
                afterComplete(ApplicationContext.Current);
            }

            _isComplete = true;

            // we're ready to serve content!
            ApplicationContext.IsReady = true;

            //stop the timer and log the output
            _timer.Dispose();
            return this;
        }

        /// <summary>
        /// Freeze resolution to not allow Resolvers to be modified
        /// </summary>
        protected virtual void FreezeResolution()
        {
            Resolution.Freeze();
        }

        /// <summary>
        /// Create the resolvers
        /// </summary>
        protected virtual void InitializeResolvers()
        {
            var builder = new ManifestBuilder(
                ApplicationCache.RuntimeCache,
                new ManifestParser(new DirectoryInfo(IOHelper.MapPath("~/App_Plugins")), ApplicationCache.RuntimeCache));

            PropertyEditorResolver.Current = new PropertyEditorResolver(ServiceProvider, LoggerResolver.Current.Logger, () => PluginManager.ResolvePropertyEditors(), builder);
            ParameterEditorResolver.Current = new ParameterEditorResolver(ServiceProvider, LoggerResolver.Current.Logger, () => PluginManager.ResolveParameterEditors(), builder);

            //setup the validators resolver with our predefined validators
            ValidatorsResolver.Current = new ValidatorsResolver(
                ServiceProvider, LoggerResolver.Current.Logger, new[]
                {
                    new Lazy<Type>(() => typeof (RequiredManifestValueValidator)),
                    new Lazy<Type>(() => typeof (RegexValidator)),
                    new Lazy<Type>(() => typeof (DelimitedManifestValueValidator)),
                    new Lazy<Type>(() => typeof (EmailValidator)),
                    new Lazy<Type>(() => typeof (IntegerValidator)),
                });

            //by default we'll use the standard configuration based sync
            ServerRegistrarResolver.Current = new ServerRegistrarResolver(
                new ConfigServerRegistrar());

            //by default (outside of the web) we'll use the default server messenger without
            //supplying a username/password, this will automatically disable distributed calls
            // .. we'll override this in the WebBootManager
            ServerMessengerResolver.Current = new ServerMessengerResolver(
                new WebServiceServerMessenger());

            MappingResolver.Current = new MappingResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                () => PluginManager.ResolveAssignedMapperTypes());

           

            //RepositoryResolver.Current = new RepositoryResolver(
            //    new RepositoryFactory(ApplicationCache));

            CacheRefreshersResolver.Current = new CacheRefreshersResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                () => PluginManager.ResolveCacheRefreshers());

            DataTypesResolver.Current = new DataTypesResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                () => PluginManager.ResolveDataTypes());

            MacroFieldEditorsResolver.Current = new MacroFieldEditorsResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                () => PluginManager.ResolveMacroRenderings());

            PackageActionsResolver.Current = new PackageActionsResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                () => PluginManager.ResolvePackageActions());

            ActionsResolver.Current = new ActionsResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                () => PluginManager.ResolveActions());

            //the database migration objects
            MigrationResolver.Current = new MigrationResolver(
                LoggerResolver.Current.Logger,
                () => PluginManager.ResolveTypes<IMigration>());

            // todo: remove once we drop IPropertyEditorValueConverter support.
            PropertyEditorValueConvertersResolver.Current = new PropertyEditorValueConvertersResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                PluginManager.ResolvePropertyEditorValueConverters());

            // need to filter out the ones we dont want!!
            PropertyValueConvertersResolver.Current = new PropertyValueConvertersResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                PluginManager.ResolveTypes<IPropertyValueConverter>());

            // use the new DefaultShortStringHelper
            ShortStringHelperResolver.Current = new ShortStringHelperResolver(
                //new LegacyShortStringHelper());
                new DefaultShortStringHelper().WithDefaultConfig());

            UrlSegmentProviderResolver.Current = new UrlSegmentProviderResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                typeof(DefaultUrlSegmentProvider));

            // by default, no factory is activated
            PublishedContentModelFactoryResolver.Current = new PublishedContentModelFactoryResolver();
        }
    }
}
