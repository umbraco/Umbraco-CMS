using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Routing;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Config;
using Examine.Providers;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Macros;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Profiling;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Sync;
using Umbraco.Web.Dictionary;
using Umbraco.Web.Install;
using Umbraco.Web.Media;
using Umbraco.Web.Media.ThumbnailProviders;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.UI.JavaScript;
using Umbraco.Web.WebApi;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;
using Umbraco.Web.Features;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Profiling;
using Umbraco.Web.Search;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;
using ProfilingViewEngine = Umbraco.Core.Profiling.ProfilingViewEngine;


namespace Umbraco.Web
{
    /// <summary>
    /// A bootstrapper for the Umbraco application which initializes all objects including the Web portion of the application
    /// </summary>
    public class WebBootManager : CoreBootManager
    {
        private readonly bool _isForTesting;

        //needs to be static because we have the public static method GetIndexesForColdBoot
        private static ExamineStartup _examineStartup;

        public WebBootManager(UmbracoApplicationBase umbracoApplication)
            : base(umbracoApplication)
        {
            _isForTesting = false;
        }

        /// <summary>
        /// Constructor for unit tests, ensures some resolvers are not initialized
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="logger"></param>
        /// <param name="isForTesting"></param>
        internal WebBootManager(UmbracoApplicationBase umbracoApplication, ProfilingLogger logger, bool isForTesting)
            : base(umbracoApplication, logger)
        {
            _isForTesting = isForTesting;
        }

        /// <summary>
        /// Creates and returns the service context for the app
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="scopeProvider"></param>
        /// <returns></returns>
        protected override ServiceContext CreateServiceContext(DatabaseContext dbContext, IScopeProvider scopeProvider)
        {
            //use a request based messaging factory
            var evtMsgs = new ScopeLifespanMessagesFactory(new SingletonHttpContextAccessor(), scopeProvider);
            return new ServiceContext(
                new RepositoryFactory(ApplicationCache, ProfilingLogger.Logger, dbContext.SqlSyntax, UmbracoConfig.For.UmbracoSettings()),
                new PetaPocoUnitOfWorkProvider(scopeProvider),
                ApplicationCache,
                ProfilingLogger.Logger,
                evtMsgs);
        }

        /// <summary>
        /// Initialize objects before anything during the boot cycle happens
        /// </summary>
        /// <returns></returns>
        public override IBootManager Initialize()
        {
            base.Initialize();

            _examineStartup = new ExamineStartup(ApplicationContext);
            _examineStartup.Initialize();

            ConfigureClientDependency();

            //set master controller factory
            ControllerBuilder.Current.SetControllerFactory(
                new MasterControllerFactory(FilteredControllerFactoriesResolver.Current));

            //set the render view engine
            ViewEngines.Engines.Add(new RenderViewEngine());
            //set the plugin view engine
            ViewEngines.Engines.Add(new PluginViewEngine());

            //set model binder
            ModelBinderProviders.BinderProviders.Add(RenderModelBinder.Instance); // is a provider

            ////add the profiling action filter
            //GlobalFilters.Filters.Add(new ProfilingActionFilter());

            // Disable the X-AspNetMvc-Version HTTP Header
            MvcHandler.DisableMvcResponseHeader = true;

            InstallHelper insHelper = new InstallHelper(UmbracoContext.Current);
            insHelper.DeleteLegacyInstaller();

            // Tell .net 4.5 that we also want to try connecting over TLS 1.2, not just 1.1
            if (ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls12) == false)
                ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | SecurityProtocolType.Tls12;

            return this;
        }

        /// <summary>
        /// Override this method in order to ensure that the UmbracoContext is also created, this can only be
        /// created after resolution is frozen!
        /// </summary>
        protected override void FreezeResolution()
        {
            base.FreezeResolution();

            //before we do anything, we'll ensure the umbraco context
            //see: http://issues.umbraco.org/issue/U4-1717
            var httpContext = new HttpContextWrapper(UmbracoApplication.Context);
            UmbracoContext.EnsureContext(
                httpContext,
                ApplicationContext,
                new WebSecurity(httpContext, ApplicationContext),
                UmbracoConfig.For.UmbracoSettings(),
                UrlProviderResolver.Current.Providers,
                false);
        }

        /// <summary>
        /// Ensure the current profiler is the web profiler
        /// </summary>
        protected override void InitializeProfilerResolver()
        {
            base.InitializeProfilerResolver();
            //Set the profiler to be the web profiler
            var profiler = new WebProfiler();
            ProfilerResolver.Current.SetProfiler(profiler);
            profiler.Start();
        }

