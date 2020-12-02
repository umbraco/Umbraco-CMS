using System;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Web.Models;
using Umbraco.Core.Media;
using Umbraco.Web.Routing;

namespace Umbraco.Extensions
{
    public static class ImageCropperTemplateCoreExtensions
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
        /// <param name="imageUrlGenerator">The image url generator.</param>
        /// <param name="publishedValueFallback">The published value fallback.</param>
        /// <param name="publishedUrlProvider">The published url provider.</param>
        /// <returns>
        /// The ImageProcessor.Web URL.
        /// </returns>
        public static string GetCropUrl(
            this IPublishedContent mediaItem,
            string cropAlias,
            IImageUrlGenerator imageUrlGenerator,
            IPublishedValueFallback publishedValueFallback,
            IPublishedUrlProvider publishedUrlProvider)
        {
            return mediaItem.GetCropUrl(imageUrlGenerator, publishedValueFallback, publishedUrlProvider, cropAlias: cropAlias, useCropDimensions: true);
        }

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
        /// <param name="imageUrlGenerator">The image url generator.</param>
        /// <param name="publishedValueFallback">The published value fallback.</param>
        /// <param name="publishedUrlProvider">The published url provider.</param>
        /// <returns>
        /// The ImageProcessor.Web URL.
        /// </returns>
        public static string GetCropUrl(
            this IPublishedContent mediaItem,
            string propertyAlias,
            string cropAlias,
            IImageUrlGenerator imageUrlGenerator,
            IPublishedValueFallback publishedValueFallback,
            IPublishedUrlProvider publishedUrlProvider)
        {
            return mediaItem.GetCropUrl( imageUrlGenerator, publishedValueFallback, publishedUrlProvider, propertyAlias: propertyAlias, cropAlias: cropAlias, useCropDimensions: true);
        }

