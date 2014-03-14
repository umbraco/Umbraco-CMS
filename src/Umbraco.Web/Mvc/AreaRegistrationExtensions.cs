using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.Mvc
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
        /// <param name="routeTokens">By default this value is just {action}/{id} but can be modified for things like web api routes</param>
        /// <param name="isMvc">Default is true for MVC, otherwise false for WebAPI</param>
        /// <param name="areaPathPrefix">
        /// If specified will add this string to the path between the umbraco path and the area path name, for example:
        ///     /umbraco/CUSTOMPATHPREFIX/areaname
        /// if not specified, will just route like:
        ///     /umbraco/areaname
        /// </param>
        /// <remarks>
        /// </remarks>
        internal static Route RouteControllerPlugin(this AreaRegistration area, string controllerName, Type controllerType, RouteCollection routes,
                                                    string controllerSuffixName, string defaultAction, object defaultId,
                                                    string umbracoTokenValue = "backoffice",
                                                    string routeTokens = "{action}/{id}",
                                                    bool isMvc = true,
                                                    string areaPathPrefix = "")
        {
            Mandate.ParameterNotNullOrEmpty(controllerName, "controllerName");
            Mandate.ParameterNotNull(controllerSuffixName, "controllerSuffixName");
            
            Mandate.ParameterNotNull(controllerType, "controllerType");
            Mandate.ParameterNotNull(routes, "routes");
            Mandate.ParameterNotNull(defaultId, "defaultId");

            var umbracoArea = GlobalSettings.UmbracoMvcArea;

            //routes are explicitly named with controller names and IDs
            var url = umbracoArea + "/" + 
                (areaPathPrefix.IsNullOrWhiteSpace() ? "" : areaPathPrefix + "/") + 
                area.AreaName + "/" + controllerName + "/" + routeTokens;

            Route controllerPluginRoute;
            //var meta = PluginController.GetMetadata(controllerType);
            if (isMvc)
            {
                //create a new route with custom name, specified url, and the namespace of the controller plugin
                controllerPluginRoute = routes.MapRoute(
                    //name
                    string.Format("umbraco-{0}-{1}", area.AreaName, controllerName),
                    //url format
                    url,
                    //set the namespace of the controller to match
                    new[] {controllerType.Namespace});

                //set defaults
                controllerPluginRoute.Defaults = new RouteValueDictionary(
                    new Dictionary<string, object>
                    {
                        {"controller", controllerName},
                        {"action", defaultAction},
                        {"id", defaultId}
                    });
            }
            else
            {
                controllerPluginRoute = routes.MapHttpRoute(
                    //name
                    string.Format("umbraco-{0}-{1}-{2}", "api", area.AreaName, controllerName),
                    //url format
                    url,
                    new { controller = controllerName, id = defaultId });
                //web api routes don't set the data tokens object
                if (controllerPluginRoute.DataTokens == null)
                {
                    controllerPluginRoute.DataTokens = new RouteValueDictionary();
                }
                //look in this namespace to create the controller
                controllerPluginRoute.DataTokens.Add("Namespaces", new[] {controllerType.Namespace});
            }

            //Don't look anywhere else except this namespace!
            controllerPluginRoute.DataTokens.Add("UseNamespaceFallback", false);

            //constraints: only match controllers ending with 'controllerSuffixName' and only match this controller's ID for this route            
            if (controllerSuffixName.IsNullOrWhiteSpace() == false)
            {                
            controllerPluginRoute.Constraints = new RouteValueDictionary(
                new Dictionary<string, object>
                    {
                        {"controller", @"(\w+)" + controllerSuffixName}
                    });
            }

            //match this area
            controllerPluginRoute.DataTokens.Add("area", area.AreaName);
            controllerPluginRoute.DataTokens.Add("umbraco", umbracoTokenValue); //ensure the umbraco token is set

            return controllerPluginRoute;
        }
    }
}