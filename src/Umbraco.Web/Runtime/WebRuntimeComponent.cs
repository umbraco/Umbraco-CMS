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
using Examine;
using LightInject;
using Microsoft.AspNet.SignalR;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Macros;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Profiling;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Runtime;
using Umbraco.Core.Services;
using Umbraco.Examine;
using Umbraco.Web.Actions;
using Umbraco.Web.Cache;
using Umbraco.Web.Composing.CompositionRoots;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Dictionary;
using Umbraco.Web.Editors;
using Umbraco.Web.Features;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Install;
using Umbraco.Web.Media;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.PublishedContent;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Search;
using Umbraco.Web.Security;
using Umbraco.Web.Services;
using Umbraco.Web.SignalR;
using Umbraco.Web.Tour;
using Umbraco.Web.Trees;
using Umbraco.Web.UI.JavaScript;
using Umbraco.Web.WebApi;

using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Runtime
{
    [RequireComponent(typeof(CoreRuntimeComponent))]
    public class WebRuntimeComponent : UmbracoComponentBase, IRuntimeComponent
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.Container.RegisterFrom<WebMappingProfilesCompositionRoot>();

            //register the install components
            //NOTE: i tried to not have these registered if we weren't installing or upgrading but post install when the site restarts
            //it still needs to use the install controller so we can't do that
            composition.Container.RegisterFrom<InstallerCompositionRoot>();

            // register accessors for cultures
            composition.Container.RegisterSingleton<IDefaultCultureAccessor, DefaultCultureAccessor>();
            composition.Container.RegisterSingleton<IVariationContextAccessor, HttpContextVariationContextAccessor>();

            var typeLoader = composition.Container.GetInstance<TypeLoader>();
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

            // register the published snapshot accessor - the "current" published snapshot is in the umbraco context
            composition.Container.RegisterSingleton<IPublishedSnapshotAccessor, UmbracoContextPublishedSnapshotAccessor>();

            // register a per-request UmbracoContext object
            // no real need to be per request but assuming it is faster
            composition.Container.Register(factory => factory.GetInstance<IUmbracoContextAccessor>().UmbracoContext, new PerRequestLifeTime());

            // register the umbraco helper
            composition.Container.Register<UmbracoHelper>(new PerRequestLifeTime());

            // register distributed cache
            composition.Container.RegisterSingleton(f => new DistributedCache());

            // replace some services
            composition.Container.RegisterSingleton<IEventMessagesFactory, DefaultEventMessagesFactory>();
            composition.Container.RegisterSingleton<IEventMessagesAccessor, HybridEventMessagesAccessor>();
            composition.Container.RegisterSingleton<IApplicationTreeService, ApplicationTreeService>();
            composition.Container.RegisterSingleton<ISectionService, SectionService>();

            composition.Container.RegisterSingleton<IExamineManager>(factory => ExamineManager.Instance);

            // IoC setup for LightInject for MVC/WebApi
            // see comments on MixedLightInjectScopeManagerProvider for explainations of what we are doing here
            if (!(composition.Container.ScopeManagerProvider is MixedLightInjectScopeManagerProvider smp))
                throw new Exception("Container.ScopeManagerProvider is not MixedLightInjectScopeManagerProvider.");
            composition.Container.EnableMvc(); // does container.EnablePerWebRequestScope()
            composition.Container.ScopeManagerProvider = smp; // reverts - we will do it last (in WebRuntime)

            composition.Container.RegisterUmbracoControllers(typeLoader, GetType().Assembly);
            composition.Container.EnableWebApi(GlobalConfiguration.Configuration);

            composition.Container.RegisterCollectionBuilder<SearchableTreeCollectionBuilder>()
                .Add(() => typeLoader.GetTypes<ISearchableTree>()); // fixme which searchable trees?!

            composition.Container.RegisterCollectionBuilder<EditorValidatorCollectionBuilder>()
                .Add(() => typeLoader.GetTypes<IEditorValidator>());

            composition.Container.RegisterCollectionBuilder<TourFilterCollectionBuilder>();

            composition.Container.RegisterSingleton<UmbracoFeatures>();

            // set the default RenderMvcController
            Current.DefaultRenderMvcControllerType = typeof(RenderMvcController); // fixme WRONG!

            composition.Container.RegisterCollectionBuilder<ActionCollectionBuilder>()
                .Add(() => typeLoader.GetTypes<IAction>());

            var surfaceControllerTypes = new SurfaceControllerTypeCollection(typeLoader.GetSurfaceControllers());
            composition.Container.RegisterInstance(surfaceControllerTypes);

            var umbracoApiControllerTypes = new UmbracoApiControllerTypeCollection(typeLoader.GetUmbracoApiControllers());
            composition.Container.RegisterInstance(umbracoApiControllerTypes);

            // both TinyMceValueConverter (in Core) and RteMacroRenderingValueConverter (in Web) will be
            // discovered when CoreBootManager configures the converters. We HAVE to remove one of them
            // here because there cannot be two converters for one property editor - and we want the full
            // RteMacroRenderingValueConverter that converts macros, etc. So remove TinyMceValueConverter.
            // (the limited one, defined in Core, is there for tests) - same for others
            composition.Container.GetInstance<PropertyValueConverterCollectionBuilder>()
                .Remove<TinyMceValueConverter>()
                .Remove<TextStringValueConverter>()
                .Remove<MarkdownEditorValueConverter>();

            // add all known factories, devs can then modify this list on application
            // startup either by binding to events or in their own global.asax
            composition.Container.RegisterCollectionBuilder<FilteredControllerFactoryCollectionBuilder>()
                .Append<RenderControllerFactory>();

            composition.Container.RegisterCollectionBuilder<UrlProviderCollectionBuilder>()
                .Append<AliasUrlProvider>()
                .Append<DefaultUrlProvider>()
                .Append<CustomRouteUrlProvider>();

            composition.Container.RegisterSingleton<IContentLastChanceFinder, ContentFinderByLegacy404>();

            composition.Container.RegisterCollectionBuilder<ContentFinderCollectionBuilder>()
                // all built-in finders in the correct order,
                // devs can then modify this list on application startup
                .Append<ContentFinderByPageIdQuery>()
                .Append<ContentFinderByUrl>()
                .Append<ContentFinderByIdPath>()
                //.Append<ContentFinderByUrlAndTemplate>() // disabled, this is an odd finder
                .Append<ContentFinderByUrlAlias>()
                .Append<ContentFinderByRedirectUrl>();

            composition.Container.RegisterSingleton<ISiteDomainHelper, SiteDomainHelper>();

            composition.Container.RegisterSingleton<ICultureDictionaryFactory, DefaultCultureDictionaryFactory>();

            // register *all* checks, except those marked [HideFromTypeFinder] of course
            composition.Container.RegisterCollectionBuilder<HealthCheckCollectionBuilder>()
                .Add(() => typeLoader.GetTypes<HealthCheck.HealthCheck>());

            composition.Container.RegisterCollectionBuilder<HealthCheckNotificationMethodCollectionBuilder>()
                .Add(() => typeLoader.GetTypes<HealthCheck.NotificationMethods.IHealthCheckNotificationMethod>());

            // auto-register views
            composition.Container.RegisterAuto(typeof(UmbracoViewPage<>));

            // register published router
            composition.Container.RegisterSingleton<PublishedRouter>();
            composition.Container.Register(_ => UmbracoConfig.For.UmbracoSettings().WebRouting);

            // register preview SignalR hub
            composition.Container.Register(_ => GlobalHost.ConnectionManager.GetHubContext<PreviewHub>(), new PerContainerLifetime());

            // register properties fallback
            composition.Container.RegisterSingleton<IPublishedValueFallback, PublishedValueFallback>();

            // register known content apps
            composition.Container.RegisterCollectionBuilder<ContentAppDefinitionCollectionBuilder>()
                .Append<ListViewContentAppDefinition>()
                .Append<ContentEditorContentAppDefinition>()
                .Append<ContentInfoContentAppDefinition>();

            composition.Container.RegisterSingleton<DashboardHelper>();
        }

        internal void Initialize(
            IRuntimeState runtime,
            IUmbracoContextAccessor umbracoContextAccessor,
            SurfaceControllerTypeCollection surfaceControllerTypes,
            UmbracoApiControllerTypeCollection apiControllerTypes,
            IPublishedSnapshotService publishedSnapshotService,
            IUserService userService,
            IUmbracoSettingsSection umbracoSettings,
            IGlobalSettings globalSettings,
            IEntityService entityService,
            IVariationContextAccessor variationContextAccessor,
            UrlProviderCollection urlProviders)
        {
            // setup mvc and webapi services
            SetupMvcAndWebApi();

            // client dependency
            ConfigureClientDependency(globalSettings);

            // Disable the X-AspNetMvc-Version HTTP Header
            MvcHandler.DisableMvcResponseHeader = true;

            InstallHelper.DeleteLegacyInstaller();

            // wrap view engines in the profiling engine
            WrapViewEngines(ViewEngines.Engines);

            // add global filters
            ConfigureGlobalFilters();

            // set routes
            CreateRoutes(umbracoContextAccessor, globalSettings, surfaceControllerTypes, apiControllerTypes);

            // get an http context
            // at that moment, HttpContext.Current != null but its .Request property is null
            var httpContext = new HttpContextWrapper(HttpContext.Current);

            // ensure there is an UmbracoContext
            // (also sets the accessor)
            // this is a *temp* UmbracoContext
            UmbracoContext.EnsureContext(
                umbracoContextAccessor,
                new HttpContextWrapper(HttpContext.Current),
                publishedSnapshotService,
                new WebSecurity(httpContext, userService, globalSettings),
                umbracoSettings,
                urlProviders,
                globalSettings,
                variationContextAccessor);

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
            XmlFileMapper.FileMapDefaultFolder = "~/App_Data/TEMP/ClientDependency";
            BaseCompositeFileProcessingProvider.UrlTypeDefault = CompositeUrlType.Base64QueryStrings;

            // Now we need to detect if we are running umbracoLocalTempStorage as EnvironmentTemp and in that case we want to change the CDF file
            // location to be there
            if (globalSettings.LocalTempStorageLocation == LocalTempStorage.EnvironmentTemp)
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

            ClientDependencySettings.Instance.MvcRendererCollection.Add(renderer);
        }
    }
}
