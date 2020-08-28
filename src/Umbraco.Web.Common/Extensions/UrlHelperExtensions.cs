﻿using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.WebApi;
using Umbraco.Web.Common.Install;
using Microsoft.AspNetCore.Routing;
using Umbraco.Core.Hosting;
using System.Globalization;
using Umbraco.Core.Configuration;
using Umbraco.Core.WebAssets;

namespace Umbraco.Extensions
{

    public static class UrlHelperExtensions
    {

        

        /// <summary>
        /// Return the back office url if the back office is installed
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetBackOfficeUrl(this IUrlHelper url)
        {
            var backOfficeControllerType = Type.GetType("Umbraco.Web.BackOffice.Controllers");
            if (backOfficeControllerType == null) return "/"; // this would indicate that the installer is installed without the back office
            return url.Action("Default", ControllerExtensions.GetControllerName(backOfficeControllerType), new { area = Constants.Web.Mvc.BackOfficeApiArea });
        }

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

            if (area.IsNullOrWhiteSpace())
            {
                if (id == null)
                {
                    return url.Action(actionName, controllerName);
                }
                else
                {
                    return url.Action(actionName, controllerName, new { id = id });
                }
            }
            else
            {
                if (id == null)
                {
                    return url.Action(actionName, controllerName, new { area = area });
                }
                else
                {
                    return url.Action(actionName, controllerName, new { area = area, id = id });
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

        /// <summary>
        /// Return the Url for an action with a cache-busting hash appended
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeVals"></param>
        /// <returns></returns>
        public static string GetUrlWithCacheBust(this IUrlHelper url, string actionName, string controllerName, RouteValueDictionary routeVals,
            IHostingEnvironment hostingEnvironment, IUmbracoVersion umbracoVersion, IRuntimeMinifier runtimeMinifier)
        {
            var applicationJs = url.Action(actionName, controllerName, routeVals);
            applicationJs = applicationJs + "?umb__rnd=" + GetCacheBustHash(hostingEnvironment, umbracoVersion, runtimeMinifier);
            return applicationJs;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public static string GetCacheBustHash(IHostingEnvironment hostingEnvironment, IUmbracoVersion umbracoVersion, IRuntimeMinifier runtimeMinifier)
        {
            //make a hash of umbraco and client dependency version
            //in case the user bypasses the installer and just bumps the web.config or client dependency config

            //if in debug mode, always burst the cache
            if (hostingEnvironment.IsDebugMode)
            {
                return DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture).GenerateHash();
            }

            var version = umbracoVersion.SemanticVersion.ToSemanticString();
            return $"{version}.{runtimeMinifier.CacheBuster}".GenerateHash();
        }
    }
}
