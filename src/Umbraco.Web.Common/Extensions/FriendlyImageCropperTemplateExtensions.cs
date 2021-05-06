using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Extensions
{
    public static class FriendlyImageCropperTemplateExtensions
    {
        private static IImageUrlGenerator ImageUrlGenerator { get; } =
            StaticServiceProvider.Instance.GetRequiredService<IImageUrlGenerator>();

        private static IPublishedValueFallback PublishedValueFallback { get; } =
            StaticServiceProvider.Instance.GetRequiredService<IPublishedValueFallback>();

        private static IPublishedUrlProvider PublishedUrlProvider { get; } =
            StaticServiceProvider.Instance.GetRequiredService<IPublishedUrlProvider>();


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
        public static string GetCropUrl(
            this IPublishedContent mediaItem,
            string cropAlias) =>
            mediaItem.GetCropUrl(cropAlias, ImageUrlGenerator, PublishedValueFallback, PublishedUrlProvider);

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
        public static string GetCropUrl(
            this IPublishedContent mediaItem,
            string propertyAlias,
            string cropAlias) =>
            mediaItem.GetCropUrl(propertyAlias, cropAlias, ImageUrlGenerator, PublishedValueFallback, PublishedUrlProvider);

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
            string propertyAlias = Cms.Core.Constants.Conventions.Media.File,
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
            => mediaItem.GetCropUrl(
                ImageUrlGenerator,
                PublishedValueFallback,
                PublishedUrlProvider,
                width,
                height,
                propertyAlias,
                cropAlias,
                quality,
                imageCropMode,
                imageCropAnchor,
                preferFocalPoint,
                useCropDimensions,
                cacheBuster,
                furtherOptions,
                ratioMode,
                upScale
            );

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
            IImageUrlGenerator ImageUrlGenerator,
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
            => imageUrl.GetCropUrl(
                ImageUrlGenerator,
                width,
                height,
                imageCropperValue,
                cropAlias,
                quality,
                imageCropMode,
                imageCropAnchor,
                preferFocalPoint,
                useCropDimensions,
                cacheBusterValue,
                furtherOptions,
                ratioMode,
                upScale
                );

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
            bool upScale = true,
            string animationProcessMode = null)
            => imageUrl.GetCropUrl(
                ImageUrlGenerator,
                cropDataSet,
                width,
                height,
                cropAlias,
                quality, imageCropMode,
                imageCropAnchor,
                preferFocalPoint,
                useCropDimensions,
                cacheBusterValue,
                furtherOptions,
                ratioMode,
                upScale,
                animationProcessMode
            );
    }
}