        /// <summary>
        /// Ensure that the OnApplicationStarted methods of the IApplicationEvents are called
        /// </summary>
        /// <param name="afterComplete"></param>
        /// <returns></returns>
        public override IBootManager Complete(Action<ApplicationContext> afterComplete)
        {
            //Wrap viewengines in the profiling engine
            WrapViewEngines(ViewEngines.Engines);

            //add global filters
            ConfigureGlobalFilters();

            //set routes
            CreateRoutes();

            base.Complete(afterComplete);

            //Now, startup all of our legacy startup handler
            ApplicationEventsResolver.Current.InstantiateLegacyStartupHandlers();

            _examineStartup.Complete();

            //Now ensure webapi is initialized after everything
            GlobalConfiguration.Configuration.EnsureInitialized();

            return this;
        }



        internal static void ConfigureGlobalFilters()
        {
            GlobalFilters.Filters.Add(new EnsurePartialViewMacroViewContextFilterAttribute());
        }

        internal static void WrapViewEngines(IList<IViewEngine> viewEngines)
        {
            if (viewEngines == null || viewEngines.Count == 0) return;

            var originalEngines = viewEngines.Select(e => e).ToArray();
            viewEngines.Clear();
            foreach (var engine in originalEngines)
            {
                var wrappedEngine = engine is ProfilingViewEngine ? engine : new ProfilingViewEngine(engine);
                viewEngines.Add(wrappedEngine);
            }
        }

        /// <summary>
        /// Creates the application cache based on the HttpRuntime cache
        /// </summary>
        protected override CacheHelper CreateApplicationCache()
        {
            //create a web-based cache helper
            var cacheHelper = new CacheHelper(
                //we need to have the dep clone runtime cache provider to ensure
                //all entities are cached properly (cloned in and cloned out)
                new DeepCloneRuntimeCacheProvider(new HttpRuntimeCacheProvider(HttpRuntime.Cache)),
                new StaticCacheProvider(),
                //we need request based cache when running in web-based context
                new HttpRequestCacheProvider(),
                new IsolatedRuntimeCache(type =>
                    //we need to have the dep clone runtime cache provider to ensure
                    //all entities are cached properly (cloned in and cloned out)
                    new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider())));

