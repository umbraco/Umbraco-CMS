using System;
using System.Globalization;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Web.Models;
using Umbraco.Core.Logging;

namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods for getting ImageProcessor URL from the core Image Cropper property editor
    /// </summary>
    public static class ImageCropperTemplateExtensions
    {
        /// <summary>
        /// Gets the ImageProcessor URL by the crop alias (from the "umbracoFile" property alias) on the IPublishedContent item
        /// </summary>
        /// <param name="mediaItem">
        /// The IPublishedContent item.
        /// </param>
        /// <param name="cropAlias">
        /// The crop alias e.g. thumbnail
        /// </param>
        /// <returns>
        /// The ImageProcessor.Web URL.
        /// </returns>
        public static string GetCropUrl(this IPublishedContent mediaItem, string cropAlias) => ImageCropperTemplateCoreExtensions.GetCropUrl(mediaItem, cropAlias, Current.ImageUrlGenerator);

        public static string GetCropUrl(this MediaWithCrops mediaWithCrops, string cropAlias) => ImageCropperTemplateCoreExtensions.GetCropUrl(mediaWithCrops, cropAlias, Current.ImageUrlGenerator);

        [Obsolete("Use the GetCropUrl overload with the updated parameter order and note this implementation has changed to get the URL from the media item.")]
        public static string GetCropUrl(this IPublishedContent mediaItem, string cropAlias, ImageCropperValue imageCropperValue) => mediaItem.GetCropUrl(imageCropperValue, cropAlias);

        /// <summary>
        /// Gets the crop URL by using only the specified <paramref name="imageCropperValue" />.
        /// </summary>
        /// <param name="mediaItem">The media item.</param>
        /// <param name="imageCropperValue">The image cropper value.</param>
        /// <param name="cropAlias">The crop alias.</param>
        /// <returns>
        /// The image crop URL.
        /// </returns>
        public static string GetCropUrl(this IPublishedContent mediaItem, ImageCropperValue imageCropperValue, string cropAlias) => ImageCropperTemplateCoreExtensions.GetCropUrl(mediaItem, imageCropperValue, cropAlias, Current.ImageUrlGenerator);

        /// <summary>
        /// Gets the ImageProcessor URL by the crop alias using the specified property containing the image cropper Json data on the IPublishedContent item.
        /// </summary>
        /// <param name="mediaItem">
        /// The IPublishedContent item.
        /// </param>
        /// <param name="propertyAlias">
        /// The property alias of the property containing the Json data e.g. umbracoFile
        /// </param>
        /// <param name="cropAlias">
        /// The crop alias e.g. thumbnail
        /// </param>
        /// <returns>
        /// The ImageProcessor.Web URL.
        /// </returns>
        public static string GetCropUrl(this IPublishedContent mediaItem, string propertyAlias, string cropAlias) => ImageCropperTemplateCoreExtensions.GetCropUrl(mediaItem, propertyAlias, cropAlias, Current.ImageUrlGenerator);

        public static string GetCropUrl(this MediaWithCrops mediaWithCrops, string propertyAlias, string cropAlias) => ImageCropperTemplateCoreExtensions.GetCropUrl(mediaWithCrops, propertyAlias, cropAlias, Current.ImageUrlGenerator);

        /// <summary>
        /// Gets the ImageProcessor URL from the IPublishedContent item.
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
        /// Use focal point, to generate an output image using the focal point instead of the predefined crop
        /// </param>
        /// <param name="useCropDimensions">
        /// Use crop dimensions to have the output image sized according to the predefined crop sizes, this will override the width and height parameters.
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
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetCropUrl(
             this IPublishedContent mediaItem,
             int? width = null,
             int? height = null,
             string propertyAlias = Constants.Conventions.Media.File,
             string cropAlias = null,
             int? quality = null,
             ImageCropMode? imageCropMode = null,
             ImageCropAnchor? imageCropAnchor = null,
             bool preferFocalPoint = false,
             bool useCropDimensions = false,
             bool cacheBuster = true,
             string furtherOptions = null,
             ImageCropRatioMode? ratioMode = null,
             bool upScale = true) => ImageCropperTemplateCoreExtensions.GetCropUrl(mediaItem, Current.ImageUrlGenerator, width, height, propertyAlias, cropAlias, quality, imageCropMode, imageCropAnchor, preferFocalPoint, useCropDimensions, cacheBuster, furtherOptions, ratioMode, upScale);

        public static string GetCropUrl(
             this MediaWithCrops mediaWithCrops,
             int? width = null,
             int? height = null,
             string propertyAlias = Constants.Conventions.Media.File,
             string cropAlias = null,
             int? quality = null,
             ImageCropMode? imageCropMode = null,
             ImageCropAnchor? imageCropAnchor = null,
             bool preferFocalPoint = false,
             bool useCropDimensions = false,
             bool cacheBuster = true,
             string furtherOptions = null,
             ImageCropRatioMode? ratioMode = null,
             bool upScale = true) => ImageCropperTemplateCoreExtensions.GetCropUrl(mediaWithCrops, Current.ImageUrlGenerator, width, height, propertyAlias, cropAlias, quality, imageCropMode, imageCropAnchor, preferFocalPoint, useCropDimensions, cacheBuster, furtherOptions, ratioMode, upScale);

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
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetCropUrl(
            this string imageUrl,
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
            bool upScale = true) => ImageCropperTemplateCoreExtensions.GetCropUrl(imageUrl, Current.ImageUrlGenerator, width, height, imageCropperValue, cropAlias, quality, imageCropMode, imageCropAnchor, preferFocalPoint, useCropDimensions, cacheBusterValue, furtherOptions, ratioMode, upScale);

        /// <summary>
        /// Gets the ImageProcessor URL from the image path.
        /// </summary>
        /// <param name="imageUrl">
        /// The image URL.
        /// </param>
        /// <param name="cropDataSet"></param>
        /// <param name="width">
        /// The width of the output image.
        /// </param>
        /// <param name="height">
        /// The height of the output image.
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
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetCropUrl(
            this string imageUrl,
            ImageCropperValue cropDataSet,
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
            bool upScale = true) => ImageCropperTemplateCoreExtensions.GetCropUrl(imageUrl, Current.ImageUrlGenerator, cropDataSet, width, height, cropAlias, quality, imageCropMode, imageCropAnchor, preferFocalPoint, useCropDimensions, cacheBusterValue, furtherOptions, ratioMode, upScale);

        [Obsolete("Use GetCrop to merge local and media crops, get automatic cache buster value and have more parameters.")]
        public static string GetLocalCropUrl(this MediaWithCrops mediaWithCrops,
            string alias,
            string cacheBusterValue = null)
            => ImageCropperTemplateCoreExtensions.GetLocalCropUrl(mediaWithCrops, alias, Current.ImageUrlGenerator, cacheBusterValue);

        private static readonly JsonSerializerSettings ImageCropperValueJsonSerializerSettings = new JsonSerializerSettings
        {
            Culture = CultureInfo.InvariantCulture,
            FloatParseHandling = FloatParseHandling.Decimal
        };

        internal static ImageCropperValue DeserializeImageCropperValue(this string json)
        {
            ImageCropperValue imageCrops = null;
            if (json.DetectIsJson())
            {
                try
                {
                    imageCrops = JsonConvert.DeserializeObject<ImageCropperValue>(json, ImageCropperValueJsonSerializerSettings);
                }
                catch (Exception ex)
                {
                    Current.Logger.Error<string>(typeof(ImageCropperTemplateExtensions), ex, "Could not parse the json string: {Json}", json);
                }
            }

            imageCrops = imageCrops ?? new ImageCropperValue();
            return imageCrops;
        }
    }
}
