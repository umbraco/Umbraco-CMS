using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Web
{
	internal static class AreaRegistrationExtensions
	{
		/// <summary>
		/// Creates a custom individual route for the specified controller plugin. Individual routes
		/// are required by controller plugins to map to a unique URL based on ID.
		/// </summary>
		/// <param name="controllerName"></param>
		/// <param name="controllerType"></param>
		/// <param name="routes">An existing route collection</param>
		/// <param name="controllerSuffixName">
		/// The suffix name that the controller name must end in before the "Controller" string for example:
		/// ContentTreeController has a controllerSuffixName of "Tree", this is used for route constraints.
		/// </param>
		/// <param name="defaultAction"></param>
		/// <param name="defaultId"></param>
		/// <param name="area"></param>
		/// <param name="umbracoTokenValue">The DataToken value to set for the 'umbraco' key, this defaults to 'backoffice' </param>
		/// <remarks>
		/// </remarks>
		internal static void RouteControllerPlugin(this AreaRegistration area, string controllerName, Type controllerType, RouteCollection routes,
		                                           string controllerSuffixName, string defaultAction, object defaultId,
		                                           string umbracoTokenValue = "backoffice")
		{
			Mandate.ParameterNotNullOrEmpty(controllerName, "controllerName");
			Mandate.ParameterNotNullOrEmpty(controllerSuffixName, "controllerSuffixName");
			Mandate.ParameterNotNullOrEmpty(defaultAction, "defaultAction");
			Mandate.ParameterNotNull(controllerType, "controllerType");
			Mandate.ParameterNotNull(routes, "routes");
			Mandate.ParameterNotNull(defaultId, "defaultId");

			var umbracoArea = GlobalSettings.UmbracoMvcArea;

			//routes are explicitly name with controller names and IDs
			var url = umbracoArea + "/" + area.AreaName + "/" + controllerName + "/{action}/{id}"; 

			//create a new route with custom name, specified url, and the namespace of the controller plugin
			var controllerPluginRoute = routes.MapRoute(
				//name
				string.Format("umbraco-{0}", controllerType.FullName),
				//url format
				url,
				//set the namespace of the controller to match
				new[] { controllerType.Namespace });
			
			//set defaults
			controllerPluginRoute.Defaults = new RouteValueDictionary(
				new Dictionary<string, object>
					{
						{ "controller", controllerName },
						{ "controllerType", controllerType.FullName },
						{ "action", defaultAction },
						{ "id", defaultId }    
					});

			//constraints: only match controllers ending with 'controllerSuffixName' and only match this controller's ID for this route            
			controllerPluginRoute.Constraints = new RouteValueDictionary(
				new Dictionary<string, object>
					{
						{ "controller", @"(\w+)" + controllerSuffixName },
						{ "controllerType", controllerType.FullName }
					});
			
			
			//match this area
			controllerPluginRoute.DataTokens.Add("area", area.AreaName);
			controllerPluginRoute.DataTokens.Add("umbraco", umbracoTokenValue); //ensure the umbraco token is set

		}
	}
}