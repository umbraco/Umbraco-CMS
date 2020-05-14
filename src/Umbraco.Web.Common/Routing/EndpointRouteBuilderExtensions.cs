using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NUglify.Helpers;
using System;
using System.Text;
using Umbraco.Extensions;

namespace Umbraco.Web.Common.Routing
{
    public static class EndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Used to map Umbraco controllers consistently
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="controllerType"></param>
        /// <param name="rootSegment"></param>
        /// <param name="areaName"></param>
        /// <param name="prefixPathSegment"></param>
        /// <param name="defaultAction"></param>
        /// <param name="includeControllerNameInRoute"></param>
        /// <param name="constraints"></param>
        public static void MapUmbracoRoute(
            this IEndpointRouteBuilder endpoints,
            Type controllerType,
            string rootSegment,
            string areaName,
            string prefixPathSegment,
            string defaultAction = "Index",
            bool includeControllerNameInRoute = true,
            object constraints = null)
        {
            var controllerName = ControllerExtensions.GetControllerName(controllerType);

            // build the route pattern
            var pattern = new StringBuilder(rootSegment);
            if (!prefixPathSegment.IsNullOrWhiteSpace())
                pattern.Append("/").Append(prefixPathSegment);
            if (includeControllerNameInRoute)
                pattern.Append("/").Append(controllerName);
            pattern.Append("/").Append("{action}/{id?}");

            var defaults = defaultAction.IsNullOrWhiteSpace()
                ? (object) new { controller = controllerName }
                : new { controller = controllerName, action = defaultAction };


            if (areaName.IsNullOrWhiteSpace())
            {
                endpoints.MapControllerRoute(
                    // named consistently
                    $"umbraco-{areaName}-{controllerName}".ToLowerInvariant(),
                    pattern.ToString().ToLowerInvariant(),
                    defaults,
                    constraints);
            }
            else
            {
                endpoints.MapAreaControllerRoute(
                    // named consistently
                    $"umbraco-{areaName}-{controllerName}".ToLowerInvariant(),
                    areaName,
                    pattern.ToString().ToLowerInvariant(),
                    defaults,
                    constraints);
            }
            
        }

        /// <summary>
        /// Used to map Umbraco controllers consistently
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoints"></param>
        /// <param name="rootSegment"></param>
        /// <param name="areaName"></param>
        /// <param name="prefixPathSegment"></param>
        /// <param name="defaultAction"></param>
        /// <param name="constraints"></param>
        public static void MapUmbracoRoute<T>(
            this IEndpointRouteBuilder endpoints,
            string rootSegment,
            string areaName,
            string prefixPathSegment,
            string defaultAction = "Index",
            bool includeControllerNameInRoute = true,
            object constraints = null)
            where T : ControllerBase
            => endpoints.MapUmbracoRoute(typeof(T), rootSegment, areaName, prefixPathSegment, defaultAction, includeControllerNameInRoute, constraints);

        /// <summary>
        /// Used to map Umbraco api controllers consistently
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoints"></param>
        /// <param name="rootSegment"></param>
        /// <param name="areaName"></param>
        /// <param name="isBackOffice">If the route is a back office route</param>
        /// <param name="constraints"></param>
        public static void MapUmbracoApiRoute<T>(
            this IEndpointRouteBuilder endpoints,
            string rootSegment,
            string areaName,
            bool isBackOffice,
            string defaultAction = "Index",
            object constraints = null)
            where T : ControllerBase
            => endpoints.MapUmbracoApiRoute(typeof(T), rootSegment, areaName, isBackOffice, defaultAction, constraints);

        /// <summary>
        /// Used to map Umbraco api controllers consistently
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="controllerType"></param>
        /// <param name="rootSegment"></param>
        /// <param name="areaName"></param>
        /// <param name="isBackOffice">If the route is a back office route</param>
        /// <param name="defaultAction"></param>
        /// <param name="constraints"></param>
        public static void MapUmbracoApiRoute(
            this IEndpointRouteBuilder endpoints,
            Type controllerType,
            string rootSegment,
            string areaName,
            bool isBackOffice,
            string defaultAction = "Index",
            object constraints = null)
            => endpoints.MapUmbracoRoute(controllerType, rootSegment, areaName,
                isBackOffice
                    ? (areaName.IsNullOrWhiteSpace() ? "BackOffice/Api" : $"BackOffice/{areaName}")
                    : (areaName.IsNullOrWhiteSpace() ? "Api" : areaName),
                defaultAction, true, constraints);
    }
}
