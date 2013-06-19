using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Dynamics;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Dictionary;
using Umbraco.Web.Media;
using Umbraco.Web.Media.ThumbnailProviders;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.Routing;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;


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
            ViewEngines.Engines.Add(new RenderViewEngine());
            //set the plugin view engine
            ViewEngines.Engines.Add(new PluginViewEngine());

            //set model binder
            ModelBinders.Binders.Add(new KeyValuePair<Type, IModelBinder>(typeof(RenderModel), new RenderModelBinder()));

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
        /// Adds custom types to the ApplicationEventsResolver
        /// </summary>
        protected override void InitializeApplicationEventsResolver()
        {
            base.InitializeApplicationEventsResolver();
            ApplicationEventsResolver.Current.AddType<CacheHelperExtensions.CacheHelperApplicationEventListener>();
            ApplicationEventsResolver.Current.AddType<LegacyScheduledTasks>();
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
            ApplicationEventsResolver.Current.InstantiateLegacyStartupHanlders();

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

            //register all back office routes
            RouteBackOfficeControllers();

            //plugin controllers must come first because the next route will catch many things
            RoutePluginControllers();
        }

        private void RouteBackOfficeControllers()
        {
            var backOfficeArea = new BackOfficeArea();
            RouteTable.Routes.RegisterArea(backOfficeArea);
        }

        private void RoutePluginControllers()
        {
            var umbracoPath = GlobalSettings.UmbracoMvcArea;

            //we need to find the plugin controllers and route them
            var pluginControllers =
                SurfaceControllerResolver.Current.RegisteredSurfaceControllers.ToArray();

            //local controllers do not contain the attribute 			
            var localControllers = pluginControllers.Where(x => PluginController.GetMetadata(x).AreaName.IsNullOrWhiteSpace());
            foreach (var s in localControllers)
            {
                RouteLocalSurfaceController(s, umbracoPath);
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
        }

        /// <summary>
        /// Initializes all web based and core resolves 
        /// </summary>
        protected override void InitializeResolvers()
        {
            base.InitializeResolvers();

            //TODO: This needs to be removed in future versions (i.e. 6.0 when the PublishedContentHelper can access the business logic)
            // see the TODO noted in the PublishedContentHelper.
            PublishedContentHelper.GetDataTypeCallback = ContentType.GetDataType;

            SurfaceControllerResolver.Current = new SurfaceControllerResolver(
                PluginManager.Current.ResolveSurfaceControllers());

            //the base creates the PropertyEditorValueConvertersResolver but we want to modify it in the web app and replace
            //the TinyMcePropertyEditorValueConverter with the RteMacroRenderingPropertyEditorValueConverter
            PropertyEditorValueConvertersResolver.Current.RemoveType<TinyMcePropertyEditorValueConverter>();
            PropertyEditorValueConvertersResolver.Current.AddType<RteMacroRenderingPropertyEditorValueConverter>();

            PublishedContentStoreResolver.Current = new PublishedContentStoreResolver(new DefaultPublishedContentStore());
            PublishedMediaStoreResolver.Current = new PublishedMediaStoreResolver(new DefaultPublishedMediaStore());

            FilteredControllerFactoriesResolver.Current = new FilteredControllerFactoriesResolver(
                //add all known factories, devs can then modify this list on application startup either by binding to events
                //or in their own global.asax
                new[]
					{
						typeof (RenderControllerFactory)
					});

            // the legacy 404 will run from within LookupByNotFoundHandlers below
            // so for the time being there is no last chance lookup
            LastChanceLookupResolver.Current = new LastChanceLookupResolver();

            DocumentLookupsResolver.Current = new DocumentLookupsResolver(
                //add all known resolvers in the correct order, devs can then modify this list on application startup either by binding to events
                //or in their own global.asax
                new[]
					{
						typeof (LookupByPageIdQuery),
						typeof (LookupByNiceUrl),
						typeof (LookupByIdPath),
                        // these will be handled by LookupByNotFoundHandlers
                        // so they can be enabled/disabled even though resolvers are not public yet
						//typeof (LookupByNiceUrlAndTemplate),
						//typeof (LookupByProfile),
						//typeof (LookupByAlias),
                        typeof (LookupByNotFoundHandlers)
					});

            RoutesCacheResolver.Current = new RoutesCacheResolver(new DefaultRoutesCache(_isForTesting == false));

            ThumbnailProvidersResolver.Current = new ThumbnailProvidersResolver(
                PluginManager.Current.ResolveThumbnailProviders());

            ImageUrlProviderResolver.Current = new ImageUrlProviderResolver(
                PluginManager.Current.ResolveImageUrlProviders());

            CultureDictionaryFactoryResolver.Current = new CultureDictionaryFactoryResolver(
                new DefaultCultureDictionaryFactory());

        }

    }
}
