using System;
using System.Linq;
using Umbraco.Core;
using Microsoft.AspNetCore.Routing;
using System.Reflection;
using Umbraco.Web.Common.Install;
using Umbraco.Core.Hosting;
using System.Linq.Expressions;
using Umbraco.Web.Common.Controllers;

namespace Umbraco.Extensions
{
    public static class LinkGeneratorExtensions
    {
        /// <summary>
        /// Return the back office url if the back office is installed
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetBackOfficeUrl(this LinkGenerator linkGenerator, IHostingEnvironment hostingEnvironment)
        {

            Type backOfficeControllerType;
            try
            {
                backOfficeControllerType = Assembly.Load("Umbraco.Web.BackOffice")?.GetType("Umbraco.Web.BackOffice.Controllers.BackOfficeController");
                if (backOfficeControllerType == null) return "/"; // this would indicate that the installer is installed without the back office
            }
            catch
            {
                return hostingEnvironment.ApplicationVirtualPath; // this would indicate that the installer is installed without the back office
            }

            return linkGenerator.GetPathByAction("Default", ControllerExtensions.GetControllerName(backOfficeControllerType), values: new { area = Constants.Web.Mvc.BackOfficeApiArea });
        }

        /// <summary>
        /// Returns the URL for the installer
        /// </summary>
        /// <param name="linkGenerator"></param>
        /// <returns></returns>
        public static string GetInstallerUrl(this LinkGenerator linkGenerator)
        {
            return linkGenerator.GetPathByAction(nameof(InstallController.Index), ControllerExtensions.GetControllerName<InstallController>(), new { area = Constants.Web.Mvc.InstallArea });
        }

        /// <summary>
        /// Return the Url for a Web Api service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService<T>(this LinkGenerator linkGenerator, string actionName, object id = null)
            where T : UmbracoApiControllerBase
        {
            return linkGenerator.GetUmbracoApiService(actionName, typeof(T), id);
        }

        public static string GetUmbracoApiServiceBaseUrl<T>(this LinkGenerator linkGenerator, Expression<Func<T, object>> methodSelector)
            where T : UmbracoApiControllerBase
        {
            var method = ExpressionHelper.GetMethodInfo(methodSelector);
            if (method == null)
            {
                throw new MissingMethodException("Could not find the method " + methodSelector + " on type " + typeof(T) + " or the result ");
            }
            return linkGenerator.GetUmbracoApiService<T>(method.Name).TrimEnd(method.Name);
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
        public static string GetUmbracoApiService(this LinkGenerator linkGenerator, string actionName, string controllerName, string area, object id = null)
        {
            if (actionName == null) throw new ArgumentNullException(nameof(actionName));
            if (string.IsNullOrWhiteSpace(actionName)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(actionName));
            if (controllerName == null) throw new ArgumentNullException(nameof(controllerName));
            if (string.IsNullOrWhiteSpace(controllerName)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(controllerName));

            if (area.IsNullOrWhiteSpace())
            {
                if (id == null)
                {
                    return linkGenerator.GetPathByAction(actionName, controllerName);
                }
                else
                {
                    return linkGenerator.GetPathByAction(actionName, controllerName, new { id = id });
                }
            }
            else
            {
                if (id == null)
                {
                    return linkGenerator.GetPathByAction(actionName, controllerName, new { area = area });
                }
                else
                {
                    return linkGenerator.GetPathByAction(actionName, controllerName, new { area = area, id = id });
                }
            }
        }

        /// <summary>
        /// Return the Url for a Web Api service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="apiControllerType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetUmbracoApiService(this LinkGenerator linkGenerator, string actionName, Type apiControllerType, object id = null)
        {
            if (actionName == null) throw new ArgumentNullException(nameof(actionName));
            if (string.IsNullOrWhiteSpace(actionName)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(actionName));
            if (apiControllerType == null) throw new ArgumentNullException(nameof(apiControllerType));

            var area = "";

            if (!typeof(UmbracoApiControllerBase).IsAssignableFrom(apiControllerType))
                throw new InvalidOperationException($"The controller {apiControllerType} is of type {typeof(UmbracoApiControllerBase)}");

            var metaData = PluginController.GetMetadata(apiControllerType);
            if (metaData.AreaName.IsNullOrWhiteSpace() == false)
            {
                //set the area to the plugin area
                area = metaData.AreaName;
            }
            return linkGenerator.GetUmbracoApiService(actionName, ControllerExtensions.GetControllerName(apiControllerType), area, id);
        }

        public static string GetUmbracoApiService<T>(this LinkGenerator linkGenerator, Expression<Func<T, object>> methodSelector)
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
                return linkGenerator.GetUmbracoApiService<T>(method.Name);
            }
            return linkGenerator.GetUmbracoApiService<T>(method.Name, methodParams.Values.First());
        }
    }
}
