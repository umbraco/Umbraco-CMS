using System;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Web.Install;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Runtime
{
    public class WebFinalComponent : IComponent
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly SurfaceControllerTypeCollection _surfaceControllerTypes;
        private readonly UmbracoApiControllerTypeCollection _apiControllerTypes;
        private readonly IGlobalSettings _globalSettings;

        public WebFinalComponent(IUmbracoContextAccessor umbracoContextAccessor, SurfaceControllerTypeCollection surfaceControllerTypes, UmbracoApiControllerTypeCollection apiControllerTypes, IGlobalSettings globalSettings)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _surfaceControllerTypes = surfaceControllerTypes;
            _apiControllerTypes = apiControllerTypes;
            _globalSettings = globalSettings;
        }

        public void Initialize()
        {
            // set routes
            CreateRoutes(_umbracoContextAccessor, _globalSettings, _surfaceControllerTypes, _apiControllerTypes);

            // ensure WebAPI is initialized, after everything
            GlobalConfiguration.Configuration.EnsureInitialized();
        }

        public void Terminate()
        { }

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
    }
}
