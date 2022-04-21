using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Website.Controllers;

namespace Umbraco.Extensions
{
    public static class LinkGeneratorExtensions
    {
        /// <summary>
        /// Return the Url for a Surface Controller
        /// </summary>
        /// <typeparam name="T">The <see cref="SurfaceController"/></typeparam>
        public static string GetUmbracoSurfaceUrl<T>(this LinkGenerator linkGenerator,
            Expression<Func<T, object>> methodSelector, string pathBase)
            where T : SurfaceController
        {
            MethodInfo method = ExpressionHelper.GetMethodInfo(methodSelector);
            IDictionary<string, object> methodParams = ExpressionHelper.GetMethodParams(methodSelector);

            if (method == null)
            {
                throw new MissingMethodException(
                    $"Could not find the method {methodSelector} on type {typeof(T)} or the result ");
            }

            if (methodParams.Any() == false)
            {
                return linkGenerator.GetUmbracoSurfaceUrl<T>(method.Name, pathBase);
            }

            return linkGenerator.GetUmbracoSurfaceUrl<T>(method.Name, pathBase, methodParams);
        }

        /// <summary>
        /// Return the Url for a Surface Controller
        /// </summary>
        /// <typeparam name="T">The <see cref="SurfaceController"/></typeparam>
        public static string GetUmbracoSurfaceUrl<T>(this LinkGenerator linkGenerator, string actionName,
            string pathBase, object id = null)
            where T : SurfaceController => linkGenerator.GetUmbracoControllerUrl(
                actionName,
                typeof(T), pathBase,
                new Dictionary<string, object>()
                {
                    ["id"] = id
                });
    }
}
