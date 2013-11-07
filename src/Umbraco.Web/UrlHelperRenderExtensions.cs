using System;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Mvc;

namespace Umbraco.Web
{
    /// <summary>
    /// Extension methods for UrlHelper for use in templates
    /// </summary>
    public static class UrlHelperRenderExtensions
    {

        /// <summary>
        /// Generates a URL based on the current Umbraco URL with a custom query string that will route to the specified SurfaceController
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public static string SurfaceAction(this UrlHelper url, string action, string controllerName)
        {
            return url.SurfaceAction(action, controllerName, null);
        }

        /// <summary>
        /// Generates a URL based on the current Umbraco URL with a custom query string that will route to the specified SurfaceController
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="additionalRouteVals"></param>
        /// <returns></returns>
        public static string SurfaceAction(this UrlHelper url, string action, string controllerName, object additionalRouteVals)
        {
            return url.SurfaceAction(action, controllerName, "", additionalRouteVals);
        }

        /// <summary>
        /// Generates a URL based on the current Umbraco URL with a custom query string that will route to the specified SurfaceController
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="area"></param>
        /// <param name="additionalRouteVals"></param>
        /// <returns></returns>
        public static string SurfaceAction(this UrlHelper url, string action, string controllerName, string area, object additionalRouteVals)
        {
            Mandate.ParameterNotNullOrEmpty(action, "action");
            Mandate.ParameterNotNullOrEmpty(controllerName, "controllerName");

            var encryptedRoute = UmbracoHelper.CreateEncryptedRouteString(controllerName, action, area, additionalRouteVals);

            var result = UmbracoContext.Current.OriginalRequestUrl.AbsolutePath.EnsureEndsWith('?') + "ufprt=" + encryptedRoute;
            return result;
        }

        /// <summary>
        /// Generates a URL based on the current Umbraco URL with a custom query string that will route to the specified SurfaceController
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action"></param>
        /// <param name="surfaceType"></param>
        /// <returns></returns>
        public static string SurfaceAction(this UrlHelper url, string action, Type surfaceType)
        {
            return url.SurfaceAction(action, surfaceType, null);
        }

        /// <summary>
        /// Generates a URL based on the current Umbraco URL with a custom query string that will route to the specified SurfaceController
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action"></param>
        /// <param name="surfaceType"></param>
        /// <param name="additionalRouteVals"></param>
        /// <returns></returns>
        public static string SurfaceAction(this UrlHelper url, string action, Type surfaceType, object additionalRouteVals)
        {
            Mandate.ParameterNotNullOrEmpty(action, "action");
            Mandate.ParameterNotNull(surfaceType, "surfaceType");

            var area = "";

            var surfaceController = SurfaceControllerResolver.Current.RegisteredSurfaceControllers
                                                             .SingleOrDefault(x => x == surfaceType);
            if (surfaceController == null)
                throw new InvalidOperationException("Could not find the surface controller of type " + surfaceType.FullName);
            var metaData = PluginController.GetMetadata(surfaceController);
            if (metaData.AreaName.IsNullOrWhiteSpace() == false)
            {
                //set the area to the plugin area
                area = metaData.AreaName;
            }

            var encryptedRoute = UmbracoHelper.CreateEncryptedRouteString(metaData.ControllerName, action, area, additionalRouteVals);

            var result = UmbracoContext.Current.OriginalRequestUrl.AbsolutePath.EnsureEndsWith('?') + "ufprt=" + encryptedRoute;
            return result;
        }

        /// <summary>
        /// Generates a URL based on the current Umbraco URL with a custom query string that will route to the specified SurfaceController
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static string SurfaceAction<T>(this UrlHelper url, string action)
            where T : SurfaceController
        {
            return url.SurfaceAction(action, typeof (T));
        }

        /// <summary>
        /// Generates a URL based on the current Umbraco URL with a custom query string that will route to the specified SurfaceController
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="action"></param>
        /// <param name="additionalRouteVals"></param>
        /// <returns></returns>
        public static string SurfaceAction<T>(this UrlHelper url, string action, object additionalRouteVals)
            where T : SurfaceController
        {
            return url.SurfaceAction(action, typeof (T), additionalRouteVals);
        }


    }
}