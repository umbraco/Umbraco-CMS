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
using LightInject;
using Microsoft.AspNet.SignalR;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Configuration;
using Umbraco.Core.DI;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Macros;
using Umbraco.Core.Plugins;
using Umbraco.Core.Profiling;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;
using Umbraco.Web.DI;
using Umbraco.Web.Dictionary;
using Umbraco.Web.Editors;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Install;
using Umbraco.Web.Media;
using Umbraco.Web.Media.ThumbnailProviders;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.Services;
using Umbraco.Web.SignalR;
using Umbraco.Web.UI.JavaScript;
using Umbraco.Web.WebApi;
using Umbraco.Web._Legacy.Actions;
using UmbracoExamine;

namespace Umbraco.Web
{
    [RequireComponent(typeof(CoreRuntimeComponent))]
    public class WebRuntimeComponent : UmbracoComponentBase, IRuntimeComponent
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.Container.RegisterFrom<WebModelMappersCompositionRoot>();

            var pluginManager = composition.Container.GetInstance<PluginManager>();
            var logger = composition.Container.GetInstance<ILogger>();
            var proflog = composition.Container.GetInstance<ProfilingLogger>();

            // register the http context and umbraco context accessors
            // we *should* use the HttpContextUmbracoContextAccessor, however there are cases when
            // we have no http context, eg when booting Umbraco or in background threads, so instead
            // let's use an hybrid accessor that can fall back to a ThreadStatic context.
            composition.Container.RegisterSingleton<IUmbracoContextAccessor, HybridUmbracoContextAccessor>();

            // register the 'current' umbraco context - transient - for eg controllers
            composition.Container.Register(factory => factory.GetInstance<IUmbracoContextAccessor>().UmbracoContext);

            // register a per-request HttpContextBase object
            // is per-request so only one wrapper is created per request
            composition.Container.Register<HttpContextBase>(factory => new HttpContextWrapper(factory.GetInstance<IHttpContextAccessor>().HttpContext), new PerRequestLifeTime());

            // register the facade accessor - the "current" facade is in the umbraco context
            composition.Container.RegisterSingleton<IFacadeAccessor, UmbracoContextFacadeAccessor>();

            // register a per-request UmbracoContext object
            // no real need to be per request but assuming it is faster
            composition.Container.Register(factory => factory.GetInstance<IUmbracoContextAccessor>().UmbracoContext, new PerRequestLifeTime());

            // register the umbraco helper
            // fixme - FUCK! how can this even work, it's not a singleton!
            composition.Container.RegisterSingleton<UmbracoHelper>();

            // replace some services
            composition.Container.RegisterSingleton<IEventMessagesFactory, DefaultEventMessagesFactory>();
            composition.Container.RegisterSingleton<IEventMessagesAccessor, HybridEventMessagesAccessor>();
            composition.Container.RegisterSingleton<IApplicationTreeService, ApplicationTreeService>();
            composition.Container.RegisterSingleton<ISectionService, SectionService>();

            composition.Container.RegisterSingleton<IExamineIndexCollectionAccessor, ExamineIndexCollectionAccessor>();

            // IoC setup for LightInject for MVC/WebApi
            // see comments on MixedScopeManagerProvider for explainations of what we are doing here
            var smp = composition.Container.ScopeManagerProvider as MixedScopeManagerProvider;
            if (smp == null) throw new Exception("Container.ScopeManagerProvider is not MixedScopeManagerProvider.");
            composition.Container.EnableMvc(); // does container.EnablePerWebRequestScope()
            composition.Container.ScopeManagerProvider = smp; // reverts - we will do it last (in WebRuntime)

            composition.Container.RegisterMvcControllers(pluginManager, GetType().Assembly);
            composition.Container.EnableWebApi(GlobalConfiguration.Configuration);
            composition.Container.RegisterApiControllers(pluginManager, GetType().Assembly);

            XsltExtensionCollectionBuilder.Register(composition.Container)
                .AddExtensionObjectProducer(() => pluginManager.ResolveXsltExtensions());

            composition.Container.RegisterCollectionBuilder<EditorValidatorCollectionBuilder>()
                .Add(() => pluginManager.ResolveTypes<IEditorValidator>());

            // set the default RenderMvcController
            Current.DefaultRenderMvcControllerType = typeof(RenderMvcController); // fixme WRONG!

            ActionCollectionBuilder.Register(composition.Container)
                .SetProducer(() => pluginManager.ResolveActions());

            var surfaceControllerTypes = new SurfaceControllerTypeCollection(pluginManager.ResolveSurfaceControllers());
            composition.Container.RegisterInstance(surfaceControllerTypes);

            var umbracoApiControllerTypes = new UmbracoApiControllerTypeCollection(pluginManager.ResolveUmbracoApiControllers());
            composition.Container.RegisterInstance(umbracoApiControllerTypes);

            // both TinyMceValueConverter (in Core) and RteMacroRenderingValueConverter (in Web) will be
            // discovered when CoreBootManager configures the converters. We HAVE to remove one of them
            // here because there cannot be two converters for one property editor - and we want the full
            // RteMacroRenderingValueConverter that converts macros, etc. So remove TinyMceValueConverter.
            // (the limited one, defined in Core, is there for tests) - same for others
            composition.Container.GetInstance<PropertyValueConverterCollectionBuilder>()
                .Remove<TinyMceValueConverter>()
                .Remove<TextStringValueConverter>()
                .Remove<MarkdownEditorValueConverter>()
                .Remove<ImageCropperValueConverter>();

