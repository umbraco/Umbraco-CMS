using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;
using ClientDependency.Core.Config;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Composing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.Web
{
    /// <summary>
    /// Extension methods for UrlHelper
    /// </summary>
    public static class UrlHelperExtensions
    {

        /// <summary>
        /// Return the URL for a Web Api service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="routeVals"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService<T>(this UrlHelper url, string actionName, RouteValueDictionary routeVals = null)
            where T : UmbracoApiController
        {
            return url.GetUmbracoApiService(actionName, typeof(T), routeVals);
        }

        public static string GetUmbracoApiServiceBaseUrl<T>(this UrlHelper url, Expression<Func<T, object>> methodSelector)
            where T : UmbracoApiController
        {
            var method = Core.ExpressionHelper.GetMethodInfo(methodSelector);
            if (method == null)
            {
                throw new MissingMethodException("Could not find the method " + methodSelector + " on type " + typeof(T) + " or the result ");
            }
            return url.GetUmbracoApiService<T>(method.Name).TrimEnd(method.Name);
        }

        public static string GetUmbracoApiService<T>(this UrlHelper url, Expression<Func<T, object>> methodSelector)
            where T : UmbracoApiController
        {
            var method = Core.ExpressionHelper.GetMethodInfo(methodSelector);
            if (method == null)
            {
                throw new MissingMethodException("Could not find the method " + methodSelector + " on type " + typeof(T) + " or the result ");
            }
            var parameters = Core.ExpressionHelper.GetMethodParams(methodSelector);
            var routeVals = new RouteValueDictionary(parameters);
            return url.GetUmbracoApiService<T>(method.Name, routeVals);
        }

        /// <summary>
        /// Return the URL for a Web Api service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="apiControllerType"></param>
        /// <param name="routeVals"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService(this UrlHelper url, string actionName, Type apiControllerType, RouteValueDictionary routeVals = null)
        {
            if (actionName == null) throw new ArgumentNullException(nameof(actionName));
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentException("Value can't be empty.", nameof(actionName));
            if (apiControllerType == null) throw new ArgumentNullException(nameof(apiControllerType));

            var area = "";

            var apiController = Current.UmbracoApiControllerTypes
                .SingleOrDefault(x => x == apiControllerType);
            if (apiController == null)
                throw new InvalidOperationException("Could not find the umbraco api controller of type " + apiControllerType.FullName);
            var metaData = PluginController.GetMetadata(apiController);
            if (!metaData.AreaName.IsNullOrWhiteSpace())
            {
                //set the area to the plugin area
                area = metaData.AreaName;
            }
            return url.GetUmbracoApiService(actionName, ControllerExtensions.GetControllerName(apiControllerType), area, routeVals);
        }

        /// <summary>
        /// Return the URL for a Web Api service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="area"></param>
        /// <param name="routeVals"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService(this UrlHelper url, string actionName, string controllerName, string area, RouteValueDictionary routeVals = null)
        {
            if (actionName == null) throw new ArgumentNullException(nameof(actionName));
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentException("Value can't be empty.", nameof(actionName));
            if (controllerName == null) throw new ArgumentNullException(nameof(controllerName));
            if (string.IsNullOrEmpty(controllerName)) throw new ArgumentException("Value can't be empty.", nameof(controllerName));

            if (routeVals == null)
            {
                routeVals = new RouteValueDictionary(new {httproute = "", area = area});
            }
            else
            {
                var requiredRouteVals = new RouteValueDictionary(new { httproute = "", area = area });
                requiredRouteVals.MergeLeft(routeVals);
                //copy it back now
                routeVals = requiredRouteVals;
            }

            return url.Action(actionName, controllerName, routeVals);
        }


        /// <summary>
        /// Return the URL for an action with a cache-busting hash appended
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeVals"></param>
        /// <returns></returns>
        public static string GetUrlWithCacheBust(this UrlHelper url, string actionName, string controllerName, RouteValueDictionary routeVals = null)
        {
            var applicationJs = url.Action(actionName, controllerName, routeVals);
            applicationJs = applicationJs + "?umb__rnd=" + GetCacheBustHash();
            return applicationJs;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public static string GetCacheBustHash()
        {
            //make a hash of umbraco and client dependency version
            //in case the user bypasses the installer and just bumps the web.config or client dependency config

            //if in debug mode, always burst the cache
            if (GlobalSettings.DebugMode)
            {
                return DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture).GenerateHash();
            }

            var version = Current.RuntimeState.SemanticVersion.ToSemanticString();
            return $"{version}.{ClientDependencySettings.Instance.Version}".GenerateHash();
        }
    }
}
