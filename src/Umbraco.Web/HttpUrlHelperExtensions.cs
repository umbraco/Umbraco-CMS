using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http.Routing;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.Web
{
    public static class HttpUrlHelperExtensions
    {
        /// <summary>
        /// Return the Url for a Web Api service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService<T>(this UrlHelper url, string actionName, object id = null)
            where T : UmbracoApiController
        {
            return url.GetUmbracoApiService(actionName, typeof(T), id);
        }

        public static string GetUmbracoApiService<T>(this UrlHelper url, Expression<Func<T, object>> methodSelector)
            where T : UmbracoApiController
        {
            var method = ExpressionHelper.GetMethodInfo(methodSelector);
            var methodParams = ExpressionHelper.GetMethodParams(methodSelector);
            if (method == null)
            {
                throw new MissingMethodException("Could not find the method " + methodSelector + " on type " + typeof(T) + " or the result ");
            }

            if (methodParams.Any() == false)
            {
                return url.GetUmbracoApiService<T>(method.Name);    
            }
            return url.GetUmbracoApiService<T>(method.Name, methodParams.Values.First());
        }

        /// <summary>
        /// Return the Url for a Web Api service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="apiControllerType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService(this UrlHelper url, string actionName, Type apiControllerType, object id = null)
        {
            Mandate.ParameterNotNullOrEmpty(actionName, "actionName");
            Mandate.ParameterNotNull(apiControllerType, "apiControllerType");

            var area = "";

            var apiController = UmbracoApiControllerResolver.Current.RegisteredUmbracoApiControllers
                .SingleOrDefault(x => x == apiControllerType);
            if (apiController == null)
                throw new InvalidOperationException("Could not find the umbraco api controller of type " + apiControllerType.FullName);
            var metaData = PluginController.GetMetadata(apiController);
            if (metaData.AreaName.IsNullOrWhiteSpace() == false)
            {
                //set the area to the plugin area
                area = metaData.AreaName;
            }
            return url.GetUmbracoApiService(actionName, ControllerExtensions.GetControllerName(apiControllerType), area, id);
        }

        /// <summary>
        /// Return the Url for a Web Api service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService(this UrlHelper url, string actionName, string controllerName, object id = null)
        {
            return url.GetUmbracoApiService(actionName, controllerName, "", id);
        }

        /// <summary>
        /// Return the Url for a Web Api service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="area"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService(this UrlHelper url, string actionName, string controllerName, string area, object id = null)
        {
            Mandate.ParameterNotNullOrEmpty(controllerName, "controllerName");
            Mandate.ParameterNotNullOrEmpty(actionName, "actionName");

            string routeName;
            if (area.IsNullOrWhiteSpace())
            {
                routeName = string.Format("umbraco-{0}-{1}", "api", controllerName);
                if (id == null)
                {
                    return url.Route(routeName, new { controller = controllerName, action = actionName, httproute = "" });
                }
                else
                {
                    return url.Route(routeName, new { controller = controllerName, action = actionName, id = id, httproute = "" });
                }
            }
            else
            {
                routeName = string.Format("umbraco-{0}-{1}-{2}", "api", area, controllerName);
                if (id == null)
                {
                    return url.Route(routeName, new { controller = controllerName, action = actionName, httproute = "" });
                }
                else
                {
                    return url.Route(routeName, new { controller = controllerName, action = actionName, id = id, httproute = "" });
                }
            }
        }
    }
}