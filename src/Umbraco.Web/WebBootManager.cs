using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using StackExchange.Profiling.MVCHelpers;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Profiling;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Sync;
using Umbraco.Web.Dictionary;
using Umbraco.Web.Media;
using Umbraco.Web.Media.ThumbnailProviders;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.WebApi;
using umbraco.BusinessLogic;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.presentation.cache;


namespace Umbraco.Web
{
    /// <summary>
    /// A bootstrapper for the Umbraco application which initializes all objects including the Web portion of the application 
    /// </summary>
    public class WebBootManager : CoreBootManager
    {
        private readonly bool _isForTesting;

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
            base.Initialize();

            // Backwards compatibility - set the path and URL type for ClientDependency 1.5.1 [LK]
            ClientDependency.Core.CompositeFiles.Providers.XmlFileMapper.FileMapVirtualFolder = "~/App_Data/TEMP/ClientDependency";
            ClientDependency.Core.CompositeFiles.Providers.BaseCompositeFileProcessingProvider.UrlTypeDefault = ClientDependency.Core.CompositeFiles.Providers.CompositeUrlType.Base64QueryStrings;

            //set master controller factory
            ControllerBuilder.Current.SetControllerFactory(
                new MasterControllerFactory(FilteredControllerFactoriesResolver.Current));

            //set the render view engine
            ViewEngines.Engines.Add(new ProfilingViewEngine(new RenderViewEngine()));
            //set the plugin view engine
            ViewEngines.Engines.Add(new ProfilingViewEngine(new PluginViewEngine()));

            //set model binder
            ModelBinders.Binders.Add(new KeyValuePair<Type, IModelBinder>(typeof(RenderModel), new RenderModelBinder()));

            //add the profiling action filter
            GlobalFilters.Filters.Add(new ProfilingActionFilter());
            
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
            UmbracoContext.EnsureContext(new HttpContextWrapper(UmbracoApplication.Context), ApplicationContext);
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
            ApplicationEventsResolver.Current.AddType<CacheHelperExtensions.CacheHelperApplicationEventListener>();
            ApplicationEventsResolver.Current.AddType<LegacyScheduledTasks>();
            //We need to remove these types because we've obsoleted them and we don't want them executing:
            ApplicationEventsResolver.Current.RemoveType<global::umbraco.LibraryCacheRefresher>();
        }

        /// <summary>
        /// Ensure that the OnApplicationStarted methods of the IApplicationEvents are called
        /// </summary>
        /// <param name="afterComplete"></param>
        /// <returns></returns>
        public override IBootManager Complete(Action<ApplicationContext> afterComplete)
        {
            //set routes
            CreateRoutes();

            base.Complete(afterComplete);

            //Now, startup all of our legacy startup handler
            ApplicationEventsResolver.Current.InstantiateLegacyStartupHandlers();

            return this;
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

            //Create the install routes
            var installPackageRoute = RouteTable.Routes.MapRoute(
                "Umbraco_install_packages",
                "Install/PackageInstaller/{action}/{id}",
                new { controller = "InstallPackage", action = "Index", id = UrlParameter.Optional }
                );
            installPackageRoute.DataTokens.Add("area", umbracoPath);

            //Create the REST/web/script service routes
            var webServiceRoutes = RouteTable.Routes.MapRoute(
                "Umbraco_web_services",
                umbracoPath + "/RestServices/{controller}/{action}/{id}",
                new { controller = "SaveFileController", action = "Index", id = UrlParameter.Optional },
                //look in this namespace for controllers
                new string[] { "Umbraco.Web.WebServices" }
                );
            webServiceRoutes.DataTokens.Add("area", umbracoPath);

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
            var route = RouteTable.Routes.MapHttpRoute(
                string.Format("umbraco-{0}-{1}", "api", meta.ControllerName),
                umbracoPath + "/Api/" + meta.ControllerName + "/{action}/{id}",//url to match
                new { controller = meta.ControllerName, id = UrlParameter.Optional },
                new { controller = @"(\w+)Api" }); //Must be suffixed with "Api" (i.e. MyApiController)
            //web api routes don't set the data tokens object
            if (route.DataTokens == null)
            {                
                route.DataTokens = new RouteValueDictionary();
            }
            route.DataTokens.Add("Namespaces", new[] {meta.ControllerNamespace}); //look in this namespace to create the controller
            route.DataTokens.Add("umbraco", "api"); //ensure the umbraco token is set
        }

        private void RouteLocalSurfaceController(Type controller, string umbracoPath)
        {
            var meta = PluginController.GetMetadata(controller);
            var route = RouteTable.Routes.MapRoute(
                string.Format("umbraco-{0}-{1}", "surface", meta.ControllerName),
                umbracoPath + "/Surface/" + meta.ControllerName + "/{action}/{id}",//url to match
                new { controller = meta.ControllerName, action = "Index", id = UrlParameter.Optional },
                //NOTE: There SHOULD be a constraint here to only match controllers with a "SurfaceController" suffix but we forgot to include it in the 
                // 4.10 release so we can't include it now :(
                new[] { meta.ControllerNamespace }); //look in this namespace to create the controller
            route.DataTokens.Add("umbraco", "surface"); //ensure the umbraco token is set                
            //make it use our custom/special SurfaceMvcHandler
            route.RouteHandler = new SurfaceRouteHandler();
        }

