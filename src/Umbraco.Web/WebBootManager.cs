using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Routing;
using ClientDependency.Core.Config;
using Examine;
using LightInject;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Macros;
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
using Umbraco.Core.Events;
using Umbraco.Core.Cache;
using Umbraco.Core.Services;
using Umbraco.Web.Services;
using Umbraco.Web.Editors;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services.Changes;
using Umbraco.Web.Cache;
using Umbraco.Web.DependencyInjection;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.HealthCheck.Checks.DataIntegrity;
using Umbraco.Web._Legacy.Actions;
using UmbracoExamine;
using Action = System.Action;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;
using ProfilingViewEngine = Umbraco.Core.Profiling.ProfilingViewEngine;
using TypeHelper = Umbraco.Core.Plugins.TypeHelper;


namespace Umbraco.Web
{
    /// <summary>
    /// A bootstrapper for the Umbraco application which initializes all objects including the Web portion of the application
    /// </summary>
    public class WebBootManager : CoreBootManager
    {
        //TODO: Fix this - we need to manually perform re-indexing on startup when necessary Examine lib no longer does this
        //NOTE: see the Initialize method for what this is used for
        //private static readonly List<BaseIndexProvider> IndexesToRebuild = new List<BaseIndexProvider>();

        public WebBootManager(UmbracoApplicationBase umbracoApplication)
            : base(umbracoApplication)
        { }

        /// <summary>
        /// Constructor for unit tests, ensures some resolvers are not initialized
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="logger"></param>
        /// <param name="isForTesting"></param>
        internal WebBootManager(UmbracoApplicationBase umbracoApplication, ProfilingLogger logger, bool isForTesting)
            : base(umbracoApplication, logger)
        { }

        /// <summary>
        /// Initialize objects before anything during the boot cycle happens
        /// </summary>
        /// <returns></returns>
        public override IBootManager Initialize()
        {
            //TODO: Fix this - we need to manually perform re-indexing on startup when necessary Examine lib no longer does this
            ////This is basically a hack for this item: http://issues.umbraco.org/issue/U4-5976
            // // when Examine initializes it will try to rebuild if the indexes are empty, however in many cases not all of Examine's
            // // event handlers will be assigned during bootup when the rebuilding starts which is a problem. So with the examine 0.1.58.2941 build
            // // it has an event we can subscribe to in order to cancel this rebuilding process, but what we'll do is cancel it and postpone the rebuilding until the
            // // boot process has completed. It's a hack but it works.
            //ExamineManager.Instance.BuildingEmptyIndexOnStartup += OnInstanceOnBuildingEmptyIndexOnStartup;

            base.Initialize();

            //setup mvc and webapi services
            SetupMvcAndWebApi();

            // Backwards compatibility - set the path and URL type for ClientDependency 1.5.1 [LK]
            ClientDependency.Core.CompositeFiles.Providers.XmlFileMapper.FileMapVirtualFolder = "~/App_Data/TEMP/ClientDependency";
            ClientDependency.Core.CompositeFiles.Providers.BaseCompositeFileProcessingProvider.UrlTypeDefault = ClientDependency.Core.CompositeFiles.Providers.CompositeUrlType.Base64QueryStrings;

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

            // Disable the X-AspNetMvc-Version HTTP Header
            MvcHandler.DisableMvcResponseHeader = true;

            InstallHelper.DeleteLegacyInstaller();

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
                httpContext, ApplicationContext,
                Current.FacadeService,
                new WebSecurity(httpContext, ApplicationContext),
                UmbracoConfig.For.UmbracoSettings(),
                Current.UrlProviders,
                false);
        }

