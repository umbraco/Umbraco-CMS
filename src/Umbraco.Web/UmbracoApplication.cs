using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;
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
					IsReady = true	// fixme, do we need this?
				};

				//find and initialize the application startup handlers
				ApplicationStartupHandler.RegisterHandlers();

				OnApplicationStarting(sender, e);				
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
	
		protected virtual void Application_Error(object sender, EventArgs e)
		{

		}

		protected virtual void Application_End(object sender, EventArgs e)
		{

		}
	}
}