        /// <summary>
        /// Gets the ImageProcessor URL from the IPublishedContent item.
        /// </summary>
        /// <param name="mediaItem">
        /// The IPublishedContent item.
        /// </param>
        /// <param name="imageUrlGenerator">The image url generator.</param>
        /// <param name="publishedValueFallback">The published value fallback.</param>
        /// <param name="publishedUrlProvider">The published url provider.</param>
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
             IImageUrlGenerator imageUrlGenerator,
             IPublishedValueFallback publishedValueFallback,
             IPublishedUrlProvider publishedUrlProvider,
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
             bool upScale = true)
        {
            if (mediaItem == null) throw new ArgumentNullException("mediaItem");

            var cacheBusterValue = cacheBuster ? mediaItem.UpdateDate.ToFileTimeUtc().ToString(CultureInfo.InvariantCulture) : null;

            if (mediaItem.HasProperty(propertyAlias) == false || mediaItem.HasValue(propertyAlias) == false)
                return string.Empty;

            var mediaItemUrl = mediaItem.MediaUrl(publishedUrlProvider, propertyAlias: propertyAlias);

            //get the default obj from the value converter
            var cropperValue = mediaItem.Value(publishedValueFallback, propertyAlias);

            //is it strongly typed?
            var stronglyTyped = cropperValue as ImageCropperValue;
            if (stronglyTyped != null)
            {
                return GetCropUrl(
                    mediaItemUrl, imageUrlGenerator, stronglyTyped, width, height, cropAlias, quality, imageCropMode, imageCropAnchor, preferFocalPoint, useCropDimensions,
                    cacheBusterValue, furtherOptions, ratioMode, upScale);
            }

            //this shouldn't be the case but we'll check
            var jobj = cropperValue as JObject;
            if (jobj != null)
            {
                stronglyTyped = jobj.ToObject<ImageCropperValue>();
                return GetCropUrl(
                    mediaItemUrl, imageUrlGenerator, stronglyTyped, width, height, cropAlias, quality, imageCropMode, imageCropAnchor, preferFocalPoint, useCropDimensions,
                    cacheBusterValue, furtherOptions, ratioMode, upScale);
            }

            //it's a single string
            return GetCropUrl(
                mediaItemUrl, imageUrlGenerator, width, height, mediaItemUrl, cropAlias, quality, imageCropMode, imageCropAnchor, preferFocalPoint, useCropDimensions,
                cacheBusterValue, furtherOptions, ratioMode, upScale);
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
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetCropUrl(
            this string imageUrl,
            IImageUrlGenerator imageUrlGenerator,
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
            bool upScale = true)
        {
            if (string.IsNullOrEmpty(imageUrl)) return string.Empty;

            ImageCropperValue cropDataSet = null;
            if (string.IsNullOrEmpty(imageCropperValue) == false && imageCropperValue.DetectIsJson() && (imageCropMode == ImageCropMode.Crop || imageCropMode == null))
            {
                cropDataSet = imageCropperValue.DeserializeImageCropperValue();
            }
            return GetCropUrl(
                imageUrl, imageUrlGenerator, cropDataSet, width, height, cropAlias, quality, imageCropMode,
                imageCropAnchor, preferFocalPoint, useCropDimensions, cacheBusterValue, furtherOptions, ratioMode, upScale);
        }

        /// <summary>
        /// Gets the ImageProcessor URL from the image path.
        /// </summary>
        /// <param name="imageUrl">
        /// The image URL.
        /// </param>
        /// <param name="imageUrlGenerator">
        /// The generator that will process all the options and the image URL to return a full image URLs with all processing options appended
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
            IImageUrlGenerator imageUrlGenerator,
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
            bool upScale = true,
            string animationProcessMode = null)
        {
            if (string.IsNullOrEmpty(imageUrl)) return string.Empty;

            ImageUrlGenerationOptions options;

            if (cropDataSet != null && (imageCropMode == ImageCropMode.Crop || imageCropMode == null))
            {
                var crop = cropDataSet.GetCrop(cropAlias);

                // if a crop was specified, but not found, return null
                if (crop == null && !string.IsNullOrWhiteSpace(cropAlias))
                    return null;

                options = cropDataSet.GetCropBaseOptions(imageUrl, crop, string.IsNullOrWhiteSpace(cropAlias), preferFocalPoint);

                if (crop != null & useCropDimensions)
                {
                    width = crop.Width;
                    height = crop.Height;
                }

                // If a predefined crop has been specified & there are no coordinates & no ratio mode, but a width parameter has been passed we can get the crop ratio for the height
                if (crop != null && string.IsNullOrEmpty(cropAlias) == false && crop.Coordinates == null && ratioMode == null && width != null && height == null)
                {
                    options.HeightRatio = (decimal)crop.Height / crop.Width;
                }

                // If a predefined crop has been specified & there are no coordinates & no ratio mode, but a height parameter has been passed we can get the crop ratio for the width
                if (crop != null && string.IsNullOrEmpty(cropAlias) == false && crop.Coordinates == null && ratioMode == null && width == null && height != null)
                {
                    options.WidthRatio = (decimal)crop.Width / crop.Height;
                }
            }
            else
            {
                options = new ImageUrlGenerationOptions (imageUrl)
                {
                    ImageCropMode = (imageCropMode ?? ImageCropMode.Pad),
                    ImageCropAnchor = imageCropAnchor
                };
            }

            options.Quality = quality;
            options.Width = ratioMode != null && ratioMode.Value == ImageCropRatioMode.Width ? null : width;
            options.Height = ratioMode != null && ratioMode.Value == ImageCropRatioMode.Height ? null : height;
            options.AnimationProcessMode = animationProcessMode;

            if (ratioMode == ImageCropRatioMode.Width && height != null)
            {
                // if only height specified then assume a square
                if (width == null) width = height;
                options.WidthRatio = (decimal)width / (decimal)height;
            }

            if (ratioMode == ImageCropRatioMode.Height && width != null)
            {
                // if only width specified then assume a square
                if (height == null) height = width;
                options.HeightRatio = (decimal)height / (decimal)width;
            }

            options.UpScale = upScale;
            options.FurtherOptions = furtherOptions;
            options.CacheBusterValue = cacheBusterValue;

            return imageUrlGenerator.GetImageUrl(options);
        }
    }
}
