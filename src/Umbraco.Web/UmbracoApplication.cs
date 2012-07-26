using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Web.Routing;

namespace Umbraco.Web
{
	/// <summary>
	/// The Umbraco global.asax class
	/// </summary>
	public class UmbracoApplication : System.Web.HttpApplication
	{

		private static readonly TraceSource Trace = new TraceSource("UmbracoApplication");

		/// <summary>
		/// Initializes the Umbraco application
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void Application_Start(object sender, EventArgs e)
		{
			Trace.TraceInformation("Initialize AppDomain");

			// Backwards compatibility - set the path and URL type for ClientDependency 1.5.1 [LK]
			ClientDependency.Core.CompositeFiles.Providers.XmlFileMapper.FileMapVirtualFolder = "~/App_Data/TEMP/ClientDependency";
			ClientDependency.Core.CompositeFiles.Providers.BaseCompositeFileProcessingProvider.UrlTypeDefault = ClientDependency.Core.CompositeFiles.Providers.CompositeUrlType.Base64QueryStrings;

			//create the ApplicationContext
			ApplicationContext.Current = new ApplicationContext(new PluginResolver())
				{
					IsReady = true	// fixme
				};

			// create the resolvers
			DocumentLookupsResolver.Current = new DocumentLookupsResolver(ApplicationContext.Current.Plugins.ResolveLookups(), new DefaultLastChanceLookup());
			RoutesCacheResolver.Current = new RoutesCacheResolver(new DefaultRoutesCache());

			Umbraco.Core.Resolving.Resolution.Freeze();

			Trace.TraceInformation("AppDomain is initialized");
		}

		protected virtual void Application_Error(object sender, EventArgs e)
		{

		}

		protected virtual void Application_End(object sender, EventArgs e)
		{

		}
	}
}
