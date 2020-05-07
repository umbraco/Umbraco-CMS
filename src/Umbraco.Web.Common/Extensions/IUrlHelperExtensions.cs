using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Web.Common.Controllers;
using Umbraco.Extensions;
using Umbraco.Web.WebApi;

namespace Umbraco.Extensions
{
    public static class HttpUrlHelperExtensions
    {
        /// <summary>
        /// Return the Url for a Web Api service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="umbracoApiControllerTypeCollection"></param>
        /// <param name="actionName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService<T>(this IUrlHelper url, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, string actionName, object id = null)
            where T : UmbracoApiController
        {
            return url.GetUmbracoApiService(umbracoApiControllerTypeCollection, actionName, typeof(T), id);
        }

        public static string GetUmbracoApiService<T>(this IUrlHelper url, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, Expression<Func<T, object>> methodSelector)
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
                return url.GetUmbracoApiService<T>(umbracoApiControllerTypeCollection, method.Name);
            }
            return url.GetUmbracoApiService<T>(umbracoApiControllerTypeCollection, method.Name, methodParams.Values.First());
        }

        /// <summary>
        /// Return the Url for a Web Api service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="umbracoApiControllerTypeCollection"></param>
        /// <param name="actionName"></param>
        /// <param name="apiControllerType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService(this IUrlHelper url, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, string actionName, Type apiControllerType, object id = null)
        {
            if (actionName == null) throw new ArgumentNullException(nameof(actionName));
            if (string.IsNullOrWhiteSpace(actionName)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(actionName));
            if (apiControllerType == null) throw new ArgumentNullException(nameof(apiControllerType));

            var area = "";

            var apiController = umbracoApiControllerTypeCollection.SingleOrDefault(x => x == apiControllerType);
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
        public static string GetUmbracoApiService(this IUrlHelper url, string actionName, string controllerName, object id = null)
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
        public static string GetUmbracoApiService(this IUrlHelper url, string actionName, string controllerName, string area, object id = null)
        {
            if (actionName == null) throw new ArgumentNullException(nameof(actionName));
            if (string.IsNullOrWhiteSpace(actionName)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(actionName));
            if (controllerName == null) throw new ArgumentNullException(nameof(controllerName));
            if (string.IsNullOrWhiteSpace(controllerName)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(controllerName));

            string routeName;
            if (area.IsNullOrWhiteSpace())
            {
                routeName = string.Format("umbraco-{0}-{1}", "api", controllerName);
                if (id == null)
                {

                    return url.RouteUrl(routeName, new { controller = controllerName, action = actionName, httproute = "" });
                }
                else
                {
                    return url.RouteUrl(routeName, new { controller = controllerName, action = actionName, id = id, httproute = "" });
                }
            }
            else
            {
                routeName = string.Format("umbraco-{0}-{1}-{2}", "api", area, controllerName);
                if (id == null)
                {
                    return url.RouteUrl(routeName, new { controller = controllerName, action = actionName, httproute = "" });
                }
                else
                {
                    return url.RouteUrl(routeName, new { controller = controllerName, action = actionName, id = id, httproute = "" });
                }
            }
        }

        /// <summary>
        /// Return the Base Url (not including the action) for a Web Api service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="umbracoApiControllerTypeCollection"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public static string GetUmbracoApiServiceBaseUrl<T>(this IUrlHelper url, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, string actionName)
            where T : UmbracoApiController
        {
            return url.GetUmbracoApiService<T>(umbracoApiControllerTypeCollection, actionName).TrimEnd(actionName);
        }

        public static string GetUmbracoApiServiceBaseUrl<T>(this IUrlHelper url, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, Expression<Func<T, object>> methodSelector)
            where T : UmbracoApiController
        {
            var method = ExpressionHelper.GetMethodInfo(methodSelector);
            if (method == null)
            {
                throw new MissingMethodException("Could not find the method " + methodSelector + " on type " + typeof(T) + " or the result ");
            }
            return url.GetUmbracoApiService<T>(umbracoApiControllerTypeCollection, method.Name).TrimEnd(method.Name);
        }
    }
}
