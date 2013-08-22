using System;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Core;
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
        /// <returns></returns>
        public static string GetUmbracoApiService<T>(this UrlHelper url, string actionName)
            where T : UmbracoApiController
        {
            return url.GetUmbracoApiService(actionName, typeof (T));
        }

        /// <summary>
        /// Return the Url for a Web Api service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="apiControllerType"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService(this UrlHelper url, string actionName, Type apiControllerType)
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
            return url.GetUmbracoApiService(actionName, ControllerExtensions.GetControllerName(apiControllerType), area);
        }

        /// <summary>
        /// Return the Url for a Web Api service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService(this UrlHelper url, string actionName, string controllerName)
        {            
            return url.GetUmbracoApiService(actionName, controllerName, "");
        }

        /// <summary>
        /// Return the Url for a Web Api service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService(this UrlHelper url, string actionName, string controllerName, string area)
        {
            Mandate.ParameterNotNullOrEmpty(controllerName, "controllerName");
            Mandate.ParameterNotNullOrEmpty(actionName, "actionName");
            return url.Action(actionName, controllerName, new { httproute = "", area = area });
        }

    }
}