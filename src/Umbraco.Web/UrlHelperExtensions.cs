using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Instrumentation;
using System.Web.Mvc;
using System.Web.Routing;
using ClientDependency.Core.Config;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebServices;

namespace Umbraco.Web
{
    /// <summary>
    /// Extension methods for UrlHelper
    /// </summary>
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Returns the base path (not including the 'action') of the MVC controller "ExamineManagementController"
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetExamineManagementServicePath(this UrlHelper url)
        {
            var result = url.GetUmbracoApiService<ExamineManagementApiController>("GetIndexerDetails");
            return result.TrimEnd("GetIndexerDetails").EnsureEndsWith('/');
        }

        /// <summary>
        /// Return the Url for a Web Api service
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

        /// <summary>
        /// Return the Base Url (not including the action) for a Web Api service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public static string GetUmbracoApiServiceBaseUrl<T>(this UrlHelper url, string actionName)
            where T : UmbracoApiController
        {
            return url.GetUmbracoApiService<T>(actionName).TrimEnd(actionName);
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
        /// Return the Url for a Web Api service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="apiControllerType"></param>
        /// <param name="routeVals"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService(this UrlHelper url, string actionName, Type apiControllerType, RouteValueDictionary routeVals = null)
        {
            Mandate.ParameterNotNullOrEmpty(actionName, "actionName");
            Mandate.ParameterNotNull(apiControllerType, "apiControllerType");

            var area = "";

            var apiController = UmbracoApiControllerResolver.Current.RegisteredUmbracoApiControllers
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
        /// Return the Url for a Web Api service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeVals"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService(this UrlHelper url, string actionName, string controllerName, RouteValueDictionary routeVals = null)
        {
            return url.GetUmbracoApiService(actionName, controllerName, "", routeVals);
        }

        /// <summary>
        /// Return the Url for a Web Api service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="area"></param>
        /// <param name="routeVals"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService(this UrlHelper url, string actionName, string controllerName, string area, RouteValueDictionary routeVals = null)
        {
            Mandate.ParameterNotNullOrEmpty(controllerName, "controllerName");
            Mandate.ParameterNotNullOrEmpty(actionName, "actionName");

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
        /// Return the Url for an action with a cache-busting hash appended
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
            //in case the user bypasses the installer and just bumps the web.config or clientdep config
            
            var versionHash = new HashCodeCombiner();

            //if in debug mode, always burst the cache
            if (GlobalSettings.DebugMode)
            {
                versionHash.AddCaseInsensitiveString(DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                //create a unique hash code of the current umb version and the current cdf version
                
                versionHash.AddCaseInsensitiveString(UmbracoVersion.Current.ToString());
                versionHash.AddCaseInsensitiveString(ClientDependencySettings.Instance.Version.ToString(CultureInfo.InvariantCulture));                
            }

            return versionHash.GetCombinedHashCode();
        }
    }
}