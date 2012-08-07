using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
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
			var route = RouteTable.Routes.MapRoute(
				"Umbraco_default",
				"Umbraco/RenderMvc/{action}/{id}",
				new { controller = "RenderMvc", action = "Index", id = UrlParameter.Optional }
				);
			route.RouteHandler = new RenderRouteHandler(ControllerBuilder.Current.GetControllerFactory());

			//find and initialize the application startup handlers
			ApplicationStartupHandler.RegisterHandlers();

			return this;
		}

		/// <summary>
		/// Initializes all web based and core resolves 
		/// </summary>
		protected override void InitializeResolvers()
		{
			base.InitializeResolvers();

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
						typeof (LookupByNiceUrl),
						typeof (LookupById),
						typeof (LookupByNiceUrlAndTemplate),
						typeof (LookupByProfile),
						typeof (LookupByAlias)
					});

			RoutesCacheResolver.Current = new RoutesCacheResolver(new DefaultRoutesCache());

			ThumbnailProvidersResolver.Current = new ThumbnailProvidersResolver(
				PluginManager.Current.ResolveThumbnailProviders());
		}

	}
}
