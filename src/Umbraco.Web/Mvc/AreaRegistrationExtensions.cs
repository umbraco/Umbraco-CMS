using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Mvc
{
    internal static class AreaRegistrationExtensions
    {
        /// <summary>
        /// Creates a custom individual route for the specified controller plugin. Individual routes
        /// are required by controller plugins to map to a unique URL based on ID.
        /// </summary>
        /// <param name="globalSettings"></param>
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
        internal static Route RouteControllerPlugin(this AreaRegistration area,
            IGlobalSettings globalSettings,
            string controllerName, Type controllerType, RouteCollection routes,
            string controllerSuffixName, string defaultAction, object defaultId,
            string umbracoTokenValue = "backoffice",
            string routeTokens = "{action}/{id}",
            bool isMvc = true,
            string areaPathPrefix = "")
        {
            if (controllerName == null) throw new ArgumentNullException(nameof(controllerName));
            if (string.IsNullOrEmpty(controllerName)) throw new ArgumentException("Value can't be empty.", nameof(controllerName));
            if (controllerSuffixName == null) throw new ArgumentNullException(nameof(controllerSuffixName));

            if (controllerType == null) throw new ArgumentNullException(nameof(controllerType));
            if (routes == null) throw new ArgumentNullException(nameof(routes));
            if (defaultId == null) throw new ArgumentNullException(nameof(defaultId));

            var umbracoArea = globalSettings.GetUmbracoMvcArea();

            //routes are explicitly named with controller names and IDs
            var url = umbracoArea + "/" +
                      (areaPathPrefix.IsNullOrWhiteSpace() ? "" : areaPathPrefix + "/") +
                      area.AreaName + "/" + controllerName + "/" + routeTokens;

            Route controllerPluginRoute;
            //var meta = PluginController.GetMetadata(controllerType);
            if (isMvc)
            {
                //create a new route with custom name, specified URL, and the namespace of the controller plugin
                controllerPluginRoute = routes.MapRoute(
                    //name
                    string.Format("umbraco-{0}-{1}", area.AreaName, controllerName),
                    //URL format
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
                    //URL format
                    url,
                    new {controller = controllerName, id = defaultId});
                //web api routes don't set the data tokens object
                if (controllerPluginRoute.DataTokens == null)
                {
                    controllerPluginRoute.DataTokens = new RouteValueDictionary();
                }

                //look in this namespace to create the controller
                controllerPluginRoute.DataTokens.Add("Namespaces", new[] {controllerType.Namespace});

                //Special case! Check if the controller type implements IRequiresSessionState and if so use our
                //custom webapi session handler
                if (typeof(IRequiresSessionState).IsAssignableFrom(controllerType))
                {
                    controllerPluginRoute.RouteHandler = new SessionHttpControllerRouteHandler();
                }
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
            controllerPluginRoute.DataTokens.Add(Core.Constants.Web.UmbracoDataToken, umbracoTokenValue); //ensure the umbraco token is set

            return controllerPluginRoute;
        }
    }
}
