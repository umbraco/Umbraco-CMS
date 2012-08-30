using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Dynamics;
using Umbraco.Web.Dictionary;
using Umbraco.Web.Media.ThumbnailProviders;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using umbraco.businesslogic;

namespace Umbraco.Web
{
	/// <summary>
	/// A bootstrapper for the Umbraco application which initializes all objects including the Web portion of the application 
	/// </summary>
	internal class WebBootManager : CoreBootManager
	{
		private readonly UmbracoApplication _umbracoApplication;

		public WebBootManager(UmbracoApplication umbracoApplication)
		{
			_umbracoApplication = umbracoApplication;
			if (umbracoApplication == null) throw new ArgumentNullException("umbracoApplication");
		}

		public void Boot()
		{
			InitializeResolvers();
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

			//set model binder
			ModelBinders.Binders.Add(new KeyValuePair<Type, IModelBinder>(typeof(RenderModel), new RenderModelBinder()));

			//set routes
			CreateRoutes();

			//find and initialize the application startup handlers, we need to initialize this resolver here because
			//it is a special resolver where they need to be instantiated first before any other resolvers in order to bind to 
			//events and to call their events during bootup.
			//ApplicationStartupHandler.RegisterHandlers();
			ApplicationEventsResolver.Current = new ApplicationEventsResolver(
				PluginManager.Current.ResolveApplicationStartupHandlers());

			//set the special flag to let us resolve before frozen resolution
			ApplicationEventsResolver.Current.CanResolveBeforeFrozen = true;

			//now we need to call the initialize methods
			ApplicationEventsResolver.Current.ApplicationEventHandlers
				.ForEach(x => x.OnApplicationInitialized(_umbracoApplication, ApplicationContext));

			return this;
		}

		/// <summary>
		/// Ensure that the OnApplicationStarting methods of the IApplicationEvents are called
		/// </summary>
		/// <param name="afterStartup"></param>
		/// <returns></returns>
		public override IBootManager Startup(Action<ApplicationContext> afterStartup)
		{
			base.Startup(afterStartup);

			//call OnApplicationStarting of each application events handler
			ApplicationEventsResolver.Current.ApplicationEventHandlers
				.ForEach(x => x.OnApplicationStarting(_umbracoApplication, ApplicationContext));

			return this;
		}

		/// <summary>
		/// Ensure that the OnApplicationStarted methods of the IApplicationEvents are called
		/// </summary>
		/// <param name="afterComplete"></param>
		/// <returns></returns>
		public override IBootManager Complete(Action<ApplicationContext> afterComplete)
		{
			base.Complete(afterComplete);

			//call OnApplicationStarting of each application events handler
			ApplicationEventsResolver.Current.ApplicationEventHandlers
				.ForEach(x => x.OnApplicationStarted(_umbracoApplication, ApplicationContext));

			return this;
		}

		/// <summary>
		/// Creates the routes
		/// </summary>
		protected internal void CreateRoutes()
		{
			//set routes
			var route = RouteTable.Routes.MapRoute(
				"Umbraco_default",
				"Umbraco/RenderMvc/{action}/{id}",
				new { controller = "RenderMvc", action = "Index", id = UrlParameter.Optional }
				);
			route.RouteHandler = new RenderRouteHandler(ControllerBuilder.Current.GetControllerFactory());
		}

		/// <summary>
		/// Initializes all web based and core resolves 
		/// </summary>
		protected override void InitializeResolvers()
		{
			base.InitializeResolvers();

			ContentStoreResolver.Current = new ContentStoreResolver(new XmlPublishedContentStore());

			FilteredControllerFactoriesResolver.Current = new FilteredControllerFactoriesResolver(
				//add all known factories, devs can then modify this list on application startup either by binding to events
				//or in their own global.asax
				new[]
					{
						typeof (RenderControllerFactory)
					});

			LastChanceLookupResolver.Current = new LastChanceLookupResolver(new DefaultLastChanceLookup());

			DocumentLookupsResolver.Current = new DocumentLookupsResolver(
				//add all known resolvers in the correct order, devs can then modify this list on application startup either by binding to events
				//or in their own global.asax
				new[]
					{
						typeof (LookupByPageIdQuery),
						typeof (LookupByNiceUrl),
						typeof (LookupByIdPath),
						typeof (LookupByNiceUrlAndTemplate),
						typeof (LookupByProfile),
						typeof (LookupByAlias)
					});

			RoutesCacheResolver.Current = new RoutesCacheResolver(new DefaultRoutesCache());

			ThumbnailProvidersResolver.Current = new ThumbnailProvidersResolver(
				PluginManager.Current.ResolveThumbnailProviders());

			CultureDictionaryFactoryResolver.Current = new CultureDictionaryFactoryResolver(
				new DefaultCultureDictionaryFactory());

			//This exists only because the new business logic classes aren't created yet and we want Dynamics in the Core project,
			//see the note in the DynamicNodeDataSourceResolver.cs class
			DynamicDocumentDataSourceResolver.Current = new DynamicDocumentDataSourceResolver(new DefaultDynamicDocumentDataSource());
		}

	}
}
