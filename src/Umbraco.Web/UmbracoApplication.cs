using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Routing;
using umbraco.businesslogic;

namespace Umbraco.Web
{
	/// <summary>
	/// The Umbraco global.asax class
	/// </summary>
	public class UmbracoApplication : System.Web.HttpApplication
	{
		public UmbracoApplication()
		{
			_bootManager = new WebBootManager(this);
		}

		private readonly IBootManager _bootManager;

		public static event EventHandler ApplicationStarting;		
		public static event EventHandler ApplicationStarted;

		/// <summary>
		/// Initializes the Umbraco application
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_Start(object sender, EventArgs e)
		{
            //don't output the MVC version header (security)
            MvcHandler.DisableMvcResponseHeader = true;

			//boot up the application
			_bootManager
				.Initialize()
				.Startup(appContext => OnApplicationStarting(sender, e))
				.Complete(appContext => OnApplicationStarted(sender, e));
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

		/// <summary>
		/// A method that can be overridden to invoke code when the application has an error.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void OnApplicationError(object sender, EventArgs e)
		{
			
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			// Code that runs when an unhandled error occurs

			// Get the exception object.
			var exc = Server.GetLastError();

			// Ignore HTTP errors
			if (exc.GetType() == typeof(HttpException))
			{
				return;
			}
			
			LogHelper.Error<UmbracoApplication>("An unhandled exception occurred", exc);
			
			OnApplicationError(sender, e);
		}

		/// <summary>
		/// A method that can be overridden to invoke code when the application shuts down.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void OnApplicationEnd(object sender, EventArgs e)
		{
			
		}

		protected void Application_End(object sender, EventArgs e)
		{
			if (SystemUtilities.GetCurrentTrustLevel() == AspNetHostingPermissionLevel.Unrestricted)
			{
				LogHelper.Info<UmbracoApplication>("Application shutdown. Reason: " + HostingEnvironment.ShutdownReason);
			}
			OnApplicationEnd(sender, e);
		}
	}
}