            return cacheHelper;
        }

        /// <summary>
        /// Creates the routes
        /// </summary>
        protected internal void CreateRoutes()
        {
            var umbracoPath = GlobalSettings.UmbracoMvcArea;

            //Create the front-end route
            var defaultRoute = RouteTable.Routes.MapRoute(
                "Umbraco_default",
                umbracoPath + "/RenderMvc/{action}/{id}",
                new { controller = "RenderMvc", action = "Index", id = UrlParameter.Optional }
                );
            defaultRoute.RouteHandler = new RenderRouteHandler(ControllerBuilder.Current.GetControllerFactory());

            //register install routes
            RouteTable.Routes.RegisterArea<UmbracoInstallArea>();

            //register all back office routes
            RouteTable.Routes.RegisterArea<BackOfficeArea>();

            //plugin controllers must come first because the next route will catch many things
            RoutePluginControllers();
        }

        private void ConfigureClientDependency()
        {
            // Backwards compatibility - set the path and URL type for ClientDependency 1.5.1 [LK]
            XmlFileMapper.FileMapDefaultFolder = "~/App_Data/TEMP/ClientDependency";
            BaseCompositeFileProcessingProvider.UrlTypeDefault = CompositeUrlType.Base64QueryStrings;

            // Now we need to detect if we are running umbracoLocalTempStorage as EnvironmentTemp and in that case we want to change the CDF file
            // location to be there
            if (GlobalSettings.LocalTempStorageLocation == LocalTempStorage.EnvironmentTemp)
            {
                var appDomainHash = HttpRuntime.AppDomainAppId.ToSHA1();
                var cachePath = Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "UmbracoData",
                    //include the appdomain hash is just a safety check, for example if a website is moved from worker A to worker B and then back
                    // to worker A again, in theory the %temp%  folder should already be empty but we really want to make sure that its not
                    // utilizing an old path
                    appDomainHash);
                
                //set the file map and composite file default location to the %temp% location
                BaseCompositeFileProcessingProvider.CompositeFilePathDefaultFolder
                    = XmlFileMapper.FileMapDefaultFolder
                    = Path.Combine(cachePath, "ClientDependency");
            }

            var section = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
            if (section != null)
            {
                //set the max url length for CDF to be the smallest of the max query length, max request length
                ClientDependency.Core.CompositeFiles.CompositeDependencyHandler.MaxHandlerUrlLength = Math.Min(section.MaxQueryStringLength, section.MaxRequestLength);
            }

            //Register a custom renderer - used to process property editor dependencies
            var renderer = new DependencyPathRenderer();
            renderer.Initialize("Umbraco.DependencyPathRenderer", new NameValueCollection
            {
                { "compositeFileHandlerPath", ClientDependencySettings.Instance.CompositeFileHandlerPath }
            });
            ClientDependencySettings.Instance.MvcRendererCollection.Add(renderer);
        }

        private void RoutePluginControllers()
        {
            var umbracoPath = GlobalSettings.UmbracoMvcArea;

            //we need to find the plugin controllers and route them
            var pluginControllers =
                SurfaceControllerResolver.Current.RegisteredSurfaceControllers.Concat(
                    UmbracoApiControllerResolver.Current.RegisteredUmbracoApiControllers).ToArray();

            //local controllers do not contain the attribute
            var localControllers = pluginControllers.Where(x => PluginController.GetMetadata(x).AreaName.IsNullOrWhiteSpace());
            foreach (var s in localControllers)
            {
                if (TypeHelper.IsTypeAssignableFrom<SurfaceController>(s))
                {
                    RouteLocalSurfaceController(s, umbracoPath);
                }
                else if (TypeHelper.IsTypeAssignableFrom<UmbracoApiController>(s))
                {
                    RouteLocalApiController(s, umbracoPath);
                }
            }

            //need to get the plugin controllers that are unique to each area (group by)
            var pluginSurfaceControlleres = pluginControllers.Where(x => !PluginController.GetMetadata(x).AreaName.IsNullOrWhiteSpace());
            var groupedAreas = pluginSurfaceControlleres.GroupBy(controller => PluginController.GetMetadata(controller).AreaName);
            //loop through each area defined amongst the controllers
            foreach (var g in groupedAreas)
            {
                //create an area for the controllers (this will throw an exception if all controllers are not in the same area)
                var pluginControllerArea = new PluginControllerArea(g.Select(PluginController.GetMetadata));
                //register it
                RouteTable.Routes.RegisterArea(pluginControllerArea);
            }
        }

        private void RouteLocalApiController(Type controller, string umbracoPath)
        {
            var meta = PluginController.GetMetadata(controller);

            //url to match
            var routePath = meta.IsBackOffice == false
                                ? umbracoPath + "/Api/" + meta.ControllerName + "/{action}/{id}"
                                : umbracoPath + "/BackOffice/Api/" + meta.ControllerName + "/{action}/{id}";

            var route = RouteTable.Routes.MapHttpRoute(
                string.Format("umbraco-{0}-{1}", "api", meta.ControllerName),
                routePath,
                new { controller = meta.ControllerName, id = UrlParameter.Optional },
                new[] { meta.ControllerNamespace });
            //web api routes don't set the data tokens object
            if (route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }
            route.DataTokens.Add(Core.Constants.Web.UmbracoDataToken, "api"); //ensure the umbraco token is set
        }

        private void RouteLocalSurfaceController(Type controller, string umbracoPath)
        {
            var meta = PluginController.GetMetadata(controller);
            var route = RouteTable.Routes.MapRoute(
                string.Format("umbraco-{0}-{1}", "surface", meta.ControllerName),
                umbracoPath + "/Surface/" + meta.ControllerName + "/{action}/{id}",//url to match
                new { controller = meta.ControllerName, action = "Index", id = UrlParameter.Optional },
                new[] { meta.ControllerNamespace }); //look in this namespace to create the controller
            route.DataTokens.Add(Core.Constants.Web.UmbracoDataToken, "surface"); //ensure the umbraco token is set
            route.DataTokens.Add("UseNamespaceFallback", false); //Don't look anywhere else except this namespace!
            //make it use our custom/special SurfaceMvcHandler
            route.RouteHandler = new SurfaceRouteHandler();
        }

        /// <summary>
        /// Initializes all web based and core resolves
        /// </summary>
        protected override void InitializeResolvers()
        {
            base.InitializeResolvers();

            FeaturesResolver.Current = new FeaturesResolver(new UmbracoFeatures());

            TourFilterResolver.Current = new TourFilterResolver(ServiceProvider, LoggerResolver.Current.Logger);

            SearchableTreeResolver.Current = new SearchableTreeResolver(ServiceProvider, LoggerResolver.Current.Logger, ApplicationContext.Services.ApplicationTreeService, () => PluginManager.ResolveSearchableTrees());

            XsltExtensionsResolver.Current = new XsltExtensionsResolver(ServiceProvider, LoggerResolver.Current.Logger, () => PluginManager.ResolveXsltExtensions());

            EditorValidationResolver.Current = new EditorValidationResolver(ServiceProvider, LoggerResolver.Current.Logger, () => PluginManager.ResolveTypes<IEditorValidator>());

            //set the default RenderMvcController
            DefaultRenderMvcControllerResolver.Current = new DefaultRenderMvcControllerResolver(typeof(RenderMvcController));

            //Override the default server messenger, we need to check if the legacy dist calls is enabled, if that is the
            // case, then we'll set the default messenger to be the old one, otherwise we'll set it to the db messenger
            // which will always be on.
            if (UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled)
            {
                //set the legacy one by default - this maintains backwards compat
                ServerMessengerResolver.Current.SetServerMessenger(new BatchedWebServiceServerMessenger(() =>
                {
                    //we should not proceed to change this if the app/database is not configured since there will
                    // be no user, plus we don't need to have server messages sent if this is the case.
                    if (ApplicationContext.IsConfigured && ApplicationContext.DatabaseContext.IsDatabaseConfigured)
                    {
                        //disable if they are not enabled
                        if (UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled == false)
                        {
                            return null;
                        }

                        try
                        {
                            var user = ApplicationContext.Services.UserService.GetUserById(UmbracoConfig.For.UmbracoSettings().DistributedCall.UserId);
                            return new Tuple<string, string>(user.Username, user.RawPasswordValue);
                        }
                        catch (Exception e)
                        {
                            LoggerResolver.Current.Logger.Error<WebBootManager>("An error occurred trying to set the IServerMessenger during application startup", e);
                            return null;
                        }
                    }
                    LoggerResolver.Current.Logger.Warn<WebBootManager>("Could not initialize the DefaultServerMessenger, the application is not configured or the database is not configured");
                    return null;
                }));
            }
            else
            {
                ServerMessengerResolver.Current.SetServerMessenger(new BatchedDatabaseServerMessenger(
                    ApplicationContext,
                    true,
                    //Default options for web including the required callbacks to build caches
                    new DatabaseServerMessengerOptions
                    {
                        //These callbacks will be executed if the server has not been synced
                        // (i.e. it is a new server or the lastsynced.txt file has been removed)
                        InitializingCallbacks = new Action[]
                        {
                            //rebuild the xml cache file if the server is not synced
                            () => global::umbraco.content.Instance.RefreshContentFromDatabase(),
                            //rebuild indexes if the server is not synced
                            // NOTE: This will rebuild ALL indexes including the members, if developers want to target specific
                            // indexes then they can adjust this logic themselves.
                            () => _examineStartup.RebuildIndexes()
                        }
                    }));
            }

            SurfaceControllerResolver.Current = new SurfaceControllerResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                PluginManager.ResolveSurfaceControllers());

            UmbracoApiControllerResolver.Current = new UmbracoApiControllerResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                PluginManager.ResolveUmbracoApiControllers());

            // both TinyMceValueConverter (in Core) and RteMacroRenderingValueConverter (in Web) will be
            // discovered when CoreBootManager configures the converters. We HAVE to remove one of them
            // here because there cannot be two converters for one property editor - and we want the full
            // RteMacroRenderingValueConverter that converts macros, etc. So remove TinyMceValueConverter.
            // (the limited one, defined in Core, is there for tests)
            PropertyValueConvertersResolver.Current.RemoveType<TinyMceValueConverter>();
            // same for other converters
            PropertyValueConvertersResolver.Current.RemoveType<Core.PropertyEditors.ValueConverters.TextStringValueConverter>();
            PropertyValueConvertersResolver.Current.RemoveType<Core.PropertyEditors.ValueConverters.MarkdownEditorValueConverter>();
            PropertyValueConvertersResolver.Current.RemoveType<Core.PropertyEditors.ValueConverters.ImageCropperValueConverter>();

            PublishedCachesResolver.Current = new PublishedCachesResolver(new PublishedCaches(
                new PublishedCache.XmlPublishedCache.PublishedContentCache(),
                new PublishedCache.XmlPublishedCache.PublishedMediaCache(ApplicationContext)));

            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector),
                new NamespaceHttpControllerSelector(GlobalConfiguration.Configuration));

            FilteredControllerFactoriesResolver.Current = new FilteredControllerFactoriesResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                // add all known factories, devs can then modify this list on application
                // startup either by binding to events or in their own global.asax
                new[]
                    {
                        typeof (RenderControllerFactory)
                    });

            UrlProviderResolver.Current = new UrlProviderResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                    //typeof(AliasUrlProvider), // not enabled by default
                    typeof(DefaultUrlProvider),
                    typeof(CustomRouteUrlProvider)
                );

            ContentLastChanceFinderResolver.Current = new ContentLastChanceFinderResolver(
                // handled by ContentLastChanceFinderByNotFoundHandlers for the time being
                // soon as we get rid of INotFoundHandler support, we must enable this
                //new ContentFinderByLegacy404()

                // implement INotFoundHandler support... remove once we get rid of it
                new ContentLastChanceFinderByNotFoundHandlers());

            ContentFinderResolver.Current = new ContentFinderResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                // all built-in finders in the correct order, devs can then modify this list
                // on application startup via an application event handler.
                typeof(ContentFinderByPageIdQuery),
                typeof(ContentFinderByNiceUrl),
                typeof(ContentFinderByIdPath),

                // these will be handled by ContentFinderByNotFoundHandlers so they can be enabled/disabled
                // via the config file... soon as we get rid of INotFoundHandler support, we must enable
                // them here.
                //typeof (ContentFinderByNiceUrlAndTemplate),
                //typeof (ContentFinderByProfile),
                //typeof (ContentFinderByUrlAlias),

                // note: that one should run *after* NiceUrlAndTemplate, UrlAlias... but at the moment
                // it cannot be done - just make sure to do it properly in v8!
                typeof(ContentFinderByRedirectUrl),

                // implement INotFoundHandler support... remove once we get rid of it
                typeof(ContentFinderByNotFoundHandlers)
            );

            SiteDomainHelperResolver.Current = new SiteDomainHelperResolver(new SiteDomainHelper());

            // ain't that a bit dirty?
            PublishedCache.XmlPublishedCache.PublishedContentCache.UnitTesting = _isForTesting;

            ThumbnailProvidersResolver.Current = new ThumbnailProvidersResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                () => PluginManager.ResolveThumbnailProviders());

            ImageUrlProviderResolver.Current = new ImageUrlProviderResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                () => PluginManager.ResolveImageUrlProviders());

            CultureDictionaryFactoryResolver.Current = new CultureDictionaryFactoryResolver(
                new DefaultCultureDictionaryFactory());

            HealthCheckResolver.Current = new HealthCheckResolver(LoggerResolver.Current.Logger,
                () => PluginManager.ResolveTypes<HealthCheck.HealthCheck>());
            HealthCheckNotificationMethodResolver.Current = new HealthCheckNotificationMethodResolver(LoggerResolver.Current.Logger,
                () => PluginManager.ResolveTypes<HealthCheck.NotificationMethods.IHealthCheckNotificatationMethod>());

            // Disable duplicate community health checks which appear in Our.Umbraco.HealtchChecks and Umbraco Core.
            // See this issue to understand why https://github.com/umbraco/Umbraco-CMS/issues/4174
            var disabledHealthCheckTypes = new[]
            {
                "Our.Umbraco.HealthChecks.Checks.Security.HstsCheck",
                "Our.Umbraco.HealthChecks.Checks.Security.TlsCheck"
            }.Select(TypeFinder.GetTypeByName).WhereNotNull();

            foreach (var type in disabledHealthCheckTypes)
            {
                HealthCheckResolver.Current.RemoveType(type);
            }
        }

        /// <summary>
        /// The method used to create indexes on a cold boot
        /// </summary>
        /// <remarks>
        /// A cold boot is when the server determines it will not (or cannot) process instructions in the cache table and
        /// will rebuild it's own caches itself.
        /// TODO: Does this method need to exist? Is it used publicly elsewhere (i.e. outside of core?)
        /// </remarks>
        public static IEnumerable<BaseIndexProvider> GetIndexesForColdBoot()
        {
            return _examineStartup.GetIndexesForColdBoot();
        }
    }
}

