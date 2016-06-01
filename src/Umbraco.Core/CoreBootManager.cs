using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using AutoMapper;
using LightInject;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Plugins;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Core.Strings;
using Umbraco.Core._Legacy.PackageActions;


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
        protected ProfilingLogger ProfilingLogger { get; private set; }
        private DisposableTimer _timer;
        protected PluginManager PluginManager { get; private set; }

        private IServiceContainer _appStartupEvtContainer;
        private bool _isInitialized = false;
        private bool _isStarted = false;
        private bool _isComplete = false;
        private readonly UmbracoApplicationBase _umbracoApplication;
        protected ApplicationContext ApplicationContext { get; private set; }
        protected CacheHelper ApplicationCache { get; private set; }

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

        internal CoreBootManager(UmbracoApplicationBase umbracoApplication, ProfilingLogger logger)
        {
            if (umbracoApplication == null) throw new ArgumentNullException("umbracoApplication");
            if (logger == null) throw new ArgumentNullException("logger");
            _umbracoApplication = umbracoApplication;
            ProfilingLogger = logger;
        }

        public virtual IBootManager Initialize()
        {
            if (_isInitialized)
                throw new InvalidOperationException("The boot manager has already been initialized");

            //Create logger/profiler, and their resolvers, these are special resolvers that can be resolved before frozen so we can start logging
            LoggerResolver.Current = new LoggerResolver(_umbracoApplication.Logger) { CanResolveBeforeFrozen = true };
            var profiler = CreateProfiler();
            ProfilerResolver.Current = new ProfilerResolver(profiler) { CanResolveBeforeFrozen = true };
            ProfilingLogger = new ProfilingLogger(_umbracoApplication.Logger, profiler);

            ProfilingLogger = ProfilingLogger?? new ProfilingLogger(LoggerResolver.Current.Logger, ProfilerResolver.Current.Profiler);

            ApplicationCache = CreateApplicationCache();

            _timer = ProfilingLogger.TraceDuration<CoreBootManager>(
                string.Format("Umbraco {0} application starting on {1}", UmbracoVersion.GetSemanticVersion().ToSemanticString(), NetworkHelper.MachineName),
                "Umbraco application startup complete");

            ServiceProvider = new ActivatorServiceProvider();

            //create the plugin manager
            //TODO: this is currently a singleton but it would be better if it weren't. Unfortunately the only way to get
            // rid of this singleton would be to put it into IoC and then use the ServiceLocator pattern.
            PluginManager.Current = PluginManager = new PluginManager(ServiceProvider, ApplicationCache.RuntimeCache, ProfilingLogger, true);

            //build up core IoC servoces
            ConfigureCoreServices(Container);

            //set the singleton resolved from the core container
            ApplicationContext.Current = ApplicationContext = Container.GetInstance<ApplicationContext>();

            //TODO: Remove these for v8!
            LegacyPropertyEditorIdToAliasConverter.CreateMappingsForCoreEditors();
            LegacyParameterEditorAliasConverter.CreateMappingsForCoreEditors();

            //Create a 'child'container which is a copy of all of the current registrations and begin a sub scope for it
            // this child container will be used to manage the application event handler instances and the scope will be
            // completed at the end of the boot process to allow garbage collection
            _appStartupEvtContainer = Container.Clone();
            _appStartupEvtContainer.BeginScope();
            _appStartupEvtContainer.RegisterCollection<PerScopeLifetime>(PluginManager.ResolveApplicationStartupHandlers());            

            //build up standard IoC services
            ConfigureApplicationServices(Container);

            InitializeResolvers();
            InitializeModelMappers();

            //now we need to call the initialize methods
            Parallel.ForEach(_appStartupEvtContainer.GetAllInstances<IApplicationEventHandler>(), x =>
            {
                try
                {
                    using (ProfilingLogger.DebugDuration<CoreBootManager>(
                        $"Executing {x.GetType()} in ApplicationInitialized",
                        $"Executed {x.GetType()} in ApplicationInitialized",
                        //only log if more than 150ms
                        150))
                    {
                        x.OnApplicationInitialized(UmbracoApplication, ApplicationContext);
                    }
                }
                catch (Exception ex)
                {
                    ProfilingLogger.Logger.Error<CoreBootManager>("An error occurred running OnApplicationInitialized for handler " + x.GetType(), ex);
                    throw;
                }
            });

            _isInitialized = true;

            return this;
        }

        /// <summary>
        /// Build the core container which contains all core things requird to build an app context
        /// </summary>
        internal virtual void ConfigureCoreServices(ServiceContainer container)
        {
            container.Register<IServiceContainer>(factory => container);

            //Logging
            container.RegisterSingleton<ILogger>(factory => _umbracoApplication.Logger);
            container.RegisterSingleton<IProfiler>(factory => ProfilingLogger.Profiler);
            container.RegisterSingleton<ProfilingLogger>(factory => ProfilingLogger);

            //Config
            container.RegisterFrom<ConfigurationCompositionRoot>();

            //Cache
            container.RegisterSingleton<CacheHelper>(factory => ApplicationCache);
            container.RegisterSingleton<IRuntimeCacheProvider>(factory => ApplicationCache.RuntimeCache);

            //Datalayer/Repositories/SQL/Database/etc...
            container.RegisterFrom<RepositoryCompositionRoot>();

            //Data Services/ServiceContext/etc...
            container.RegisterFrom<ServicesCompositionRoot>();

            //ModelMappers
            container.RegisterFrom<CoreModelMappersCompositionRoot>();

            //TODO: Don't think we'll need this when the resolvers are all container resolvers
            container.RegisterSingleton<IServiceProvider, ActivatorServiceProvider>();
            container.RegisterSingleton<PluginManager>(factory => PluginManager);

            container.RegisterSingleton<ApplicationContext>();
            container.Register<MediaFileSystem>(factory => FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>());

        }

        /// <summary>
        /// Called to customize the IoC container
        /// </summary>
        /// <param name="container"></param>
        internal virtual void ConfigureApplicationServices(ServiceContainer container)
        {

        }

        /// <summary>
        /// Creates the ApplicationCache based on a new instance of System.Web.Caching.Cache
        /// </summary>
        protected virtual CacheHelper CreateApplicationCache()
        {
            var cacheHelper = new CacheHelper(
                //we need to have the dep clone runtime cache provider to ensure 
                //all entities are cached properly (cloned in and cloned out)
                new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider()),
                new StaticCacheProvider(),
                //we have no request based cache when not running in web-based context
                new NullCacheProvider(),
                new IsolatedRuntimeCache(type =>
                    //we need to have the dep clone runtime cache provider to ensure 
                    //all entities are cached properly (cloned in and cloned out)
                    new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider())));

            return cacheHelper;
        }

        /// <summary>
        /// This method initializes all of the model mappers registered in the container
        /// </summary>        
        protected void InitializeModelMappers()
        {
            Mapper.Initialize(configuration =>
            {
                //foreach (var m in ApplicationEventsResolver.Current.ApplicationEventHandlers.OfType<IMapperConfiguration>())
                foreach (var m in Container.GetAllInstances<ModelMapperConfiguration>())
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
            Parallel.ForEach(_appStartupEvtContainer.GetAllInstances<IApplicationEventHandler>(), x =>
            {
                try
                {
                    using (ProfilingLogger.DebugDuration<CoreBootManager>(
                        $"Executing {x.GetType()} in ApplicationStarting",
                        $"Executed {x.GetType()} in ApplicationStarting",
                        //only log if more than 150ms
                        150))
                    {
                        x.OnApplicationStarting(UmbracoApplication, ApplicationContext);
                    }
                }
                catch (Exception ex)
                {
                    ProfilingLogger.Logger.Error<CoreBootManager>("An error occurred running OnApplicationStarting for handler " + x.GetType(), ex);
                    throw;
                }
            });

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

            //Here we need to make sure the db can be connected to
		    EnsureDatabaseConnection();


            //This is a special case for the user service, we need to tell it if it's an upgrade, if so we need to ensure that
            // exceptions are bubbled up if a user is attempted to be persisted during an upgrade (i.e. when they auth to login)
            ((UserService) ApplicationContext.Services.UserService).IsUpgrading = true;



            //call OnApplicationStarting of each application events handler
            Parallel.ForEach(_appStartupEvtContainer.GetAllInstances<IApplicationEventHandler>(), x =>
            {
                try
                {
                    using (ProfilingLogger.DebugDuration<CoreBootManager>(
                        $"Executing {x.GetType()} in ApplicationStarted",
                        $"Executed {x.GetType()} in ApplicationStarted", 
                        //only log if more than 150ms
                        150))
                    {
                        x.OnApplicationStarted(UmbracoApplication, ApplicationContext);
                    }
                }
                catch (Exception ex)
                {
                    ProfilingLogger.Logger.Error<CoreBootManager>("An error occurred running OnApplicationStarted for handler " + x.GetType(), ex);
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
        /// We cannot continue if the db cannot be connected to
        /// </summary>
        private void EnsureDatabaseConnection()
        {
            if (ApplicationContext.IsConfigured == false) return;
            if (ApplicationContext.DatabaseContext.IsDatabaseConfigured == false) return;

            //try now
            if (ApplicationContext.DatabaseContext.CanConnect)
                return;

            var currentTry = 0;
            while (currentTry < 5)
            {
                //first wait, then retry
                Thread.Sleep(1000);

                if (ApplicationContext.DatabaseContext.CanConnect)
                    break;

                currentTry++;
            }

            if (currentTry == 5)
            {
                throw new UmbracoStartupFailedException("Umbraco cannot start. A connection string is configured but the Umbraco cannot connect to the database.");
            }
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

            var manifestParser = new ManifestParser(ProfilingLogger.Logger, new DirectoryInfo(IOHelper.MapPath("~/App_Plugins")), ApplicationCache.RuntimeCache);
            var manifestBuilder = new ManifestBuilder(ApplicationCache.RuntimeCache, manifestParser);

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
                    new Lazy<Type>(() => typeof (DecimalValidator)),
                });

            //by default we'll use the db server registrar unless the developer has the legacy
            // dist calls enabled, in which case we'll use the config server registrar
            if (UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled)
            {
                ServerRegistrarResolver.Current = new ServerRegistrarResolver(new ConfigServerRegistrar(UmbracoConfig.For.UmbracoSettings()));
            }
            else
            {
                ServerRegistrarResolver.Current = new ServerRegistrarResolver(
                    new DatabaseServerRegistrar(
                        new Lazy<IServerRegistrationService>(() => ApplicationContext.Services.ServerRegistrationService),
                        new DatabaseServerRegistrarOptions()));
            }


            //by default we'll use the database server messenger with default options (no callbacks),
            // this will be overridden in the web startup
            ServerMessengerResolver.Current = new ServerMessengerResolver(
                Container,
                factory => new DatabaseServerMessenger(ApplicationContext, true, new DatabaseServerMessengerOptions()));

            MappingResolver.Current = new MappingResolver(
                Container, ProfilingLogger.Logger,
                () => PluginManager.ResolveAssignedMapperTypes());


            //RepositoryResolver.Current = new RepositoryResolver(
            //    new RepositoryFactory(ApplicationCache));

            CacheRefreshersResolver.Current = new CacheRefreshersResolver(
                Container, ProfilingLogger.Logger,
                () => PluginManager.ResolveCacheRefreshers());
                        
            PackageActionsResolver.Current = new PackageActionsResolver(
                ServiceProvider, ProfilingLogger.Logger,
                () => PluginManager.ResolvePackageActions());
            
            //the database migration objects
            MigrationResolver.Current = new MigrationResolver(
                Container, ProfilingLogger.Logger,
                () => PluginManager.ResolveTypes<IMigration>());


            // need to filter out the ones we dont want!!
            PropertyValueConvertersResolver.Current = new PropertyValueConvertersResolver(
                Container, ProfilingLogger.Logger,
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
        
    }
}