        /// <summary>
        /// Initializes all web based and core resolves 
        /// </summary>
        protected override void InitializeResolvers()
        {
            base.InitializeResolvers();

            //set the default RenderMvcController
            DefaultRenderMvcControllerResolver.Current = new DefaultRenderMvcControllerResolver(typeof(RenderMvcController));

            //Override the ServerMessengerResolver to set a username/password for the distributed calls
            ServerMessengerResolver.Current.SetServerMessenger(new DefaultServerMessenger(() =>
            {
                //we should not proceed to change this if the app/database is not configured since there will 
                // be no user, plus we don't need to have server messages sent if this is the case.
                if (ApplicationContext.IsConfigured && ApplicationContext.DatabaseContext.IsDatabaseConfigured)
                {                    
                    try
                    {
                        var user = User.GetUser(UmbracoSettings.DistributedCallUser);
                        return new System.Tuple<string, string>(user.LoginName, user.GetPassword());
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error<WebBootManager>("An error occurred trying to set the IServerMessenger during application startup", e);
                        return null;
                    }
                }
                LogHelper.Warn<WebBootManager>("Could not initialize the DefaultServerMessenger, the application is not configured or the database is not configured");
                return null;
            }));

            //We are going to manually remove a few cache refreshers here because we've obsoleted them and we don't want them
            // to be registered more than once
            CacheRefreshersResolver.Current.RemoveType<pageRefresher>();
            CacheRefreshersResolver.Current.RemoveType<global::umbraco.presentation.cache.MediaLibraryRefreshers>();
            CacheRefreshersResolver.Current.RemoveType<global::umbraco.presentation.cache.MemberLibraryRefreshers>();
            CacheRefreshersResolver.Current.RemoveType<global::umbraco.templateCacheRefresh>();
            CacheRefreshersResolver.Current.RemoveType<global::umbraco.macroCacheRefresh>();
            
            SurfaceControllerResolver.Current = new SurfaceControllerResolver(
                PluginManager.Current.ResolveSurfaceControllers());

            UmbracoApiControllerResolver.Current = new UmbracoApiControllerResolver(
                PluginManager.Current.ResolveUmbracoApiControllers());

            //the base creates the PropertyEditorValueConvertersResolver but we want to modify it in the web app and replace
            //the TinyMcePropertyEditorValueConverter with the RteMacroRenderingPropertyEditorValueConverter
            PropertyEditorValueConvertersResolver.Current.RemoveType<TinyMcePropertyEditorValueConverter>();
            PropertyEditorValueConvertersResolver.Current.AddType<RteMacroRenderingPropertyEditorValueConverter>();

            PublishedCachesResolver.Current = new PublishedCachesResolver(new PublishedCaches(
                new PublishedCache.XmlPublishedCache.PublishedContentCache(),
                new PublishedCache.XmlPublishedCache.PublishedMediaCache()));

            FilteredControllerFactoriesResolver.Current = new FilteredControllerFactoriesResolver(
                // add all known factories, devs can then modify this list on application
                // startup either by binding to events or in their own global.asax
                new[]
					{
						typeof (RenderControllerFactory)
					});

            UrlProviderResolver.Current = new UrlProviderResolver(
                    //typeof(AliasUrlProvider), // not enabled by default
                    typeof(DefaultUrlProvider)
                );

            // the legacy 404 will run from within ContentFinderByNotFoundHandlers below
            // so for the time being there is no last chance finder
			ContentLastChanceFinderResolver.Current = new ContentLastChanceFinderResolver();

			ContentFinderResolver.Current = new ContentFinderResolver(
				// add all known resolvers in the correct order, devs can then modify this list
                // on application startup either by binding to events or in their own global.asax
						typeof (ContentFinderByPageIdQuery),
						typeof (ContentFinderByNiceUrl),
						typeof (ContentFinderByIdPath),
                        // these will be handled by ContentFinderByNotFoundHandlers
                        // so they can be enabled/disabled even though resolvers are not public yet
						//typeof (ContentFinderByNiceUrlAndTemplate),
						//typeof (ContentFinderByProfile),
						//typeof (ContentFinderByUrlAlias),
                        typeof (ContentFinderByNotFoundHandlers)
					);

            SiteDomainHelperResolver.Current = new SiteDomainHelperResolver(new SiteDomainHelper());

            // ain't that a bit dirty?
            PublishedCache.XmlPublishedCache.PublishedContentCache.UnitTesting = _isForTesting;

            ThumbnailProvidersResolver.Current = new ThumbnailProvidersResolver(
                PluginManager.Current.ResolveThumbnailProviders());

            ImageUrlProviderResolver.Current = new ImageUrlProviderResolver(
                PluginManager.Current.ResolveImageUrlProviders());

            CultureDictionaryFactoryResolver.Current = new CultureDictionaryFactoryResolver(
                new DefaultCultureDictionaryFactory());
        }

    }
}