        /// <summary>
        /// Ensure the current profiler is the web profiler
        /// </summary>
        protected override IProfiler CreateProfiler()
        {
            return new WebProfiler();
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

            //rebuild any empty indexes
            //TODO: Do we want to make this optional? otherwise the only way to disable this on startup
            // would be to implement a custom WebBootManager and override this method
            RebuildIndexes(true);

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

        private void RoutePluginControllers()
        {
            var umbracoPath = GlobalSettings.UmbracoMvcArea;

            //we need to find the plugin controllers and route them
            var pluginControllers = Current.SurfaceControllerTypes
                .Concat(Current.UmbracoApiControllerTypes)
                .ToArray();

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
            var pluginSurfaceControlleres = pluginControllers.Where(x => PluginController.GetMetadata(x).AreaName.IsNullOrWhiteSpace() == false);
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
        /// Build the core container which contains all core things requird to build an app context
        /// </summary>
        internal override void ConfigureCoreServices(ServiceContainer container)
        {
            base.ConfigureCoreServices(container);

            // register model mappers
            container.RegisterFrom<WebModelMappersCompositionRoot>();

            // support web request scope
            // note: everything that is PerRequestLifeTime will be disposed by LightInject at the end of the request
            container.EnablePerWebRequestScope();

            // register the http context and umbraco context accessors
            // we *should* use the HttpContextUmbracoContextAccessor, however there are cases when
            // we have no http context, eg when booting Umbraco or in background threads, so instead
            // let's use an hybrid accessor that can fall back to a ThreadStatic context.
            container.RegisterSingleton<IHttpContextAccessor, AspNetHttpContextAccessor>(); // replaces HttpContext.Current
            container.RegisterSingleton<IUmbracoContextAccessor, HybridUmbracoContextAccessor>();

            // register a per-request HttpContextBase object
            // is per-request so only one wrapper is created per request
            container.Register<HttpContextBase>(factory => new HttpContextWrapper(factory.GetInstance<IHttpContextAccessor>().HttpContext), new PerRequestLifeTime());

            // register the facade accessor - the "current" facade is in the umbraco context
            container.RegisterSingleton<IFacadeAccessor, UmbracoContextFacadeAccessor>();

            // register the umbraco database accessor
            // have to use the hybrid thing...
            container.RegisterSingleton<IUmbracoDatabaseAccessor, HybridUmbracoDatabaseAccessor>();

            // register the XML facade service
            //container.RegisterSingleton<IFacadeService>(factory => new PublishedCache.XmlPublishedCache.FacadeService(
            //    factory.GetInstance<ServiceContext>(),
            //    factory.GetInstance<IDatabaseUnitOfWorkProvider>(),
            //    factory.GetInstance<CacheHelper>().RequestCache,
            //    factory.GetAllInstances<IUrlSegmentProvider>(),
            //    factory.GetInstance<IFacadeAccessor>()));

            // register the NuCache facade service
            container.RegisterSingleton<IFacadeService>(factory => new PublishedCache.NuCache.FacadeService(
                new PublishedCache.NuCache.FacadeService.Options { FacadeCacheIsApplicationRequestCache = true },
                factory.GetInstance<ApplicationContext>().MainDom,
                factory.GetInstance<ServiceContext>(),
                factory.GetInstance<IDatabaseUnitOfWorkProvider>(),
                factory.GetInstance<IFacadeAccessor>(),
                factory.GetInstance<ILogger>()));

            // register a per-request UmbracoContext object
            // no real need to be per request but assuming it is faster
            container.Register(factory => factory.GetInstance<IUmbracoContextAccessor>().UmbracoContext, new PerRequestLifeTime());

            // register the umbraco helper
            container.RegisterSingleton<UmbracoHelper>();

            // replace some services
            container.RegisterSingleton<IEventMessagesFactory, DefaultEventMessagesFactory>();
            container.RegisterSingleton<IEventMessagesAccessor, HybridEventMessagesAccessor>();
            container.RegisterSingleton<IApplicationTreeService, ApplicationTreeService>();
            container.RegisterSingleton<ISectionService, SectionService>();

            container.RegisterSingleton<IExamineIndexCollectionAccessor, ExamineIndexCollectionAccessor>();
        }

        /// <summary>
        /// Called to customize the IoC container
        /// </summary>
        /// <param name="container"></param>
        internal override void ConfigureApplicationServices(ServiceContainer container)
        {
            base.ConfigureApplicationServices(container);

            // IoC setup for LightInject for MVC/WebApi
            Container.EnableMvc();
            Container.RegisterMvcControllers(PluginManager, GetType().Assembly);
            container.EnableWebApi(GlobalConfiguration.Configuration);
            container.RegisterApiControllers(PluginManager, GetType().Assembly);
        }

        /// <summary>
        /// Initializes all web based and core resolves
        /// </summary>
        protected override void InitializeResolvers()
        {
            base.InitializeResolvers();

            XsltExtensionCollectionBuilder.Register(Container)
                .AddExtensionObjectProducer(() => PluginManager.ResolveXsltExtensions());

            EditorValidatorCollectionBuilder.Register(Container)
                .AddProducer(() => PluginManager.ResolveTypes<IEditorValidator>());

            // set the default RenderMvcController
            Current.DefaultRenderMvcControllerType = typeof (RenderMvcController); // fixme WRONG!

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
                        ProfilingLogger.Logger.Error<WebBootManager>("An error occurred trying to set the IServerMessenger during application startup", e);
                            return null;
                        }
                    }
                ProfilingLogger.Logger.Warn<WebBootManager>("Could not initialize the DefaultServerMessenger, the application is not configured or the database is not configured");
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
                            () =>
                            {
                                // rebuild the facade caches entirely, if the server is not synced
                                // this is equivalent to DistributedCache RefreshAllFacade but local only
                                // (we really should have a way to reuse RefreshAllFacade... locally)
                                // note: refresh all content & media caches does refresh content types too
    					        IFacadeService svc = Current.FacadeService;
                                bool ignored1, ignored2;
                                svc.Notify(new[] { new DomainCacheRefresher.JsonPayload(0, DomainChangeTypes.RefreshAll) });
                                svc.Notify(new[] { new ContentCacheRefresher.JsonPayload(0, TreeChangeTypes.RefreshAll) }, out ignored1, out ignored2);
                                svc.Notify(new[] { new MediaCacheRefresher.JsonPayload(0, TreeChangeTypes.RefreshAll) }, out ignored1);
                            },
                            //rebuild indexes if the server is not synced
                            // NOTE: This will rebuild ALL indexes including the members, if developers want to target specific
                            // indexes then they can adjust this logic themselves.
                            () => RebuildIndexes(false)
                        }
                    }));
            }

            ActionCollectionBuilder.Register(Container)
                .SetProducer(() => PluginManager.ResolveActions());

            var surfaceControllerTypes = new SurfaceControllerTypeCollection(PluginManager.ResolveSurfaceControllers());
            Container.RegisterInstance(surfaceControllerTypes);

            var umbracoApiControllerTypes = new UmbracoApiControllerTypeCollection(PluginManager.ResolveUmbracoApiControllers());
            Container.RegisterInstance(umbracoApiControllerTypes);

            // both TinyMceValueConverter (in Core) and RteMacroRenderingValueConverter (in Web) will be
            // discovered when CoreBootManager configures the converters. We HAVE to remove one of them
            // here because there cannot be two converters for one property editor - and we want the full
            // RteMacroRenderingValueConverter that converts macros, etc. So remove TinyMceValueConverter.
            // (the limited one, defined in Core, is there for tests) - same for others
            Container.GetInstance<PropertyValueConverterCollectionBuilder>()
                .Remove<TinyMceValueConverter>()
                .Remove<TextStringValueConverter>()
                .Remove<MarkdownEditorValueConverter>()
                .Remove<ImageCropperValueConverter>();

            // add all known factories, devs can then modify this list on application
            // startup either by binding to events or in their own global.asax
            FilteredControllerFactoryCollectionBuilder.Register(Container)
                .Append<RenderControllerFactory>();

            UrlProviderCollectionBuilder.Register(Container)
                //.Append<AliasUrlProvider>() // not enabled by default
                .Append<DefaultUrlProvider>()
                .Append<CustomRouteUrlProvider>();

            Container.Register<IContentLastChanceFinder, ContentFinderByLegacy404>();

            ContentFinderCollectionBuilder.Register(Container)
                // all built-in finders in the correct order,
                // devs can then modify this list on application startup
                .Append<ContentFinderByPageIdQuery>()
                .Append<ContentFinderByNiceUrl>()
                .Append<ContentFinderByIdPath>()
                .Append<ContentFinderByNiceUrlAndTemplate>()
                .Append<ContentFinderByProfile>()
                .Append<ContentFinderByUrlAlias>()
                .Append<ContentFinderByRedirectUrl>();

            Container.Register<ISiteDomainHelper, SiteDomainHelper>();

            ThumbnailProviderCollectionBuilder.Register(Container)
                .Add(PluginManager.ResolveThumbnailProviders());

            ImageUrlProviderCollectionBuilder.Register(Container)
                .Append(PluginManager.ResolveImageUrlProviders());

            CultureDictionaryFactoryResolver.Current = new CultureDictionaryFactoryResolver(Container);
            Container.Register<ICultureDictionaryFactory, DefaultCultureDictionaryFactory>();

            HealthCheckCollectionBuilder.Register(Container)
                .AddProducer(() => PluginManager.ResolveTypes<HealthCheck.HealthCheck>())
                .Exclude<XmlDataIntegrityHealthCheck>(); // fixme must remove else NuCache dies!
            // but we should also have one for NuCache AND NuCache should be a component that does all this
        }

        /// <summary>
        /// Sets up MVC/WebApi services
        /// </summary>
        private void SetupMvcAndWebApi()
        {
            //don't output the MVC version header (security)
            MvcHandler.DisableMvcResponseHeader = true;

            // set master controller factory
            var controllerFactory = new MasterControllerFactory(() => Current.FilteredControllerFactories);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            // set the render & plugin view engines
            ViewEngines.Engines.Add(new RenderViewEngine());
            ViewEngines.Engines.Add(new PluginViewEngine());

            //set model binder
            ModelBinderProviders.BinderProviders.Add(new ContentModelBinder()); // is a provider

            ////add the profiling action filter
            //GlobalFilters.Filters.Add(new ProfilingActionFilter());

            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector),
                new NamespaceHttpControllerSelector(GlobalConfiguration.Configuration));
        }

        protected virtual void RebuildIndexes(bool onlyEmptyIndexes)
        {
            if (ApplicationContext.IsConfigured == false || ApplicationContext.DatabaseContext.IsDatabaseConfigured == false)
            {
                return;
            }

            foreach (var indexer in ExamineManager.Instance.IndexProviders)
            {
                if (onlyEmptyIndexes == false || indexer.Value.IsIndexNew())
                {
                    indexer.Value.RebuildIndex();
                }
            }

        }

    }
}

