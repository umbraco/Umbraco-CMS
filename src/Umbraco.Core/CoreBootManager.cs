using System;
using System.IO;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.LightInject;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Profiling;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Publishing;
using Umbraco.Core.Macros;
using Umbraco.Core.Manifest;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Core.Strings;


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
        
        private ServiceContainer _appStartupEvtContainer;
        protected ProfilingLogger ProfilingLogger { get; private set; }
        private DisposableTimer _timer;
        protected PluginManager PluginManager { get; private set; }
        private CacheHelper _cacheHelper;

        private bool _isInitialized = false;
        private bool _isStarted = false;
        private bool _isComplete = false;
        private readonly UmbracoApplicationBase _umbracoApplication;

        protected ApplicationContext ApplicationContext { get; private set; }

        protected UmbracoApplicationBase UmbracoApplication
        {
            get { return _umbracoApplication; }
        }

        internal ServiceContainer Container
        {
            get { return _umbracoApplication.Container; }
        }

        protected IServiceProvider ServiceProvider { get; private set; }

        public CoreBootManager(UmbracoApplicationBase umbracoApplication)
        {
            if (umbracoApplication == null) throw new ArgumentNullException("umbracoApplication");
            _umbracoApplication = umbracoApplication;            
        }

        public virtual IBootManager Initialize()
        {
            if (_isInitialized)
                throw new InvalidOperationException("The boot manager has already been initialized");

            //Create logger/profiler, and their resolvers, these are special resolvers that can be resolved before frozen so we can start logging
            LoggerResolver.Current = new LoggerResolver(_umbracoApplication.Logger) { CanResolveBeforeFrozen = true };
            var profiler = CreateProfiler();
            ProfilerResolver.Current = new ProfilerResolver(profiler) {CanResolveBeforeFrozen = true};
            ProfilingLogger = new ProfilingLogger(_umbracoApplication.Logger, profiler);

            _timer = ProfilingLogger.DebugDuration<CoreBootManager>("Umbraco application starting", "Umbraco application startup complete");

            //create the plugin manager
            //TODO: this is currently a singleton but it would be better if it weren't. Unfortunately the only way to get
            // rid of this singleton would be to put it into IoC and then use the ServiceLocator pattern.
            _cacheHelper = CreateApplicationCache();
            ServiceProvider = new ActivatorServiceProvider();
            PluginManager.Current = PluginManager = new PluginManager(ServiceProvider, _cacheHelper.RuntimeCache, ProfilingLogger, true);

            //build up core IoC servoces
            ConfigureCoreServices(Container);

            //set the singleton resolved from the core container
            ApplicationContext.Current = ApplicationContext = Container.GetInstance<ApplicationContext>();

            //TODO: Remove these for v8! 
            LegacyPropertyEditorIdToAliasConverter.CreateMappingsForCoreEditors();
            LegacyParameterEditorAliasConverter.CreateMappingsForCoreEditors();
            //TODO: Make this as part of the db ctor!
            Database.Mapper = new PetaPocoMapper();
            
            //Create a 'child'container which is a copy of all of the current registrations and begin a sub scope for it
            // this child container will be used to manage the application event handler instances and the scope will be
            // completed at the end of the boot process to allow garbage collection
            _appStartupEvtContainer = Container.CreateChildContainer();
            _appStartupEvtContainer.BeginScope();
            _appStartupEvtContainer.RegisterCollection<IApplicationEventHandler, PerScopeLifetime>(PluginManager.ResolveApplicationStartupHandlers());
            
            //build up standard IoC services
            ConfigureServices(Container);

            InitializeResolvers();
            InitializeModelMappers();

            //now we need to call the initialize methods
            Parallel.ForEach(_appStartupEvtContainer.GetAllInstances<IApplicationEventHandler>(), x => x.OnApplicationInitialized(UmbracoApplication, ApplicationContext));

            _isInitialized = true;

            return this;
        }

        /// <summary>
        /// Build the core container which contains all core things requird to build an app context
        /// </summary>
        private void ConfigureCoreServices(ServiceContainer container)
        {
            container.Register<IServiceContainer>(factory => container);
            container.Register<ILogger>(factory => _umbracoApplication.Logger, new PerContainerLifetime());
            container.Register<IProfiler>(factory => ProfilingLogger.Profiler, new PerContainerLifetime());
            container.Register<ProfilingLogger>(factory => ProfilingLogger, new PerContainerLifetime());
            var settings = UmbracoConfig.For.UmbracoSettings();
            container.Register<IUmbracoSettingsSection>(factory => settings);
            container.Register<IContentSection>(factory => settings.Content);
            //TODO: Add the other config areas...
            container.Register<CacheHelper>(factory => _cacheHelper, new PerContainerLifetime());
            container.Register<IRuntimeCacheProvider>(factory => _cacheHelper.RuntimeCache, new PerContainerLifetime());
            container.Register<IServiceProvider, ActivatorServiceProvider>();
            container.Register<PluginManager>(factory => PluginManager, new PerContainerLifetime());
            container.Register<IDatabaseFactory>(factory => new DefaultDatabaseFactory(GlobalSettings.UmbracoConnectionName, factory.GetInstance<ILogger>()));
            container.Register<DatabaseContext>(factory => GetDbContext(factory), new PerContainerLifetime());
            container.Register<SqlSyntaxProviders>(factory => SqlSyntaxProviders.CreateDefault(factory.GetInstance<ILogger>()));
            container.Register<IDatabaseUnitOfWorkProvider, PetaPocoUnitOfWorkProvider>();
            container.Register<IUnitOfWorkProvider, FileUnitOfWorkProvider>();
            container.Register<BasePublishingStrategy, PublishingStrategy>();
            container.Register<IMappingResolver>(factory => new MappingResolver(
                factory.GetInstance<IServiceContainer>(), 
                factory.GetInstance<ILogger>(),
                () => PluginManager.ResolveAssignedMapperTypes()));
            container.Register<RepositoryFactory>();
            container.Register<ServiceContext>(factory => new ServiceContext(
                factory.GetInstance<RepositoryFactory>(),
                factory.GetInstance<IDatabaseUnitOfWorkProvider>(),
                factory.GetInstance<IUnitOfWorkProvider>(),
                factory.GetInstance<BasePublishingStrategy>(),
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetAllInstances<IUrlSegmentProvider>()));
            container.Register<ApplicationContext>(new PerContainerLifetime());
            container.Register<MediaFileSystem>(factory => FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>());

            container.Register<ISqlSyntaxProvider>(factory => factory.GetInstance<DatabaseContext>().SqlSyntax);
        }

        /// <summary>
        /// Called to customize the IoC container
        /// </summary>
        /// <param name="container"></param>
        internal virtual void ConfigureServices(ServiceContainer container)
        {
            
        }

        /// <summary>
        /// Creates and initializes the db context when IoC requests it
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        private DatabaseContext GetDbContext(IServiceFactory container)
        {
            var dbCtx = new DatabaseContext(
                container.GetInstance<IDatabaseFactory>(),
                container.GetInstance<ILogger>(),
                container.GetInstance<SqlSyntaxProviders>());

            //when it's first created we need to initialize it
            dbCtx.Initialize();
            return dbCtx;
        }

        /// <summary>
        /// Creates the ApplicationCache based on a new instance of System.Web.Caching.Cache
        /// </summary>
        protected virtual CacheHelper CreateApplicationCache()
        {
            var cacheHelper = new CacheHelper(
                new ObjectCacheRuntimeCacheProvider(),
                new StaticCacheProvider(),
                //we have no request based cache when not running in web-based context
                new NullCacheProvider());

            return cacheHelper;
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
                    //foreach (var m in ApplicationEventsResolver.Current.ApplicationEventHandlers.OfType<IMapperConfiguration>())
                    foreach (var m in _appStartupEvtContainer.GetAllInstances<IApplicationEventHandler>().OfType<IMapperConfiguration>())
                    {
                        m.ConfigureMappings(configuration, ApplicationContext);
                    }
                });
        }

        /// <summary>
        /// Creates the application's IProfiler
        /// </summary>
        protected virtual IProfiler CreateProfiler()
        {
            return new LogProfiler(ProfilingLogger.Logger);
        }

        /// <summary>
        /// Special method to extend the use of Umbraco by enabling the consumer to overwrite
        /// the absolute path to the root of an Umbraco site/solution, which is used for stuff
        /// like Umbraco.Core.IO.IOHelper.MapPath etc.
        /// </summary>
        /// <param name="rootPath">Absolute</param>
        protected virtual void InitializeApplicationRootPath(string rootPath)
        {
            IO.IOHelper.SetRootDirectory(rootPath);
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
            Parallel.ForEach(_appStartupEvtContainer.GetAllInstances<IApplicationEventHandler>(), x => x.OnApplicationStarting(UmbracoApplication, ApplicationContext));

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
            Parallel.ForEach(_appStartupEvtContainer.GetAllInstances<IApplicationEventHandler>(), x => x.OnApplicationStarted(UmbracoApplication, ApplicationContext));

            //end the current scope which was created to intantiate all of the startup handlers
            _appStartupEvtContainer.EndCurrentScope();

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
            var manifestParser = new ManifestParser(ProfilingLogger.Logger, new DirectoryInfo(IOHelper.MapPath("~/App_Plugins")), _cacheHelper.RuntimeCache);
            var manifestBuilder = new ManifestBuilder(_cacheHelper.RuntimeCache, manifestParser);

            PropertyEditorResolver.Current = new PropertyEditorResolver(
                Container, ProfilingLogger.Logger, () => PluginManager.ResolvePropertyEditors(),
                manifestBuilder);
            ParameterEditorResolver.Current = new ParameterEditorResolver(
                Container, ProfilingLogger.Logger, () => PluginManager.ResolveParameterEditors(),
                manifestBuilder);

            //setup the validators resolver with our predefined validators
            ValidatorsResolver.Current = new ValidatorsResolver(
                ServiceProvider, ProfilingLogger.Logger, new[]
                {
                    new Lazy<Type>(() => typeof (RequiredManifestValueValidator)),
                    new Lazy<Type>(() => typeof (RegexValidator)),
                    new Lazy<Type>(() => typeof (DelimitedManifestValueValidator)),
                    new Lazy<Type>(() => typeof (EmailValidator)),
                    new Lazy<Type>(() => typeof (IntegerValidator)),
                });

            //by default we'll use the standard configuration based sync
            ServerRegistrarResolver.Current = new ServerRegistrarResolver(Container, typeof(ConfigServerRegistrar));

            //by default (outside of the web) we'll use the default server messenger without
            //supplying a username/password, this will automatically disable distributed calls
            // .. we'll override this in the WebBootManager
            ServerMessengerResolver.Current = new ServerMessengerResolver(Container, typeof (WebServiceServerMessenger));


            //RepositoryResolver.Current = new RepositoryResolver(
            //    new RepositoryFactory(ApplicationCache));

            CacheRefreshersResolver.Current = new CacheRefreshersResolver(
                ServiceProvider, ProfilingLogger.Logger,
                () => PluginManager.ResolveCacheRefreshers());

            MacroFieldEditorsResolver.Current = new MacroFieldEditorsResolver(
                ServiceProvider, ProfilingLogger.Logger,
                () => PluginManager.ResolveMacroRenderings());

            PackageActionsResolver.Current = new PackageActionsResolver(
                ServiceProvider, ProfilingLogger.Logger,
                () => PluginManager.ResolvePackageActions());

            ActionsResolver.Current = new ActionsResolver(
                ServiceProvider, ProfilingLogger.Logger,
                () => PluginManager.ResolveActions());

            //the database migration objects
            MigrationResolver.Current = new MigrationResolver(
                Container, ProfilingLogger.Logger,
                () => PluginManager.ResolveTypes<IMigration>());


            // need to filter out the ones we dont want!!
            PropertyValueConvertersResolver.Current = new PropertyValueConvertersResolver(
                ServiceProvider, ProfilingLogger.Logger,
                PluginManager.ResolveTypes<IPropertyValueConverter>());

            // use the new DefaultShortStringHelper
            ShortStringHelperResolver.Current = new ShortStringHelperResolver(Container, 
                factory => new DefaultShortStringHelper(factory.GetInstance<IUmbracoSettingsSection>()).WithDefaultConfig());

            UrlSegmentProviderResolver.Current = new UrlSegmentProviderResolver(
                Container, ProfilingLogger.Logger,
                typeof(DefaultUrlSegmentProvider));

            // by default, no factory is activated
            PublishedContentModelFactoryResolver.Current = new PublishedContentModelFactoryResolver(Container);
        }

        ///// <summary>
        ///// An IoC lifetime that will dispose instances at the end of the bootup sequence
        ///// </summary>
        //private class BootManagerLifetime : ILifetime
        //{
        //    public BootManagerLifetime(UmbracoApplicationBase appBase)
        //    {
        //        appBase.ApplicationStarted += appBase_ApplicationStarted;
        //    }

        //    void appBase_ApplicationStarted(object sender, EventArgs e)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    private object _instance;

        //    /// <summary>
        //    /// Returns a service instance according to the specific lifetime characteristics.
        //    /// </summary>
        //    /// <param name="createInstance">The function delegate used to create a new service instance.</param>
        //    /// <param name="scope">The <see cref="Scope"/> of the current service request.</param>
        //    /// <returns>The requested services instance.</returns>
        //    public object GetInstance(Func<object> createInstance, Scope scope)
        //    {
        //        if (_instance == null)
        //        {
        //            _instance = createInstance();

        //            var disposable = _instance as IDisposable;
        //            if (disposable != null)
        //            {
        //                if (scope == null)
        //                {
        //                    throw new InvalidOperationException("Attempt to create an disposable object without a current scope.");
        //                }
        //                scope.TrackInstance(disposable);
        //            }

        //        }
        //        return createInstance;
        //    }
        //}
    }
}
