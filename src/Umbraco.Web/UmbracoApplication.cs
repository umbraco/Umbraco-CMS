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

		protected virtual void Application_Error(object sender, EventArgs e)
		{

		}

		protected virtual void Application_End(object sender, EventArgs e)
		{

		}
	}
}
