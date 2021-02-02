using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Strings;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Constants = Umbraco.Core.Constants;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Runtime
{
    public sealed class WebInitialComponent : IComponent
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly UmbracoApiControllerTypeCollection _apiControllerTypes;
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IShortStringHelper _shortStringHelper;

        public WebInitialComponent(
            IUmbracoContextAccessor umbracoContextAccessor,
            UmbracoApiControllerTypeCollection apiControllerTypes,
            GlobalSettings globalSettings,
            IHostingEnvironment hostingEnvironment,
            IShortStringHelper shortStringHelper)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _apiControllerTypes = apiControllerTypes;
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
            _shortStringHelper = shortStringHelper;
        }

        public void Initialize()
        {
            // setup mvc and webapi services
            SetupMvcAndWebApi();

            // Disable the X-AspNetMvc-Version HTTP Header
            MvcHandler.DisableMvcResponseHeader = true;

            // wrap view engines in the profiling engine
            WrapViewEngines(ViewEngines.Engines);

            // add global filters
            ConfigureGlobalFilters();

            // set routes
            CreateRoutes(_umbracoContextAccessor, _globalSettings, _shortStringHelper, _apiControllerTypes, _hostingEnvironment);
        }

        public void Terminate()
        { }

        private static void ConfigureGlobalFilters()
        {
            //GlobalFilters.Filters.Add(new EnsurePartialViewMacroViewContextFilterAttribute());
        }

        // internal for tests
        internal static void WrapViewEngines(IList<IViewEngine> viewEngines)
        {
            if (viewEngines == null || viewEngines.Count == 0) return;

            var originalEngines = viewEngines.ToList();
            viewEngines.Clear();
            foreach (var engine in originalEngines)
            {
                var wrappedEngine = engine;// TODO introduce in NETCORE: is ProfilingViewEngine ? engine : new ProfilingViewEngine(engine);
                viewEngines.Add(wrappedEngine);
            }
        }

        private void SetupMvcAndWebApi()
        {
            //don't output the MVC version header (security)
            //MvcHandler.DisableMvcResponseHeader = true;

            // set master controller factory
            // var controllerFactory = new MasterControllerFactory(() => Current.FilteredControllerFactories);
            // ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            // set the render & plugin view engines
            // ViewEngines.Engines.Add(new RenderViewEngine(_hostingEnvironment));
            // ViewEngines.Engines.Add(new PluginViewEngine());

            ////add the profiling action filter
            //GlobalFilters.Filters.Add(new ProfilingActionFilter());

            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector),
                new NamespaceHttpControllerSelector(GlobalConfiguration.Configuration));
        }

        // internal for tests
        internal static void CreateRoutes(
            IUmbracoContextAccessor umbracoContextAccessor,
            GlobalSettings globalSettings,
            IShortStringHelper shortStringHelper,
            UmbracoApiControllerTypeCollection apiControllerTypes,
            IHostingEnvironment hostingEnvironment)
        {
            var umbracoPath = globalSettings.GetUmbracoMvcArea(hostingEnvironment);

            // create the front-end route
            var defaultRoute = RouteTable.Routes.MapRoute(
                "Umbraco_default",
                umbracoPath + "/RenderMvc/{action}/{id}",
                new { controller = "RenderMvc", action = "Index", id = UrlParameter.Optional }
            );

            defaultRoute.RouteHandler = new RenderRouteHandler(umbracoContextAccessor, ControllerBuilder.Current.GetControllerFactory(), shortStringHelper);
        }
    }
}
