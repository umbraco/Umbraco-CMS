using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web
{
    /// <summary>
    /// Extension methods for UrlHelper for use in templates
    /// </summary>
    public static class UrlHelperRenderExtensions
    {
        private static readonly IHtmlString EmptyHtmlString = new HtmlString(string.Empty);

        private static IHtmlString CreateHtmlString(string value, bool htmlEncode) => htmlEncode ? new HtmlString(HttpUtility.HtmlEncode(value)) : new HtmlString(value);

        #region GetCropUrl

        /// <summary>
        /// Gets the ImageProcessor URL of a media item by the crop alias (using default media item property alias of "umbracoFile")
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="mediaItem">
        /// The IPublishedContent item.
        /// </param>
        /// <param name="cropAlias">
        /// The crop alias e.g. thumbnail
        /// </param>
        /// <param name="htmlEncode">
        /// Whether to HTML encode this URL - default is true - w3c standards require HTML attributes to be HTML encoded but this can be
        /// set to false if using the result of this method for CSS.
        /// </param>
        /// <returns></returns>
        public static IHtmlString GetCropUrl(this UrlHelper urlHelper, IPublishedContent mediaItem, string cropAlias, bool htmlEncode = true)
        {
            if (mediaItem == null) return EmptyHtmlString;

            var url = mediaItem.GetCropUrl(cropAlias: cropAlias, useCropDimensions: true);

            return CreateHtmlString(url, htmlEncode);
        }

        public static IHtmlString GetCropUrl(this UrlHelper urlHelper, MediaWithCrops mediaWithCrops, string cropAlias, bool htmlEncode = true)
        {
            if (mediaWithCrops == null) return EmptyHtmlString;

            var url = mediaWithCrops.GetCropUrl(cropAlias: cropAlias, useCropDimensions: true);

            return CreateHtmlString(url, htmlEncode);
        }

        /// <summary>
        /// Gets the ImageProcessor URL by the crop alias using the specified property containing the image cropper Json data on the IPublishedContent item.
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
        /// Whether to HTML encode this URL - default is true - w3c standards require HTML attributes to be HTML encoded but this can be
        /// set to false if using the result of this method for CSS.
        /// </param>
        /// <returns>
        /// The ImageProcessor.Web URL.
        /// </returns>
        public static IHtmlString GetCropUrl(this UrlHelper urlHelper, IPublishedContent mediaItem, string propertyAlias, string cropAlias, bool htmlEncode = true)
        {
            if (mediaItem == null) return EmptyHtmlString;

            var url = mediaItem.GetCropUrl(propertyAlias: propertyAlias, cropAlias: cropAlias, useCropDimensions: true);

            return CreateHtmlString(url, htmlEncode);
        }

        public static IHtmlString GetCropUrl(this UrlHelper urlHelper, MediaWithCrops mediaWithCrops, string propertyAlias, string cropAlias, bool htmlEncode = true)
        {
            if (mediaWithCrops == null) return EmptyHtmlString;

            var url = mediaWithCrops.GetCropUrl(propertyAlias: propertyAlias, cropAlias: cropAlias, useCropDimensions: true);

            return CreateHtmlString(url, htmlEncode);
        }

        /// <summary>
        /// Gets the ImageProcessor URL from the image path.
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
        /// Add a serialized date of the last edit of the item to ensure client cache refresh when updated
        /// </param>
        /// <param name="furtherOptions">
        /// These are any query string parameters (formatted as query strings) that ImageProcessor supports. For example:
        /// <example>
        /// <![CDATA[
        /// furtherOptions: "&bgcolor=fff"
        /// ]]>
        /// </example>
        /// </param>
        /// <param name="ratioMode">
        /// Use a dimension as a ratio
        /// </param>
        /// <param name="upScale">
        /// If the image should be upscaled to requested dimensions
        /// </param>
        /// <param name="urlHelper"></param>
        /// <param name="htmlEncode">
        /// Whether to HTML encode this URL - default is true - w3c standards require HTML attributes to be HTML encoded but this can be
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
            if (mediaItem == null) return EmptyHtmlString;

            var url = mediaItem.GetCropUrl(width, height, propertyAlias, cropAlias, quality, imageCropMode, imageCropAnchor, preferFocalPoint, useCropDimensions, cacheBuster, furtherOptions, ratioMode, upScale);

            return CreateHtmlString(url, htmlEncode);
        }

        public static IHtmlString GetCropUrl(this UrlHelper urlHelper,
            MediaWithCrops mediaWithCrops,
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
            if (mediaWithCrops == null) return EmptyHtmlString;

            var url = mediaWithCrops.GetCropUrl(width, height, propertyAlias, cropAlias, quality, imageCropMode, imageCropAnchor, preferFocalPoint, useCropDimensions, cacheBuster, furtherOptions, ratioMode, upScale);

            return CreateHtmlString(url, htmlEncode);
        }

        /// <summary>
        /// Gets the ImageProcessor URL from the image path.
        /// </summary>
        /// <param name="imageUrl">
        /// The image URL.
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
        /// Add a serialized date of the last edit of the item to ensure client cache refresh when updated
        /// </param>
        /// <param name="furtherOptions">
        /// These are any query string parameters (formatted as query strings) that ImageProcessor supports. For example:
        /// <example>
        /// <![CDATA[
        /// furtherOptions: "&bgcolor=fff"
        /// ]]>
        /// </example>
        /// </param>
        /// <param name="ratioMode">
        /// Use a dimension as a ratio
        /// </param>
        /// <param name="upScale">
        /// If the image should be upscaled to requested dimensions
        /// </param>
        /// <param name="urlHelper"></param>
        /// <param name="htmlEncode">
        /// Whether to HTML encode this URL - default is true - w3c standards require HTML attributes to be HTML encoded but this can be
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
            var url = imageUrl.GetCropUrl(width, height, imageCropperValue, cropAlias, quality, imageCropMode, imageCropAnchor, preferFocalPoint, useCropDimensions, cacheBusterValue, furtherOptions, ratioMode, upScale);

            return CreateHtmlString(url, htmlEncode);
        }

        // TODO: enable again in v9 and make sure to document that `@Url.GetCropUrl(Model.Property, cropAlias: "Featured")` needs to be updated - see https://github.com/umbraco/Umbraco-CMS/pull/10527 for alternatives
        // public static IHtmlString GetCropUrl(this UrlHelper urlHelper, ImageCropperValue imageCropperValue, string cropAlias, bool htmlEncode = true)
        // {
        //     if (imageCropperValue == null || string.IsNullOrEmpty(imageCropperValue.Src)) return EmptyHtmlString;
        //
        //     var url = imageCropperValue.Src.GetCropUrl(imageCropperValue, cropAlias: cropAlias, useCropDimensions: true);
        //
        //     return CreateHtmlString(url, htmlEncode);
        // }

        public static IHtmlString GetCropUrl(this UrlHelper urlHelper,
            ImageCropperValue imageCropperValue,
            int? width = null,
            int? height = null,
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
            if (imageCropperValue == null || string.IsNullOrEmpty(imageCropperValue.Src)) return EmptyHtmlString;

            var url = imageCropperValue.Src.GetCropUrl(imageCropperValue, width, height, cropAlias, quality, imageCropMode, imageCropAnchor, preferFocalPoint, useCropDimensions, cacheBusterValue, furtherOptions, ratioMode, upScale);

            return CreateHtmlString(url, htmlEncode);
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
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (string.IsNullOrEmpty(action)) throw new ArgumentException("Value can't be empty.", nameof(action));
            if (controllerName == null) throw new ArgumentNullException(nameof(controllerName));
            if (string.IsNullOrEmpty(controllerName)) throw new ArgumentException("Value can't be empty.", nameof(controllerName));

            var encryptedRoute = CreateEncryptedRouteString(controllerName, action, area, additionalRouteVals);

            var result = Current.UmbracoContext.OriginalRequestUrl.AbsolutePath.EnsureEndsWith('?') + "ufprt=" + encryptedRoute;
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
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (string.IsNullOrEmpty(action)) throw new ArgumentException("Value can't be empty.", nameof(action));
            if (surfaceType == null) throw new ArgumentNullException(nameof(surfaceType));

            var area = "";

            var surfaceController = Current.SurfaceControllerTypes.SingleOrDefault(x => x == surfaceType);
            if (surfaceController == null)
                throw new InvalidOperationException("Could not find the surface controller of type " + surfaceType.FullName);
            var metaData = PluginController.GetMetadata(surfaceController);
            if (metaData.AreaName.IsNullOrWhiteSpace() == false)
            {
                //set the area to the plugin area
                area = metaData.AreaName;
            }

            var encryptedRoute = CreateEncryptedRouteString(metaData.ControllerName, action, area, additionalRouteVals);

            var result = Current.UmbracoContext.OriginalRequestUrl.AbsolutePath.EnsureEndsWith('?') + "ufprt=" + encryptedRoute;
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

        /// <summary>
        /// This is used in methods like BeginUmbracoForm and SurfaceAction to generate an encrypted string which gets submitted in a request for which
        /// Umbraco can decrypt during the routing process in order to delegate the request to a specific MVC Controller.
        /// </summary>
        /// <param name="controllerName"></param>
        /// <param name="controllerAction"></param>
        /// <param name="area"></param>
        /// <param name="additionalRouteVals"></param>
        /// <returns></returns>
        internal static string CreateEncryptedRouteString(string controllerName, string controllerAction, string area, object additionalRouteVals = null)
        {
            if (controllerName == null) throw new ArgumentNullException(nameof(controllerName));
            if (string.IsNullOrEmpty(controllerName)) throw new ArgumentException("Value can't be empty.", nameof(controllerName));
            if (controllerAction == null) throw new ArgumentNullException(nameof(controllerAction));
            if (string.IsNullOrEmpty(controllerAction)) throw new ArgumentException("Value can't be empty.", nameof(controllerAction));
            if (area == null) throw new ArgumentNullException(nameof(area));

            //need to create a params string as Base64 to put into our hidden field to use during the routes
            var surfaceRouteParams = $"c={HttpUtility.UrlEncode(controllerName)}&a={HttpUtility.UrlEncode(controllerAction)}&ar={area}";

            //checking if the additional route values is already a dictionary and convert to querystring
            string additionalRouteValsAsQuery;
            if (additionalRouteVals != null)
            {
                if (additionalRouteVals is Dictionary<string, object> additionalRouteValsAsDictionary)
                    additionalRouteValsAsQuery = additionalRouteValsAsDictionary.ToQueryString();
                else
                    additionalRouteValsAsQuery = additionalRouteVals.ToDictionary<object>().ToQueryString();
            }
            else
                additionalRouteValsAsQuery = null;

            if (additionalRouteValsAsQuery.IsNullOrWhiteSpace() == false)
                surfaceRouteParams += "&" + additionalRouteValsAsQuery;

            return surfaceRouteParams.EncryptWithMachineKey();
        }
    }
}
