using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Web.Routing;
using umbraco.businesslogic;

namespace Umbraco.Web
{
	/// <summary>
	/// The Umbraco global.asax class
	/// </summary>
	public class UmbracoApplication : System.Web.HttpApplication
	{
		public static event EventHandler ApplicationStarting;
		
		public static event EventHandler ApplicationStarted;

		/// <summary>
		/// Initializes the Umbraco application
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_Start(object sender, EventArgs e)
		{
			using (DisposableTimer.TraceDuration<UmbracoApplication>(
				"Umbraco application starting", 
				"Umbraco application startup complete"))
			{
				// Backwards compatibility - set the path and URL type for ClientDependency 1.5.1 [LK]
				ClientDependency.Core.CompositeFiles.Providers.XmlFileMapper.FileMapVirtualFolder = "~/App_Data/TEMP/ClientDependency";
				ClientDependency.Core.CompositeFiles.Providers.BaseCompositeFileProcessingProvider.UrlTypeDefault = ClientDependency.Core.CompositeFiles.Providers.CompositeUrlType.Base64QueryStrings;

				//create the ApplicationContext
				ApplicationContext.Current = new ApplicationContext()
				{
					IsReady = true	// fixme
				};

				//find and initialize the application startup handlers
				ApplicationStartupHandler.RegisterHandlers();

				// create the resolvers...
				
				LastChanceLookupResolver.Current = new LastChanceLookupResolver(new DefaultLastChanceLookup());

				DocumentLookupsResolver2.Current = new DocumentLookupsResolver2(
					//add all known resolvers in the correct order, devs can then modify this list on application startup either by binding to events
					//or in their own global.asax
					new IDocumentLookup[]
						{
							new LookupByNiceUrl(),
							new LookupById(), 
							new LookupByNiceUrlAndTemplate(),
 							new LookupByProfile(),
							new LookupByAlias()  
						},
					LastChanceLookupResolver.Current);
				RoutesCacheResolver.Current = new RoutesCacheResolver(new DefaultRoutesCache());

				OnApplicationStarting(sender, e);

				//all resolvers are now frozen and cannot be modified
				Umbraco.Core.Resolving.Resolution.Freeze();

				OnApplicationStarted(sender, e);
			}
		}

		/// <summary>
		/// Developers can override this method to modify objects on startup
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void OnApplicationStarting(object sender, EventArgs e)
		{
			if (ApplicationStarting != null)
				ApplicationStarting(sender, e);
		}

		/// <summary>
		/// Developers can override this method to do anything they need to do once the application startup routine is completed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void OnApplicationStarted(object sender, EventArgs e)
		{
			if (ApplicationStarted != null)
				ApplicationStarted(sender, e);
		}

		protected virtual void Application_Error(object sender, EventArgs e)
		{

		}

		protected virtual void Application_End(object sender, EventArgs e)
		{

		}
	}
}
