using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using AutoMapper;
using LightInject;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.PublishedContent;
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
    /// Represents the Core Umbraco runtime.
    /// </summary>
    /// <remarks>Does not handle any of the web-related aspects of Umbraco (startup, etc). It
    /// should be possible to use this runtime in console apps.</remarks>
    public class CoreRuntime : IRuntime
    {
        private BootLoader _bootLoader;
        private DisposableTimer _timer;

        // fixme cleanup these
        private IServiceContainer _appStartupEvtContainer;
        private bool _isInitialized;
        private bool _isStarted;
        private bool _isComplete;


        // what the UmbracoApplication does is...
        // create the container and configure for core
        // create and register a logger
        // then:
        //GetBootManager()
        //    .Initialize()
        //    .Startup(appContext => OnApplicationStarting(sender, e))
        //    .Complete(appContext => OnApplicationStarted(sender, e));
        //
        // edit so it becomes:
        //GetBootLoader()
        //    .Boot();
        //
        // merge all RegisterX into one Register
        //
        // WebBootLoader should
        // configure the container for web BUT that should be AFTER the components have initialized? OR?
        //
        // Startup runs all app event handler OnApplicationStarting methods
        //  then triggers the OnApplicationStarting event of the app
        // Complete
        //   freezes resolution
        //   ensures database connection (else?)
        //   tells user service it is upgrading (?)
        //   runs all app event handler OnApplicationStarted methods
        //   then triggers the OnApplicationStarted event of the app
        //   and sets Ready
        //
        // note: what's deciding whether install, upgrade, run?

        private RuntimeState _state; // fixme what about web?!

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRuntime"/> class.
        /// </summary>
        /// <param name="umbracoApplication">The Umbraco HttpApplication.</param>
        public CoreRuntime(UmbracoApplicationBase umbracoApplication)
        {
            if (umbracoApplication == null) throw new ArgumentNullException(nameof(umbracoApplication));
            UmbracoApplication = umbracoApplication;
        }

        /// <inheritdoc/>
        public virtual void Boot(ServiceContainer container)
        {
            // create and register essential stuff

            Logger = container.GetInstance<ILogger>();
            container.RegisterInstance(Profiler = GetProfiler());
            container.RegisterInstance(ProfilingLogger = new ProfilingLogger(Current.Logger, Profiler));

            container.RegisterInstance(_state = new RuntimeState());

            // then compose
            Compose(container);

            // fixme!
            Compose1(container);

            // the boot loader boots using a container scope, so anything that is PerScope will
            // be disposed after the boot loader has booted, and anything else will remain.
            // note that this REQUIRES that perWebRequestScope has NOT been enabled yet, else
            // the container will fail to create a scope since there is no http context when
            // the application starts.
            // the boot loader is kept in the runtime for as long as Umbraco runs, and components
            // are NOT disposed - which is not a big deal as long as they remain lightweight
            // objects.

            _bootLoader = new BootLoader(container);
            _bootLoader.Boot(GetComponentTypes());
        }

        public virtual void Terminate()
        {
            _bootLoader?.Terminate();
        }

        public virtual void Compose(ServiceContainer container)
        {
            // create and register essential stuff

            var cache = GetApplicationCache();
            container.RegisterInstance(cache);
            container.RegisterInstance(cache.RuntimeCache);

            container.RegisterInstance(new PluginManager(cache.RuntimeCache, ProfilingLogger));

            // register from roots
            container.RegisterFrom<ConfigurationCompositionRoot>(); // fixme - used to be before caches?
            container.RegisterFrom<RepositoryCompositionRoot>();
            container.RegisterFrom<ServicesCompositionRoot>();
            container.RegisterFrom<CoreModelMappersCompositionRoot>();
        }

        protected virtual void Compose1(ServiceContainer container)
        {
            ServiceProvider = new ActivatorServiceProvider();

            //create the plugin manager
            //TODO: this is currently a singleton but it would be better if it weren't. Unfortunately the only way to get
            // rid of this singleton would be to put it into IoC and then use the ServiceLocator pattern.
            PluginManager.Current = PluginManager = Current.PluginManager; //new PluginManager(ApplicationCache.RuntimeCache, ProfilingLogger, true);

            //TODO: Don't think we'll need this when the resolvers are all container resolvers
            container.RegisterSingleton<IServiceProvider, ActivatorServiceProvider>();
            container.RegisterSingleton<ApplicationContext>();
            container.Register<MediaFileSystem>(factory => FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>());

            // fixme - should we capture Logger, etc here or use factory?

            // register manifest builder, will be injected in eg PropertyEditorCollectionBuilder
            container.RegisterSingleton(factory
                => new ManifestParser(factory.GetInstance<ILogger>(), new DirectoryInfo(IOHelper.MapPath("~/App_Plugins")), factory.GetInstance<IRuntimeCacheProvider>()));
            container.RegisterSingleton<ManifestBuilder>();

            PropertyEditorCollectionBuilder.Register(container)
                .AddProducer(factory => factory.GetInstance<PluginManager>().ResolvePropertyEditors());

            ParameterEditorCollectionBuilder.Register(container)
                .AddProducer(factory => factory.GetInstance<PluginManager>().ResolveParameterEditors());

            // register our predefined validators
            ValidatorCollectionBuilder.Register(container)
                .Add<RequiredManifestValueValidator>()
                .Add<RegexValidator>()
                .Add<DelimitedManifestValueValidator>()
                .Add<EmailValidator>()
                .Add<IntegerValidator>()
                .Add<DecimalValidator>();

            // register a server registrar, by default it's the db registrar unless the dev
            // has the legacy dist calls enabled - fixme - should obsolete the legacy thing
            container.RegisterSingleton(factory => UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled
                ? (IServerRegistrar)new ConfigServerRegistrar(UmbracoConfig.For.UmbracoSettings())
                : (IServerRegistrar)new DatabaseServerRegistrar(
                    new Lazy<IServerRegistrationService>(() => factory.GetInstance<ApplicationContext>().Services.ServerRegistrationService),
                    new DatabaseServerRegistrarOptions()));

            // by default we'll use the database server messenger with default options (no callbacks),
            // this will be overridden in the web startup
            // fixme - painful, have to take care of lifetime! - we CANNOT ask users to remember!
            // fixme - same issue with PublishedContentModelFactory and many more, I guess!
            container.RegisterSingleton<IServerMessenger>(factory 
                => new DatabaseServerMessenger(factory.GetInstance<ApplicationContext>(), true, new DatabaseServerMessengerOptions()));

            CacheRefresherCollectionBuilder.Register(container)
                .AddProducer(factory => factory.GetInstance<PluginManager>().ResolveCacheRefreshers());

            PackageActionCollectionBuilder.Register(container)
                .AddProducer(f => f.GetInstance<PluginManager>().ResolvePackageActions());

            MigrationCollectionBuilder.Register(container)
                .AddProducer(factory => factory.GetInstance<PluginManager>().ResolveTypes<IMigration>());

            // need to filter out the ones we dont want!! fixme - what does that mean?
            PropertyValueConverterCollectionBuilder.Register(container)
                .Append(factory => factory.GetInstance<PluginManager>().ResolveTypes<IPropertyValueConverter>());

            container.RegisterSingleton<IShortStringHelper>(factory
                => new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(factory.GetInstance<IUmbracoSettingsSection>())));

            UrlSegmentProviderCollectionBuilder.Register(container)
                .Append<DefaultUrlSegmentProvider>();

            // by default, register a noop factory
            container.RegisterSingleton<IPublishedContentModelFactory, NoopPublishedContentModelFactory>();
        }

        /// <summary>
        /// Gets the Umbraco HttpApplication.
        /// </summary>
        protected UmbracoApplicationBase UmbracoApplication { get; }

        #region Locals

        protected ILogger Logger { get; private set; }

        protected IProfiler Profiler { get; private set; }

        protected ProfilingLogger ProfilingLogger { get; private set; }

        // fixme do we need that one?
        protected PluginManager PluginManager { get; private set; }

        #endregion

        #region Getters

        // getters can be implemented by runtimes inheriting from CoreRuntime

        protected virtual IEnumerable<Type> GetComponentTypes() => Current.PluginManager.ResolveTypes<IUmbracoComponent>();

        protected virtual IProfiler GetProfiler() => new LogProfiler(Logger);

        protected virtual CacheHelper GetApplicationCache() => new CacheHelper(
                // we need to have the dep clone runtime cache provider to ensure
                // all entities are cached properly (cloned in and cloned out)
                new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider()),
                new StaticCacheProvider(),
                // we have no request based cache when not running in web-based context
                new NullCacheProvider(),
                new IsolatedRuntimeCache(type =>
                    // we need to have the dep clone runtime cache provider to ensure
                    // all entities are cached properly (cloned in and cloned out)
                    new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider())));

        #endregion

        #region Core

        // cannot run if the db is not there
        // tries to connect to db (if configured)
        private void EnsureDatabaseConnection()
        {
            if (Current.ApplicationContext.IsConfigured == false) return;
            if (Current.ApplicationContext.DatabaseContext.IsDatabaseConfigured == false) return;

            for (var i = 0; i < 5; i++)
            {
                if (Current.ApplicationContext.DatabaseContext.CanConnect) return;
                Thread.Sleep(1000);
            }

            throw new UmbracoStartupFailedException("Umbraco cannot start: a connection string is configured but Umbraco could not connect to the database.");
        }

        protected void InitializeModelMappers()
        {
            Mapper.Initialize(configuration =>
            {
                // fixme why ApplicationEventHandler?!
                //foreach (var m in ApplicationEventsResolver.Current.ApplicationEventHandlers.OfType<IMapperConfiguration>())
                foreach (var m in Container.GetAllInstances<ModelMapperConfiguration>())
                {
                    Logger.Debug<CoreRuntime>("FIXME " + m.GetType().FullName);
                    m.ConfigureMappings(configuration, Current.ApplicationContext);
                }
            });
        }

        /// <summary>
        /// Special method to extend the use of Umbraco by enabling the consumer to overwrite
        /// the absolute path to the root of an Umbraco site/solution, which is used for stuff
        /// like Umbraco.Core.IO.IOHelper.MapPath etc.
        /// </summary>
        /// <param name="rootPath">Absolute Umbraco root path.</param>
        protected virtual void InitializeApplicationRootPath(string rootPath)
        {
            IOHelper.SetRootDirectory(rootPath);
        }

        #endregion

        // FIXME everything below needs to be sorted out!


        protected ServiceContainer Container => Current.Container; // fixme kill

        protected IServiceProvider ServiceProvider { get; private set; }

        public virtual IRuntime Initialize()
        {
            if (_isInitialized)
                throw new InvalidOperationException("The boot manager has already been initialized");

            //ApplicationCache = Container.GetInstance<CacheHelper>(); //GetApplicationCache();

            _timer = ProfilingLogger.TraceDuration<CoreRuntime>(
                string.Format("Umbraco {0} application starting on {1}", UmbracoVersion.GetSemanticVersion().ToSemanticString(), NetworkHelper.MachineName),
                "Umbraco application startup complete");


            // register
            //Compose1(Container);

            //TODO: Remove these for v8!
            LegacyPropertyEditorIdToAliasConverter.CreateMappingsForCoreEditors();
            LegacyParameterEditorAliasConverter.CreateMappingsForCoreEditors();

            InitializeModelMappers();

            //now we need to call the initialize methods
            //Create a 'child'container which is a copy of all of the current registrations and begin a sub scope for it
            // this child container will be used to manage the application event handler instances and the scope will be
            // completed at the end of the boot process to allow garbage collection
            // using (Container.BeginScope()) { } // fixme - throws
            _appStartupEvtContainer = Container.Clone(); // fixme - but WHY clone? because *then* it has its own scope?! bah ;-(
            //using (_appStartupEvtContainer.BeginScope()) // fixme - works wtf?
            _appStartupEvtContainer.BeginScope(); // fixme - but then attend to end a scope before all child scopes are completed wtf?!
            _appStartupEvtContainer.RegisterCollection<PerScopeLifetime>(factory => factory.GetInstance<PluginManager>().ResolveApplicationStartupHandlers());

            // fixme - parallel? what about our dependencies?
            Parallel.ForEach(_appStartupEvtContainer.GetAllInstances<IApplicationEventHandler>(), x =>
            {
                try
                {
                    using (ProfilingLogger.DebugDuration<CoreRuntime>(
                        $"Executing {x.GetType()} in ApplicationInitialized",
                        $"Executed {x.GetType()} in ApplicationInitialized",
                        //only log if more than 150ms
                        150))
                    {
                        x.OnApplicationInitialized(UmbracoApplication, Current.ApplicationContext);
                    }
                }
                catch (Exception ex)
                {
                    ProfilingLogger.Logger.Error<CoreRuntime>("An error occurred running OnApplicationInitialized for handler " + x.GetType(), ex);
                    throw;
                }
            });

            _isInitialized = true;

            return this;
        }


        /// <summary>
        /// Fires after initialization and calls the callback to allow for customizations to occur &
        /// Ensure that the OnApplicationStarting methods of the IApplicationEvents are called
        /// </summary>
        /// <param name="afterStartup"></param>
        /// <returns></returns>
        public virtual IRuntime Startup(Action<ApplicationContext> afterStartup)
        {
            if (_isStarted)
                throw new InvalidOperationException("The boot manager has already been initialized");

            //call OnApplicationStarting of each application events handler
            Parallel.ForEach(_appStartupEvtContainer.GetAllInstances<IApplicationEventHandler>(), x =>
            {
                try
                {
                    using (ProfilingLogger.DebugDuration<CoreRuntime>(
                        $"Executing {x.GetType()} in ApplicationStarting",
                        $"Executed {x.GetType()} in ApplicationStarting",
                        //only log if more than 150ms
                        150))
                    {
                        x.OnApplicationStarting(UmbracoApplication, Current.ApplicationContext);
                    }
                }
                catch (Exception ex)
                {
                    ProfilingLogger.Logger.Error<CoreRuntime>("An error occurred running OnApplicationStarting for handler " + x.GetType(), ex);
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
        public virtual IRuntime Complete(Action<ApplicationContext> afterComplete)
        {
            if (_isComplete)
                throw new InvalidOperationException("The boot manager has already been completed");

            FreezeResolution();

            //Here we need to make sure the db can be connected to
		    EnsureDatabaseConnection();


            //This is a special case for the user service, we need to tell it if it's an upgrade, if so we need to ensure that
            // exceptions are bubbled up if a user is attempted to be persisted during an upgrade (i.e. when they auth to login)
            ((UserService) Current.ApplicationContext.Services.UserService).IsUpgrading = true;



            //call OnApplicationStarting of each application events handler
            Parallel.ForEach(_appStartupEvtContainer.GetAllInstances<IApplicationEventHandler>(), x =>
            {
                try
                {
                    using (ProfilingLogger.DebugDuration<CoreRuntime>(
                        $"Executing {x.GetType()} in ApplicationStarted",
                        $"Executed {x.GetType()} in ApplicationStarted",
                        //only log if more than 150ms
                        150))
                    {
                        x.OnApplicationStarted(UmbracoApplication, Current.ApplicationContext);
                    }
                }
                catch (Exception ex)
                {
                    ProfilingLogger.Logger.Error<CoreRuntime>("An error occurred running OnApplicationStarted for handler " + x.GetType(), ex);
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
            Current.ApplicationContext.IsReady = true;

            //stop the timer and log the output
            _timer.Dispose();
            return this;
		}

        protected virtual void FreezeResolution()
        {
        }
    }
}