            // add all known factories, devs can then modify this list on application
            // startup either by binding to events or in their own global.asax
            composition.Container.RegisterCollectionBuilder<FilteredControllerFactoryCollectionBuilder>()
                .Append<RenderControllerFactory>();

            composition.Container.RegisterCollectionBuilder<UrlProviderCollectionBuilder>()
                //.Append<AliasUrlProvider>() // not enabled by default
                .Append<DefaultUrlProvider>()
                .Append<CustomRouteUrlProvider>();

            composition.Container.RegisterSingleton<IContentLastChanceFinder, ContentFinderByLegacy404>();

            composition.Container.RegisterCollectionBuilder<ContentFinderCollectionBuilder>()
                // all built-in finders in the correct order,
                // devs can then modify this list on application startup
                .Append<ContentFinderByPageIdQuery>()
                .Append<ContentFinderByNiceUrl>()
                .Append<ContentFinderByIdPath>()
                .Append<ContentFinderByNiceUrlAndTemplate>()
                .Append<ContentFinderByProfile>()
                .Append<ContentFinderByUrlAlias>()
                .Append<ContentFinderByRedirectUrl>();

            composition.Container.RegisterSingleton<ISiteDomainHelper, SiteDomainHelper>();

            composition.Container.RegisterCollectionBuilder<ThumbnailProviderCollectionBuilder>()
                .Add(pluginManager.ResolveThumbnailProviders());

            composition.Container.RegisterCollectionBuilder<ImageUrlProviderCollectionBuilder>()
                .Append(pluginManager.ResolveImageUrlProviders());

            composition.Container.RegisterSingleton<ICultureDictionaryFactory, DefaultCultureDictionaryFactory>();

            // register *all* checks, except those marked [HideFromTypeFinder] of course
            composition.Container.RegisterCollectionBuilder<HealthCheckCollectionBuilder>()
                .Add(() => pluginManager.ResolveTypes<HealthCheck.HealthCheck>());

            // auto-register views
            composition.Container.RegisterAuto(typeof(UmbracoViewPage<>));

            // register facade router
            composition.Container.Register<FacadeRouter>();
            composition.Container.Register(_ => UmbracoConfig.For.UmbracoSettings().WebRouting);

            // register preview SignalR hub
            composition.Container.Register(_ => GlobalHost.ConnectionManager.GetHubContext<PreviewHub>(), new PerContainerLifetime());
        }

        internal void Initialize(
            IRuntimeState runtime,
            IUmbracoContextAccessor umbracoContextAccessor,
            SurfaceControllerTypeCollection surfaceControllerTypes,
            UmbracoApiControllerTypeCollection apiControllerTypes,
            IFacadeService facadeService,
            IUserService userService,
            UrlProviderCollection urlProviders)
        {
            // setup mvc and webapi services
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

            // wrap view engines in the profiling engine
            WrapViewEngines(ViewEngines.Engines);

            // add global filters
            ConfigureGlobalFilters();

            // set routes
            CreateRoutes(umbracoContextAccessor, surfaceControllerTypes, apiControllerTypes);

            // get an http context
            // at that moment, HttpContext.Current != null but its .Request property is null
            var httpContext = new HttpContextWrapper(HttpContext.Current);

            // ensure there is an UmbracoContext
            // (also sets the accessor)
            // this is a *temp* UmbracoContext
            UmbracoContext.EnsureContext(
                umbracoContextAccessor,
                new HttpContextWrapper(HttpContext.Current),
                facadeService,
                new WebSecurity(httpContext, userService),
                UmbracoConfig.For.UmbracoSettings(),
                urlProviders);

            // ensure WebAPI is initialized, after everything
            GlobalConfiguration.Configuration.EnsureInitialized();
        }

        private static void ConfigureGlobalFilters()
        {
            GlobalFilters.Filters.Add(new EnsurePartialViewMacroViewContextFilterAttribute());
        }

        // internal for tests
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

        // internal for tests
        internal static void CreateRoutes(
            IUmbracoContextAccessor umbracoContextAccessor,
            SurfaceControllerTypeCollection surfaceControllerTypes,
            UmbracoApiControllerTypeCollection apiControllerTypes)
        {
            var umbracoPath = GlobalSettings.UmbracoMvcArea;

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
            RouteTable.Routes.RegisterArea<BackOfficeArea>();

            // plugin controllers must come first because the next route will catch many things
            RoutePluginControllers(surfaceControllerTypes, apiControllerTypes);
        }

        private static void RoutePluginControllers(
            SurfaceControllerTypeCollection surfaceControllerTypes,
            UmbracoApiControllerTypeCollection apiControllerTypes)
        {
            var umbracoPath = GlobalSettings.UmbracoMvcArea;

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
            ModelBinderProviders.BinderProviders.Add(new ContentModelBinder()); // is a provider

            ////add the profiling action filter
            //GlobalFilters.Filters.Add(new ProfilingActionFilter());

            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector),
                new NamespaceHttpControllerSelector(GlobalConfiguration.Configuration));
        }
    }
}
