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
using Umbraco.Core.Plugins;
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
    /// Represents the Web Umbraco runtime.
    /// </summary>
    /// <remarks>On top of CoreRuntime, handles all of the web-related aspects of Umbraco (startup, etc).</remarks>
    public class WebRuntime : CoreRuntime // fixme - probably crazy but... could it be a component?!
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebRuntime"/> class.
        /// </summary>
        /// <param name="umbracoApplication"></param>
        public WebRuntime(UmbracoApplicationBase umbracoApplication)
            : base(umbracoApplication)
        { }

        /// <inheritdoc/>
        public override void Boot(ServiceContainer container)
        {
            base.Boot(container);

            // now is the time to switch over to perWebRequest scopes
            var smp = container.ScopeManagerProvider as MixedScopeManagerProvider;
            if (smp == null) throw new Exception("Container.ScopeManagerProvider is not MixedScopeManagerProvider.");
            smp.EnablePerWebRequestScope();
        }

        /// <inheritdoc/>
        public override void Terminate()
        {
            base.Terminate();
        }

        /// <inheritdoc/>
        public override void Compose(ServiceContainer container)
        {
            base.Compose(container);
        }

        #region Getters

        protected override IProfiler GetProfiler() => new WebProfiler();

        protected override CacheHelper GetApplicationCache() => new CacheHelper(
                // we need to have the dep clone runtime cache provider to ensure
                // all entities are cached properly (cloned in and cloned out)
                new DeepCloneRuntimeCacheProvider(new HttpRuntimeCacheProvider(HttpRuntime.Cache)),
                new StaticCacheProvider(),
                // we need request based cache when running in web-based context
                new HttpRequestCacheProvider(),
                new IsolatedRuntimeCache(type =>
                    // we need to have the dep clone runtime cache provider to ensure
                    // all entities are cached properly (cloned in and cloned out)
                    new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider())));

        #endregion

        #region Web

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

        private static void RoutePluginControllers()
        {
            var umbracoPath = GlobalSettings.UmbracoMvcArea;

            // need to find the plugin controllers and route them
            var pluginControllers = Current.SurfaceControllerTypes
                .Concat(Current.UmbracoApiControllerTypes)
                .ToArray();

            // local controllers do not contain the attribute
            var localControllers = pluginControllers.Where(x => PluginController.GetMetadata(x).AreaName.IsNullOrWhiteSpace());
            foreach (var s in localControllers)
            {
                if (TypeHelper.IsTypeAssignableFrom<SurfaceController>(s))
                    RouteLocalSurfaceController(s, umbracoPath);
                else if (TypeHelper.IsTypeAssignableFrom<UmbracoApiController>(s))
                    RouteLocalApiController(s, umbracoPath);
            }

            // need to get the plugin controllers that are unique to each area (group by)
            var pluginSurfaceControlleres = pluginControllers.Where(x => PluginController.GetMetadata(x).AreaName.IsNullOrWhiteSpace() == false);
            var groupedAreas = pluginSurfaceControlleres.GroupBy(controller => PluginController.GetMetadata(controller).AreaName);
            // loop through each area defined amongst the controllers
            foreach (var g in groupedAreas)
            {
                // create & register an area for the controllers (this will throw an exception if all controllers are not in the same area)
                var pluginControllerArea = new PluginControllerArea(g.Select(PluginController.GetMetadata));
                RouteTable.Routes.RegisterArea(pluginControllerArea);
            }
        }

        private static void RouteLocalApiController(Type controller, string umbracoPath)
        {
            var meta = PluginController.GetMetadata(controller);
            var url = umbracoPath + (meta.IsBackOffice ? "/BackOffice" : "") + "/Api/" + meta.ControllerName + "/{action}/{id}";
            var route = RouteTable.Routes.MapHttpRoute(
                $"umbraco-api-{meta.ControllerName}",
                url, // url to match
                new { controller = meta.ControllerName, id = UrlParameter.Optional },
                new[] { meta.ControllerNamespace });
            if (route.DataTokens == null) // web api routes don't set the data tokens object
                route.DataTokens = new RouteValueDictionary();
            route.DataTokens.Add(Core.Constants.Web.UmbracoDataToken, "api"); //ensure the umbraco token is set
        }

        private static void RouteLocalSurfaceController(Type controller, string umbracoPath)
        {
            var meta = PluginController.GetMetadata(controller);
            var url = umbracoPath + "/Surface/" + meta.ControllerName + "/{action}/{id}";
            var route = RouteTable.Routes.MapRoute(
                $"umbraco-surface-{meta.ControllerName}",
                url, // url to match
                new { controller = meta.ControllerName, action = "Index", id = UrlParameter.Optional },
                new[] { meta.ControllerNamespace }); // look in this namespace to create the controller
            route.DataTokens.Add(Core.Constants.Web.UmbracoDataToken, "surface"); // ensure the umbraco token is set
            route.DataTokens.Add("UseNamespaceFallback", false); // don't look anywhere else except this namespace!
            // make it use our custom/special SurfaceMvcHandler
            route.RouteHandler = new SurfaceRouteHandler();
        }

        #endregion

        /// <summary>
        /// Initialize objects before anything during the boot cycle happens
        /// </summary>
        /// <returns></returns>
        public override IRuntime Initialize()
        {
            //TODO: Fix this - we need to manually perform re-indexing on startup when necessary Examine lib no longer does this
            ////This is basically a hack for this item: http://issues.umbraco.org/issue/U4-5976
            // // when Examine initializes it will try to rebuild if the indexes are empty, however in many cases not all of Examine's
            // // event handlers will be assigned during bootup when the rebuilding starts which is a problem. So with the examine 0.1.58.2941 build
            // // it has an event we can subscribe to in order to cancel this rebuilding process, but what we'll do is cancel it and postpone the rebuilding until the
            // // boot process has completed. It's a hack but it works.
            //ExamineManager.Instance.BuildingEmptyIndexOnStartup += OnInstanceOnBuildingEmptyIndexOnStartup;

            base.Initialize();

            // fixme - only here so that everything above can have its OWN scope => must come AFTER?? we have initialized all the components... OR?!
            // so that should basically happen AFTER we have Compose() and Initialize() each component = wtf?
            // this is all completely fucked
            //Container.EnablePerWebRequestScope(); // THAT ONE is causing the issue
            // the rest is moving to Compose?
            //Container.EnableMvc();
            //Container.RegisterMvcControllers(PluginManager, GetType().Assembly);
            //Container.EnableWebApi(GlobalConfiguration.Configuration);
            //Container.RegisterApiControllers(PluginManager, GetType().Assembly);

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
        protected override void Complete2()
        {
            base.Complete2();

            //before we do anything, we'll ensure the umbraco context
            //see: http://issues.umbraco.org/issue/U4-1717
            var httpContext = new HttpContextWrapper(UmbracoApplication.Context);
            UmbracoContext.EnsureContext( // fixme - refactor! UmbracoContext & UmbracoRequestContext!
                httpContext, Current.ApplicationContext,
                Current.FacadeService,
                new WebSecurity(httpContext, Current.ApplicationContext),
                UmbracoConfig.For.UmbracoSettings(),
                Current.UrlProviders,
                false);
        }

        /// <summary>
        /// Ensure that the OnApplicationStarted methods of the IApplicationEvents are called
        /// </summary>
        /// <param name="afterComplete"></param>
        /// <returns></returns>
        public override IRuntime Complete(Action<ApplicationContext> afterComplete)
        {
            //Wrap viewengines in the profiling engine
            WrapViewEngines(ViewEngines.Engines);

            //add global filters
            ConfigureGlobalFilters();

            //set routes
            CreateRoutes(); // fixme - throws, wtf?!

            base.Complete(afterComplete);

            //rebuild any empty indexes
            //TODO: Do we want to make this optional? otherwise the only way to disable this on startup
            // would be to implement a custom WebBootManager and override this method
            RebuildIndexes(true);

            //Now ensure webapi is initialized after everything
            GlobalConfiguration.Configuration.EnsureInitialized();

            return this;
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
            if (Current.ApplicationContext.IsConfigured == false || Current.ApplicationContext.DatabaseContext.IsDatabaseConfigured == false)
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

