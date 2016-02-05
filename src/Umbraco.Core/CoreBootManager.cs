using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using AutoMapper;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
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
        protected ProfilingLogger ProfilingLogger { get; private set; }
        private DisposableTimer _timer;
        private bool _isInitialized = false;
        private bool _isStarted = false;
        private bool _isComplete = false;
        private readonly IServiceProvider _serviceProvider = new ActivatorServiceProvider();
        private readonly UmbracoApplicationBase _umbracoApplication;
        protected ApplicationContext ApplicationContext { get; private set; }
        protected CacheHelper ApplicationCache { get; private set; }
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

            InitializeLoggerResolver();
            InitializeProfilerResolver();

            ProfilingLogger = ProfilingLogger?? new ProfilingLogger(LoggerResolver.Current.Logger, ProfilerResolver.Current.Profiler);

            _timer = ProfilingLogger.TraceDuration<CoreBootManager>(
                string.Format("Umbraco {0} application starting on {1}", UmbracoVersion.GetSemanticVersion().ToSemanticString(), NetworkHelper.MachineName),
                "Umbraco application startup complete");

            ApplicationCache = CreateApplicationCache();

            //create and set the plugin manager (I'd much prefer to not use this singleton anymore but many things are using it unfortunately and
            // the way that it is setup, there must only ever be one per app so without IoC it would be hard to make this not a singleton)
            PluginManager = new PluginManager(ServiceProvider, ApplicationCache.RuntimeCache, ProfilingLogger);
            PluginManager.Current = PluginManager;

            //Create the legacy prop-eds mapping
            LegacyPropertyEditorIdToAliasConverter.CreateMappingsForCoreEditors();
            LegacyParameterEditorAliasConverter.CreateMappingsForCoreEditors();

            //create database and service contexts for the app context
            var dbFactory = new DefaultDatabaseFactory(GlobalSettings.UmbracoConnectionName, ProfilingLogger.Logger);
            Database.Mapper = new PetaPocoMapper();

            var dbContext = new DatabaseContext(
                dbFactory,
                ProfilingLogger.Logger,
                SqlSyntaxProviders.CreateDefault(ProfilingLogger.Logger));

            //initialize the DatabaseContext
            dbContext.Initialize();

            //get the service context
            var serviceContext = CreateServiceContext(dbContext, dbFactory);

            //set property and singleton from response
            ApplicationContext.Current = ApplicationContext = CreateApplicationContext(dbContext, serviceContext);

            InitializeApplicationEventsResolver();

            InitializeResolvers();

            InitializeModelMappers();

            using (ProfilingLogger.DebugDuration<CoreBootManager>(
                string.Format("Executing {0} IApplicationEventHandler.OnApplicationInitialized", ApplicationEventsResolver.Current.ApplicationEventHandlers.Count()),
                "Finished executing IApplicationEventHandler.OnApplicationInitialized"))
	        {
                //now we need to call the initialize methods
                ApplicationEventsResolver.Current.ApplicationEventHandlers
                    .ForEach(x =>
                    {
                        try
                        {
                            using (ProfilingLogger.DebugDuration<CoreBootManager>(string.Format("Executing {0} in ApplicationInitialized", x.GetType())))
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
	        }

            _isInitialized = true;

            return this;
        }

        /// <summary>
        /// Creates and returns the service context for the app
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="dbFactory"></param>
        /// <returns></returns>
        protected virtual ServiceContext CreateServiceContext(DatabaseContext dbContext, IDatabaseFactory dbFactory)
        {
            //default transient factory
            var msgFactory = new TransientMessagesFactory();
            return new ServiceContext(
                new RepositoryFactory(ApplicationCache, ProfilingLogger.Logger, dbContext.SqlSyntax, UmbracoConfig.For.UmbracoSettings()),
                new PetaPocoUnitOfWorkProvider(dbFactory),
                new FileUnitOfWorkProvider(),
                new PublishingStrategy(msgFactory, ProfilingLogger.Logger),
                ApplicationCache,
                ProfilingLogger.Logger,
                msgFactory);
        }

        /// <summary>
        /// Creates and returns the application context for the app
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="serviceContext"></param>
        protected virtual ApplicationContext CreateApplicationContext(DatabaseContext dbContext, ServiceContext serviceContext)
        {
            //create the ApplicationContext
            return new ApplicationContext(dbContext, serviceContext, ApplicationCache, ProfilingLogger);
        }

        /// <summary>
        /// Creates and returns the CacheHelper for the app
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
            LoggerResolver.Current = new LoggerResolver(ProfilingLogger == null ? Logger.CreateWithDefaultLog4NetConfiguration() : ProfilingLogger.Logger)
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
            ProfilerResolver.Current = new ProfilerResolver(ProfilingLogger == null ? new LogProfiler(LoggerResolver.Current.Logger) : ProfilingLogger.Profiler)
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
                ProfilingLogger.Logger,
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
            IOHelper.SetRootDirectory(rootPath);
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

            using (ProfilingLogger.DebugDuration<CoreBootManager>(
                string.Format("Executing {0} IApplicationEventHandler.OnApplicationStarting", ApplicationEventsResolver.Current.ApplicationEventHandlers.Count()),
                "Finished executing IApplicationEventHandler.OnApplicationStarting"))
		    {
		        //call OnApplicationStarting of each application events handler
		        ApplicationEventsResolver.Current.ApplicationEventHandlers
		            .ForEach(x =>
		            {
		                try
		                {
		                    using (ProfilingLogger.DebugDuration<CoreBootManager>(string.Format("Executing {0} in ApplicationStarting", x.GetType())))
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
		    }

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


            using (ProfilingLogger.DebugDuration<CoreBootManager>(
                string.Format("Executing {0} IApplicationEventHandler.OnApplicationStarted", ApplicationEventsResolver.Current.ApplicationEventHandlers.Count()),
                "Finished executing IApplicationEventHandler.OnApplicationStarted"))
            {
                //call OnApplicationStarting of each application events handler
                ApplicationEventsResolver.Current.ApplicationEventHandlers
                    .ForEach(x =>
                    {
                        try
                        {
                            using (ProfilingLogger.DebugDuration<CoreBootManager>(string.Format("Executing {0} in ApplicationStarted", x.GetType())))
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
            }

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
            var builder = new ManifestBuilder(
                ApplicationCache.RuntimeCache,
                new ManifestParser(new DirectoryInfo(IOHelper.MapPath("~/App_Plugins")), ApplicationCache.RuntimeCache));

            PropertyEditorResolver.Current = new PropertyEditorResolver(ServiceProvider, ProfilingLogger.Logger, () => PluginManager.ResolvePropertyEditors(), builder);
            ParameterEditorResolver.Current = new ParameterEditorResolver(ServiceProvider, ProfilingLogger.Logger, () => PluginManager.ResolveParameterEditors(), builder);

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
                ServerRegistrarResolver.Current = new ServerRegistrarResolver(new ConfigServerRegistrar());
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
                new DatabaseServerMessenger(ApplicationContext, true, new DatabaseServerMessengerOptions()));

            MappingResolver.Current = new MappingResolver(
                ServiceProvider, ProfilingLogger.Logger,
                () => PluginManager.ResolveAssignedMapperTypes());

           
            //RepositoryResolver.Current = new RepositoryResolver(
            //    new RepositoryFactory(ApplicationCache));

            CacheRefreshersResolver.Current = new CacheRefreshersResolver(
                ServiceProvider, ProfilingLogger.Logger,
                () => PluginManager.ResolveCacheRefreshers());

            DataTypesResolver.Current = new DataTypesResolver(
                ServiceProvider, ProfilingLogger.Logger,
                () => PluginManager.ResolveDataTypes());

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
                ProfilingLogger.Logger,
                () => PluginManager.ResolveTypes<IMigration>());

            // todo: remove once we drop IPropertyEditorValueConverter support.
            PropertyEditorValueConvertersResolver.Current = new PropertyEditorValueConvertersResolver(
                ServiceProvider, ProfilingLogger.Logger,
                PluginManager.ResolvePropertyEditorValueConverters());

            // need to filter out the ones we dont want!!
            PropertyValueConvertersResolver.Current = new PropertyValueConvertersResolver(
                ServiceProvider, ProfilingLogger.Logger,
                PluginManager.ResolveTypes<IPropertyValueConverter>());

            // use the new DefaultShortStringHelper
            ShortStringHelperResolver.Current = new ShortStringHelperResolver(
                //new LegacyShortStringHelper());
                new DefaultShortStringHelper(UmbracoConfig.For.UmbracoSettings()).WithDefaultConfig());

            UrlSegmentProviderResolver.Current = new UrlSegmentProviderResolver(
                ServiceProvider, ProfilingLogger.Logger,
                typeof(DefaultUrlSegmentProvider));

            // by default, no factory is activated
            PublishedContentModelFactoryResolver.Current = new PublishedContentModelFactoryResolver();
        }
    }
}
