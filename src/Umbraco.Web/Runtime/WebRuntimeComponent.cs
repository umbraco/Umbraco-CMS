using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Routing;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Config;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.Install;
using Umbraco.Web.JavaScript;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;

using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Runtime
{
    public sealed class WebRuntimeComponent : IComponent
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly SurfaceControllerTypeCollection _surfaceControllerTypes;
        private readonly UmbracoApiControllerTypeCollection _apiControllerTypes;
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly IUserService _userService;
        private readonly IUmbracoSettingsSection _umbracoSettings;
        private readonly IGlobalSettings _globalSettings;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly UrlProviderCollection _urlProviders;

        public WebRuntimeComponent(
            IUmbracoContextAccessor umbracoContextAccessor,
            SurfaceControllerTypeCollection surfaceControllerTypes,
            UmbracoApiControllerTypeCollection apiControllerTypes,
            IPublishedSnapshotService publishedSnapshotService,
            IUserService userService,
            IUmbracoSettingsSection umbracoSettings,
            IGlobalSettings globalSettings,
            IVariationContextAccessor variationContextAccessor,
            UrlProviderCollection urlProviders)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _surfaceControllerTypes = surfaceControllerTypes;
            _apiControllerTypes = apiControllerTypes;
            _publishedSnapshotService = publishedSnapshotService;
            _userService = userService;
            _umbracoSettings = umbracoSettings;
            _globalSettings = globalSettings;
            _variationContextAccessor = variationContextAccessor;
            _urlProviders = urlProviders;
        }

        public void Initialize()
        { 
            // setup mvc and webapi services
            SetupMvcAndWebApi();

            // client dependency
            ConfigureClientDependency(_globalSettings);

            // Disable the X-AspNetMvc-Version HTTP Header
            MvcHandler.DisableMvcResponseHeader = true;

            // wrap view engines in the profiling engine
            WrapViewEngines(ViewEngines.Engines);

            // add global filters
            ConfigureGlobalFilters();

            // set routes
            CreateRoutes(_umbracoContextAccessor, _globalSettings, _surfaceControllerTypes, _apiControllerTypes);

            // get an http context
            // at that moment, HttpContext.Current != null but its .Request property is null
            var httpContext = new HttpContextWrapper(HttpContext.Current);

            // ensure there is an UmbracoContext
            // (also sets the accessor)
            // this is a *temp* UmbracoContext
            UmbracoContext.EnsureContext(
                _umbracoContextAccessor,
                new HttpContextWrapper(HttpContext.Current),
                _publishedSnapshotService,
                new WebSecurity(httpContext, _userService, _globalSettings),
                _umbracoSettings,
                _urlProviders,
                _globalSettings,
                _variationContextAccessor);

            // ensure WebAPI is initialized, after everything
            GlobalConfiguration.Configuration.EnsureInitialized();
        }

        public void Terminate()
        { }

        private static void ConfigureGlobalFilters()
        {
            GlobalFilters.Filters.Add(new EnsurePartialViewMacroViewContextFilterAttribute());
        }

        // internal for tests
        internal static void WrapViewEngines(IList<IViewEngine> viewEngines)
        {
            if (viewEngines == null || viewEngines.Count == 0) return;

            var originalEngines = viewEngines.ToList();
            viewEngines.Clear();
            foreach (var engine in originalEngines)
            {
                var wrappedEngine = engine is ProfilingViewEngine ? engine : new ProfilingViewEngine(engine);
                viewEngines.Add(wrappedEngine);
            }
        }

        // internal for tests
        internal static void CreateRoutes(
            IUmbracoContextAccessor umbracoContextAccessor,
            IGlobalSettings globalSettings,
            SurfaceControllerTypeCollection surfaceControllerTypes,
            UmbracoApiControllerTypeCollection apiControllerTypes)
        {
            var umbracoPath = globalSettings.GetUmbracoMvcArea();

            // create the front-end route
            var defaultRoute = RouteTable.Routes.MapRoute(
                "Umbraco_default",
                umbracoPath + "/RenderMvc/{action}/{id}",
                new { controller = "RenderMvc", action = "Index", id = UrlParameter.Optional }
                );
            defaultRoute.RouteHandler = new RenderRouteHandler(umbracoContextAccessor, ControllerBuilder.Current.GetControllerFactory());

            // register install routes
            RouteTable.Routes.RegisterArea<UmbracoInstallArea>();

            // register all back office routes
            RouteTable.Routes.RegisterArea(new BackOfficeArea(globalSettings));

            // plugin controllers must come first because the next route will catch many things
            RoutePluginControllers(globalSettings, surfaceControllerTypes, apiControllerTypes);
        }

        private static void RoutePluginControllers(
            IGlobalSettings globalSettings,
            SurfaceControllerTypeCollection surfaceControllerTypes,
            UmbracoApiControllerTypeCollection apiControllerTypes)
        {
            var umbracoPath = globalSettings.GetUmbracoMvcArea();

            // need to find the plugin controllers and route them
            var pluginControllers = surfaceControllerTypes.Concat(apiControllerTypes).ToArray();

            // local controllers do not contain the attribute
            var localControllers = pluginControllers.Where(x => PluginController.GetMetadata(x).AreaName.IsNullOrWhiteSpace());
            foreach (var s in localControllers)
            {
                if (TypeHelper.IsTypeAssignableFrom<SurfaceController>(s))
                    RouteLocalSurfaceController(s, umbracoPath);
                else if (TypeHelper.IsTypeAssignableFrom<UmbracoApiController>(s))
                    RouteLocalApiController(s, umbracoPath);
            }

            // get the plugin controllers that are unique to each area (group by)
            var pluginSurfaceControlleres = pluginControllers.Where(x => PluginController.GetMetadata(x).AreaName.IsNullOrWhiteSpace() == false);
            var groupedAreas = pluginSurfaceControlleres.GroupBy(controller => PluginController.GetMetadata(controller).AreaName);
            // loop through each area defined amongst the controllers
            foreach (var g in groupedAreas)
            {
                // create & register an area for the controllers (this will throw an exception if all controllers are not in the same area)
                var pluginControllerArea = new PluginControllerArea(globalSettings, g.Select(PluginController.GetMetadata));
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

        private static void SetupMvcAndWebApi()
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
            ModelBinderProviders.BinderProviders.Add(ContentModelBinder.Instance); // is a provider

            ////add the profiling action filter
            //GlobalFilters.Filters.Add(new ProfilingActionFilter());

            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector),
                new NamespaceHttpControllerSelector(GlobalConfiguration.Configuration));
        }

        private static void ConfigureClientDependency(IGlobalSettings globalSettings)
        {
            // Backwards compatibility - set the path and URL type for ClientDependency 1.5.1 [LK]
            XmlFileMapper.FileMapDefaultFolder = SystemDirectories.TempData.EnsureEndsWith('/') + "ClientDependency";
            BaseCompositeFileProcessingProvider.UrlTypeDefault = CompositeUrlType.Base64QueryStrings;

            // Now we need to detect if we are running 'Umbraco.Core.LocalTempStorage' as EnvironmentTemp and in that case we want to change the CDF file
            // location to be there
            if (globalSettings.LocalTempStorageLocation == LocalTempStorage.EnvironmentTemp)
            {
                var appDomainHash = HttpRuntime.AppDomainAppId.ToSHA1();
                var cachePath = Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "UmbracoData",
                    //include the AppDomain hash is just a safety check, for example if a website is moved from worker A to worker B and then back
                    // to worker A again, in theory the %temp%  folder should already be empty but we really want to make sure that its not
                    // utilizing an old path
                    appDomainHash);

                //set the file map and composite file default location to the %temp% location
                BaseCompositeFileProcessingProvider.CompositeFilePathDefaultFolder
                    = XmlFileMapper.FileMapDefaultFolder
                    = Path.Combine(cachePath, "ClientDependency");
            }

            if (ConfigurationManager.GetSection("system.web/httpRuntime") is HttpRuntimeSection section)
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

            // When using a non-web runtime and this component is loaded ClientDependency explodes because it'll
            // want to access HttpContext.Current, which doesn't exist
            if (IOHelper.IsHosted)
                ClientDependencySettings.Instance.MvcRendererCollection.Add(renderer);
        }
    }
}
