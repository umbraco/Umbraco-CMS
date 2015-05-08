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
using umbraco;
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
using Umbraco.Web.Macros;
using Umbraco.Web.Media;
using Umbraco.Web.Media.ThumbnailProviders;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.PropertyEditors.ValueConverters;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.Scheduling;
using Umbraco.Web.UI.JavaScript;
using Umbraco.Web.WebApi;
using umbraco.BusinessLogic;
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
        //NOTE: see the Initialize method for what this is used for
        private readonly List<IIndexer> _indexesToRebuild = new List<IIndexer>(); 

        public WebBootManager(UmbracoApplicationBase umbracoApplication)
            : this(umbracoApplication, false)
        {

        }

        /// <summary>
        /// Constructor for unit tests, ensures some resolvers are not initialized
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="isForTesting"></param>
        internal WebBootManager(UmbracoApplicationBase umbracoApplication, bool isForTesting)
            : base(umbracoApplication)
        {
            _isForTesting = isForTesting;
        }

        /// <summary>
        /// Initialize objects before anything during the boot cycle happens
        /// </summary>
        /// <returns></returns>
        public override IBootManager Initialize()
        {
             //This is basically a hack for this item: http://issues.umbraco.org/issue/U4-5976
            // when Examine initializes it will try to rebuild if the indexes are empty, however in many cases not all of Examine's 
            // event handlers will be assigned during bootup when the rebuilding starts which is a problem. So with the examine 0.1.58.2941 build
            // it has an event we can subscribe to in order to cancel this rebuilding process, but what we'll do is cancel it and postpone the rebuilding until the
            // boot process has completed. It's a hack but it works.
            ExamineManager.Instance.BuildingEmptyIndexOnStartup += OnInstanceOnBuildingEmptyIndexOnStartup;

            base.Initialize();

            // Backwards compatibility - set the path and URL type for ClientDependency 1.5.1 [LK]
            ClientDependency.Core.CompositeFiles.Providers.XmlFileMapper.FileMapVirtualFolder = "~/App_Data/TEMP/ClientDependency";
            ClientDependency.Core.CompositeFiles.Providers.BaseCompositeFileProcessingProvider.UrlTypeDefault = ClientDependency.Core.CompositeFiles.Providers.CompositeUrlType.Base64QueryStrings;

            var section = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
            if (section != null)
            {
                //set the max url length for CDF to be the smallest of the max query length, max request length
                ClientDependency.Core.CompositeFiles.CompositeDependencyHandler.MaxHandlerUrlLength = Math.Min(section.MaxQueryStringLength, section.MaxRequestLength);
            }

            //set master controller factory
            ControllerBuilder.Current.SetControllerFactory(
                new MasterControllerFactory(FilteredControllerFactoriesResolver.Current));

            //set the render view engine
            ViewEngines.Engines.Add(new RenderViewEngine());
            //set the plugin view engine
            ViewEngines.Engines.Add(new PluginViewEngine());

            //set model binder
            ModelBinders.Binders.Add(new KeyValuePair<Type, IModelBinder>(typeof(RenderModel), new RenderModelBinder()));

            ////add the profiling action filter
            //GlobalFilters.Filters.Add(new ProfilingActionFilter());

            //Register a custom renderer - used to process property editor dependencies
            var renderer = new DependencyPathRenderer();
            renderer.Initialize("Umbraco.DependencyPathRenderer", new NameValueCollection
            {
                { "compositeFileHandlerPath", ClientDependencySettings.Instance.CompositeFileHandlerPath }
            });
            ClientDependencySettings.Instance.MvcRendererCollection.Add(renderer);
            

            InstallHelper insHelper = new InstallHelper(UmbracoContext.Current);
            insHelper.DeleteLegacyInstaller();

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
                new WebSecurity(httpContext, ApplicationContext));
        }

        /// <summary>
        /// Ensure the current profiler is the web profiler
        /// </summary>
        protected override void InitializeProfilerResolver()
        {
            base.InitializeProfilerResolver();

            //Set the profiler to be the web profiler
            ProfilerResolver.Current.SetProfiler(new WebProfiler());
        }

        /// <summary>
        /// Adds custom types to the ApplicationEventsResolver
        /// </summary>
        protected override void InitializeApplicationEventsResolver()
        {
            base.InitializeApplicationEventsResolver();
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

            //set routes
            CreateRoutes();

            base.Complete(afterComplete);

            //Now, startup all of our legacy startup handler
            ApplicationEventsResolver.Current.InstantiateLegacyStartupHandlers();
            
            //Ok, now that everything is complete we'll check if we've stored any references to index that need rebuilding and run them 
            // (see the initialize method for notes) - we'll ensure we remove the event handler too in case examine manager doesn't actually
            // initialize during startup, in which case we want it to rebuild the indexes itself.
            ExamineManager.Instance.BuildingEmptyIndexOnStartup -= OnInstanceOnBuildingEmptyIndexOnStartup;
            if (_indexesToRebuild.Any())
            {
                foreach (var indexer in _indexesToRebuild)
                {
                    indexer.RebuildIndex();
                }
            }

            //Now ensure webapi is initialized after everything
            GlobalConfiguration.Configuration.EnsureInitialized();

            return this;
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
        protected override void CreateApplicationCache()
        {
            //create a web-based cache helper
            ApplicationCache = new CacheHelper();
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
            route.DataTokens.Add("umbraco", "api"); //ensure the umbraco token is set
        }
        private void RouteLocalSurfaceController(Type controller, string umbracoPath)
        {
            var meta = PluginController.GetMetadata(controller);
            var route = RouteTable.Routes.MapRoute(
                string.Format("umbraco-{0}-{1}", "surface", meta.ControllerName),
                umbracoPath + "/Surface/" + meta.ControllerName + "/{action}/{id}",//url to match
                new { controller = meta.ControllerName, action = "Index", id = UrlParameter.Optional },
                new[] { meta.ControllerNamespace }); //look in this namespace to create the controller
            route.DataTokens.Add("umbraco", "surface"); //ensure the umbraco token is set                
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

            XsltExtensionsResolver.Current = new XsltExtensionsResolver(ServiceProvider, LoggerResolver.Current.Logger, () => PluginManager.Current.ResolveXsltExtensions());

            //set the default RenderMvcController
            DefaultRenderMvcControllerResolver.Current = new DefaultRenderMvcControllerResolver(typeof(RenderMvcController));

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
                        var user = User.GetUser(UmbracoConfig.For.UmbracoSettings().DistributedCall.UserId);
                        return new System.Tuple<string, string>(user.LoginName, user.GetPassword());
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

            SurfaceControllerResolver.Current = new SurfaceControllerResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                PluginManager.Current.ResolveSurfaceControllers());

            UmbracoApiControllerResolver.Current = new UmbracoApiControllerResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                PluginManager.Current.ResolveUmbracoApiControllers());

            // both TinyMceValueConverter (in Core) and RteMacroRenderingValueConverter (in Web) will be
            // discovered when CoreBootManager configures the converters. We HAVE to remove one of them
            // here because there cannot be two converters for one property editor - and we want the full
            // RteMacroRenderingValueConverter that converts macros, etc. So remove TinyMceValueConverter.
            // (the limited one, defined in Core, is there for tests)
            PropertyValueConvertersResolver.Current.RemoveType<TinyMceValueConverter>();
            // same for other converters
            PropertyValueConvertersResolver.Current.RemoveType<Core.PropertyEditors.ValueConverters.TextStringValueConverter>();
            PropertyValueConvertersResolver.Current.RemoveType<Core.PropertyEditors.ValueConverters.MarkdownEditorValueConverter>();

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

                // implement INotFoundHandler support... remove once we get rid of it
                typeof(ContentFinderByNotFoundHandlers)
            );

            SiteDomainHelperResolver.Current = new SiteDomainHelperResolver(new SiteDomainHelper());

            // ain't that a bit dirty?
            PublishedCache.XmlPublishedCache.PublishedContentCache.UnitTesting = _isForTesting;

            ThumbnailProvidersResolver.Current = new ThumbnailProvidersResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                PluginManager.Current.ResolveThumbnailProviders());

            ImageUrlProviderResolver.Current = new ImageUrlProviderResolver(
                ServiceProvider, LoggerResolver.Current.Logger,
                PluginManager.Current.ResolveImageUrlProviders());

            CultureDictionaryFactoryResolver.Current = new CultureDictionaryFactoryResolver(
                new DefaultCultureDictionaryFactory());
        }


        private void OnInstanceOnBuildingEmptyIndexOnStartup(object sender, BuildingEmptyIndexOnStartupEventArgs args)
        {
            //store the indexer that needs rebuilding because it's empty for when the boot process 
            // is complete and cancel this current event so the rebuild process doesn't start right now.
            args.Cancel = true;
            _indexesToRebuild.Add(args.Indexer);
        }
    }
}
