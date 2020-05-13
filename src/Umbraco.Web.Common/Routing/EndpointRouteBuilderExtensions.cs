using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NUglify.Helpers;
using System.Text;
using Umbraco.Extensions;

namespace Umbraco.Web.Common.Routing
{
    public static class EndpointRouteBuilderExtensions
    {
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
        {
            var controllerName = ControllerExtensions.GetControllerName<T>();

            // build the route pattern
            var pattern = new StringBuilder(rootSegment);
            if (!prefixPathSegment.IsNullOrWhiteSpace())
                pattern.Append("/").Append(prefixPathSegment);
            if (includeControllerNameInRoute)
                pattern.Append("/").Append(controllerName);
            pattern.Append("/").Append("{action}/{id?}");

            endpoints.MapAreaControllerRoute(
                // named consistently
                $"umbraco-{areaName}-{controllerName}".ToLowerInvariant(),
                areaName,
                pattern.ToString().ToLowerInvariant(),
                new { controller = controllerName, action = defaultAction },
                constraints);
        }

        /// <summary>
        /// Used to map Umbraco controllers consistently
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoints"></param>
        /// <param name="rootPathSegment"></param>
        /// <param name="areaName"></param>
        /// <param name="isBackOffice"></param>
        /// <param name="constraints"></param>
        public static void MapUmbracoRoute<T>(
            this IEndpointRouteBuilder endpoints,
            string rootPathSegment,
            string areaName,
            bool isBackOffice,
            string defaultAction = "Index",
            object constraints = null)
            where T : ControllerBase
            => endpoints.MapUmbracoRoute<T>(rootPathSegment, areaName, isBackOffice ? "BackOffice" : "Api", defaultAction, true, constraints);
    }
}
