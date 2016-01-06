using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web
{
    /// <summary>
    /// Extension methods for UrlHelper for use in templates
    /// </summary>
    public static class UrlHelperRenderExtensions
    {

        #region GetCropUrl

        /// <summary>
        /// Gets the ImageProcessor Url of a media item by the crop alias (using default media item property alias of "umbracoFile")
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="mediaItem">
        /// The IPublishedContent item.
        /// </param>
        /// <param name="cropAlias">
        /// The crop alias e.g. thumbnail
        /// </param>
        /// <param name="htmlEncode">
        /// Whether to HTML encode this URL - default is true - w3c standards require html attributes to be html encoded but this can be 
        /// set to false if using the result of this method for CSS.
        /// </param>
        /// <returns></returns>
        public static IHtmlString GetCropUrl(this UrlHelper urlHelper, IPublishedContent mediaItem, string cropAlias, bool htmlEncode = true)
        {
            var url = mediaItem.GetCropUrl(cropAlias: cropAlias, useCropDimensions: true);
            return htmlEncode ? new HtmlString(HttpUtility.HtmlEncode(url)) : new HtmlString(url);
        }

        /// <summary>
        /// Gets the ImageProcessor Url by the crop alias using the specified property containing the image cropper Json data on the IPublishedContent item.
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="mediaItem">
        /// The IPublishedContent item.
        /// </param>
        /// <param name="propertyAlias">
        /// The property alias of the property containing the Json data e.g. umbracoFile
        /// </param>
        /// <param name="cropAlias">
        /// The crop alias e.g. thumbnail
        /// </param>
        /// <param name="htmlEncode">
        /// Whether to HTML encode this URL - default is true - w3c standards require html attributes to be html encoded but this can be 
        /// set to false if using the result of this method for CSS.
        /// </param>
        /// <returns>
        /// The ImageProcessor.Web Url.
        /// </returns>
        public static IHtmlString GetCropUrl(this UrlHelper urlHelper, IPublishedContent mediaItem, string propertyAlias, string cropAlias, bool htmlEncode = true)
        {
            var url = mediaItem.GetCropUrl(propertyAlias: propertyAlias, cropAlias: cropAlias, useCropDimensions: true);
            return htmlEncode ? new HtmlString(HttpUtility.HtmlEncode(url)) : new HtmlString(url);
        }

        /// <summary>
        /// Gets the ImageProcessor Url from the image path.
        /// </summary>
        /// <param name="mediaItem">
        /// The IPublishedContent item.
        /// </param>
        /// <param name="width">
        /// The width of the output image.
        /// </param>
        /// <param name="height">
        /// The height of the output image.
        /// </param>
        /// <param name="propertyAlias">
        /// Property alias of the property containing the Json data.
        /// </param>
        /// <param name="cropAlias">
        /// The crop alias.
        /// </param>
        /// <param name="quality">
        /// Quality percentage of the output image.
        /// </param>
        /// <param name="imageCropMode">
        /// The image crop mode.
        /// </param>
        /// <param name="imageCropAnchor">
        /// The image crop anchor.
        /// </param>
        /// <param name="preferFocalPoint">
        /// Use focal point to generate an output image using the focal point instead of the predefined crop if there is one
        /// </param>
        /// <param name="useCropDimensions">
        /// Use crop dimensions to have the output image sized according to the predefined crop sizes, this will override the width and height parameters
        /// </param>
        /// <param name="cacheBuster">
        /// Add a serialised date of the last edit of the item to ensure client cache refresh when updated
        /// </param>
        /// <param name="furtherOptions">
        /// The further options.
        /// </param>
        /// <param name="ratioMode">
        /// Use a dimension as a ratio
        /// </param>
        /// <param name="upScale">
        /// If the image should be upscaled to requested dimensions
        /// </param>
        /// <param name="urlHelper"></param>
        /// <param name="htmlEncode">
        /// Whether to HTML encode this URL - default is true - w3c standards require html attributes to be html encoded but this can be 
        /// set to false if using the result of this method for CSS.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static IHtmlString GetCropUrl(this UrlHelper urlHelper,
            IPublishedContent mediaItem,
            int? width = null,
            int? height = null,
            string propertyAlias = Umbraco.Core.Constants.Conventions.Media.File,
            string cropAlias = null,
            int? quality = null,
            ImageCropMode? imageCropMode = null,
            ImageCropAnchor? imageCropAnchor = null,
            bool preferFocalPoint = false,
            bool useCropDimensions = false,
            bool cacheBuster = true,
            string furtherOptions = null,
            ImageCropRatioMode? ratioMode = null,
            bool upScale = true,
            bool htmlEncode = true)
        {
            var url = mediaItem.GetCropUrl(width, height, propertyAlias, cropAlias, quality, imageCropMode,
                imageCropAnchor, preferFocalPoint, useCropDimensions, cacheBuster, furtherOptions, ratioMode,
                upScale);
            return htmlEncode ? new HtmlString(HttpUtility.HtmlEncode(url)) : new HtmlString(url);
        }

        /// <summary>
        /// Gets the ImageProcessor Url from the image path.
        /// </summary>
        /// <param name="imageUrl">
        /// The image url.
        /// </param>
        /// <param name="width">
        /// The width of the output image.
        /// </param>
        /// <param name="height">
        /// The height of the output image.
        /// </param>
        /// <param name="imageCropperValue">
        /// The Json data from the Umbraco Core Image Cropper property editor
        /// </param>
        /// <param name="cropAlias">
        /// The crop alias.
        /// </param>
        /// <param name="quality">
        /// Quality percentage of the output image.
        /// </param>
        /// <param name="imageCropMode">
        /// The image crop mode.
        /// </param>
        /// <param name="imageCropAnchor">
        /// The image crop anchor.
        /// </param>
        /// <param name="preferFocalPoint">
        /// Use focal point to generate an output image using the focal point instead of the predefined crop if there is one
        /// </param>
        /// <param name="useCropDimensions">
        /// Use crop dimensions to have the output image sized according to the predefined crop sizes, this will override the width and height parameters
        /// </param>
        /// <param name="cacheBusterValue">
        /// Add a serialised date of the last edit of the item to ensure client cache refresh when updated
        /// </param>
        /// <param name="furtherOptions">
        /// The further options.
        /// </param>
        /// <param name="ratioMode">
        /// Use a dimension as a ratio
        /// </param>
        /// <param name="upScale">
        /// If the image should be upscaled to requested dimensions
        /// </param>
        /// <param name="urlHelper"></param>
        /// <param name="htmlEncode">
        /// Whether to HTML encode this URL - default is true - w3c standards require html attributes to be html encoded but this can be 
        /// set to false if using the result of this method for CSS.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static IHtmlString GetCropUrl(this UrlHelper urlHelper,
            string imageUrl,
            int? width = null,
            int? height = null,
            string imageCropperValue = null,
            string cropAlias = null,
            int? quality = null,
            ImageCropMode? imageCropMode = null,
            ImageCropAnchor? imageCropAnchor = null,
            bool preferFocalPoint = false,
            bool useCropDimensions = false,
            string cacheBusterValue = null,
            string furtherOptions = null,
            ImageCropRatioMode? ratioMode = null,
            bool upScale = true,
            bool htmlEncode = true)
        {
            var url = imageUrl.GetCropUrl(width, height, imageCropperValue, cropAlias, quality, imageCropMode,
                imageCropAnchor, preferFocalPoint, useCropDimensions, cacheBusterValue, furtherOptions, ratioMode,
                upScale);
            return htmlEncode ? new HtmlString(HttpUtility.HtmlEncode(url)) : new HtmlString(url);            
        }

        #endregion

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