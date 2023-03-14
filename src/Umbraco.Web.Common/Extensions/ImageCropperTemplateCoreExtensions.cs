using System.Globalization;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Extensions;

public static class ImageCropperTemplateCoreExtensions
{
    /// <summary>
    ///     Gets the underlying image processing service URL by the crop alias (from the "umbracoFile" property alias) on the
    ///     IPublishedContent item.
    /// </summary>
    /// <param name="mediaItem">The IPublishedContent item.</param>
    /// <param name="cropAlias">The crop alias e.g. thumbnail.</param>
    /// <param name="imageUrlGenerator">The image URL generator.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    /// <param name="publishedUrlProvider">The published URL provider.</param>
    /// <param name="urlMode">The url mode.</param>
    /// <returns>
    ///     The URL of the cropped image.
    /// </returns>
    public static string? GetCropUrl(
        this IPublishedContent mediaItem,
        string cropAlias,
        IImageUrlGenerator imageUrlGenerator,
        IPublishedValueFallback publishedValueFallback,
        IPublishedUrlProvider publishedUrlProvider,
        UrlMode urlMode = UrlMode.Default) =>
        mediaItem.GetCropUrl(
            imageUrlGenerator,
            publishedValueFallback,
            publishedUrlProvider,
            cropAlias: cropAlias,
            useCropDimensions: true,
            urlMode: urlMode);

    /// <summary>
    ///     Gets the underlying image processing service URL by the crop alias (from the "umbracoFile" property alias in the
    ///     MediaWithCrops content item) on the MediaWithCrops item.
    /// </summary>
    /// <param name="mediaWithCrops">The MediaWithCrops item.</param>
    /// <param name="cropAlias">The crop alias e.g. thumbnail.</param>
    /// <param name="imageUrlGenerator">The image URL generator.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    /// <param name="publishedUrlProvider">The published URL provider.</param>
    /// <param name="urlMode">The url mode.</param>
    /// <returns>
    ///     The URL of the cropped image.
    /// </returns>
    public static string? GetCropUrl(
        this MediaWithCrops mediaWithCrops,
        string cropAlias,
        IImageUrlGenerator imageUrlGenerator,
        IPublishedValueFallback publishedValueFallback,
        IPublishedUrlProvider publishedUrlProvider,
        UrlMode urlMode = UrlMode.Default) =>
        mediaWithCrops.GetCropUrl(
            imageUrlGenerator,
            publishedValueFallback,
            publishedUrlProvider,
            cropAlias: cropAlias,
            useCropDimensions: true,
            urlMode: urlMode);

    /// <summary>
    ///     Gets the crop URL by using only the specified <paramref name="imageCropperValue" />.
    /// </summary>
    /// <param name="mediaItem">The media item.</param>
    /// <param name="imageCropperValue">The image cropper value.</param>
    /// <param name="cropAlias">The crop alias.</param>
    /// <param name="imageUrlGenerator">The image URL generator.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    /// <param name="publishedUrlProvider">The published URL provider.</param>
    /// <param name="urlMode">The url mode.s</param>
    /// <returns>
    ///     The image crop URL.
    /// </returns>
    public static string? GetCropUrl(
        this IPublishedContent mediaItem,
        ImageCropperValue imageCropperValue,
        string cropAlias,
        IImageUrlGenerator imageUrlGenerator,
        IPublishedValueFallback publishedValueFallback,
        IPublishedUrlProvider publishedUrlProvider,
        UrlMode urlMode = UrlMode.Default) =>
        mediaItem.GetCropUrl(
            imageUrlGenerator,
            publishedValueFallback,
            publishedUrlProvider,
            imageCropperValue,
            true,
            cropAlias: cropAlias,
            useCropDimensions: true,
            urlMode: urlMode);

    /// <summary>
    ///     Gets the underlying image processing service URL by the crop alias using the specified property containing the
    ///     image cropper JSON data on the IPublishedContent item.
    /// </summary>
    /// <param name="mediaItem">The IPublishedContent item.</param>
    /// <param name="propertyAlias">The property alias of the property containing the JSON data e.g. umbracoFile.</param>
    /// <param name="cropAlias">The crop alias e.g. thumbnail.</param>
    /// <param name="imageUrlGenerator">The image URL generator.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    /// <param name="publishedUrlProvider">The published URL provider.</param>
    /// <param name="urlMode">The url mode.</param>
    /// <returns>
    ///     The URL of the cropped image.
    /// </returns>
    public static string? GetCropUrl(
        this IPublishedContent mediaItem,
        string propertyAlias,
        string cropAlias,
        IImageUrlGenerator imageUrlGenerator,
        IPublishedValueFallback publishedValueFallback,
        IPublishedUrlProvider publishedUrlProvider,
        UrlMode urlMode = UrlMode.Default) =>
        mediaItem.GetCropUrl(
            imageUrlGenerator,
            publishedValueFallback,
            publishedUrlProvider,
            propertyAlias: propertyAlias,
            cropAlias: cropAlias,
            useCropDimensions: true,
            urlMode: urlMode);

    /// <summary>
    ///     Gets the underlying image processing service URL by the crop alias using the specified property containing the
    ///     image cropper JSON data on the MediaWithCrops content item.
    /// </summary>
    /// <param name="mediaWithCrops">The MediaWithCrops item.</param>
    /// <param name="propertyAlias">The property alias of the property containing the JSON data e.g. umbracoFile.</param>
    /// <param name="cropAlias">The crop alias e.g. thumbnail.</param>
    /// <param name="imageUrlGenerator">The image URL generator.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    /// <param name="publishedUrlProvider">The published URL provider.</param>
    /// <param name="urlMode">The url mode.</param>
    /// <returns>
    ///     The URL of the cropped image.
    /// </returns>
    public static string? GetCropUrl(
        this MediaWithCrops mediaWithCrops,
        IPublishedValueFallback publishedValueFallback,
        IPublishedUrlProvider publishedUrlProvider,
        string propertyAlias,
        string cropAlias,
        IImageUrlGenerator imageUrlGenerator,
        UrlMode urlMode = UrlMode.Default) =>
        mediaWithCrops.GetCropUrl(
            imageUrlGenerator,
            publishedValueFallback,
            publishedUrlProvider,
            propertyAlias: propertyAlias,
            cropAlias: cropAlias,
            useCropDimensions: true,
            urlMode: urlMode);

    /// <summary>
    ///     Gets the underlying image processing service URL from the IPublishedContent item.
    /// </summary>
    /// <param name="mediaItem">The IPublishedContent item.</param>
    /// <param name="imageUrlGenerator">The image URL generator.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    /// <param name="publishedUrlProvider">The published URL provider.</param>
    /// <param name="width">The width of the output image.</param>
    /// <param name="height">The height of the output image.</param>
    /// <param name="propertyAlias">Property alias of the property containing the JSON data.</param>
    /// <param name="cropAlias">The crop alias.</param>
    /// <param name="quality">Quality percentage of the output image.</param>
    /// <param name="imageCropMode">The image crop mode.</param>
    /// <param name="imageCropAnchor">The image crop anchor.</param>
    /// <param name="preferFocalPoint">
    ///     Use focal point, to generate an output image using the focal point instead of the
    ///     predefined crop.
    /// </param>
    /// <param name="useCropDimensions">
    ///     Use crop dimensions to have the output image sized according to the predefined crop
    ///     sizes, this will override the width and height parameters.
    /// </param>
    /// <param name="cacheBuster">
    ///     Add a serialized date of the last edit of the item to ensure client cache refresh when
    ///     updated.
    /// </param>
    /// <param name="furtherOptions">
    ///     These are any query string parameters (formatted as query strings) that ImageProcessor supports. For example:
    ///     <example><![CDATA[
    /// furtherOptions: "bgcolor=fff"
    /// ]]></example>
    /// </param>
    /// <param name="urlMode">The url mode.</param>
    /// <returns>
    ///     The URL of the cropped image.
    /// </returns>
    public static string? GetCropUrl(
        this IPublishedContent mediaItem,
        IImageUrlGenerator imageUrlGenerator,
        IPublishedValueFallback publishedValueFallback,
        IPublishedUrlProvider publishedUrlProvider,
        int? width = null,
        int? height = null,
        string propertyAlias = Constants.Conventions.Media.File,
        string? cropAlias = null,
        int? quality = null,
        ImageCropMode? imageCropMode = null,
        ImageCropAnchor? imageCropAnchor = null,
        bool preferFocalPoint = false,
        bool useCropDimensions = false,
        bool cacheBuster = true,
        string? furtherOptions = null,
        UrlMode urlMode = UrlMode.Default) =>
        mediaItem.GetCropUrl(
            imageUrlGenerator,
            publishedValueFallback,
            publishedUrlProvider,
            null,
            false,
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
            urlMode);

    /// <summary>
    ///     Gets the underlying image processing service URL from the MediaWithCrops item.
    /// </summary>
    /// <param name="mediaWithCrops">The MediaWithCrops item.</param>
    /// <param name="imageUrlGenerator">The image URL generator.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    /// <param name="publishedUrlProvider">The published URL provider.</param>
    /// <param name="width">The width of the output image.</param>
    /// <param name="height">The height of the output image.</param>
    /// <param name="propertyAlias">Property alias of the property containing the JSON data.</param>
    /// <param name="cropAlias">The crop alias.</param>
    /// <param name="quality">Quality percentage of the output image.</param>
    /// <param name="imageCropMode">The image crop mode.</param>
    /// <param name="imageCropAnchor">The image crop anchor.</param>
    /// <param name="preferFocalPoint">
    ///     Use focal point, to generate an output image using the focal point instead of the
    ///     predefined crop.
    /// </param>
    /// <param name="useCropDimensions">
    ///     Use crop dimensions to have the output image sized according to the predefined crop
    ///     sizes, this will override the width and height parameters.
    /// </param>
    /// <param name="cacheBuster">
    ///     Add a serialized date of the last edit of the item to ensure client cache refresh when
    ///     updated.
    /// </param>
    /// <param name="furtherOptions">
    ///     These are any query string parameters (formatted as query strings) that ImageProcessor supports. For example:
    ///     <example><![CDATA[
    /// furtherOptions: "bgcolor=fff"
    /// ]]></example>
    /// </param>
    /// <param name="urlMode">The url mode.</param>
    /// <returns>
    ///     The URL of the cropped image.
    /// </returns>
    public static string? GetCropUrl(
        this MediaWithCrops mediaWithCrops,
        IImageUrlGenerator imageUrlGenerator,
        IPublishedValueFallback publishedValueFallback,
        IPublishedUrlProvider publishedUrlProvider,
        int? width = null,
        int? height = null,
        string propertyAlias = Constants.Conventions.Media.File,
        string? cropAlias = null,
        int? quality = null,
        ImageCropMode? imageCropMode = null,
        ImageCropAnchor? imageCropAnchor = null,
        bool preferFocalPoint = false,
        bool useCropDimensions = false,
        bool cacheBuster = true,
        string? furtherOptions = null,
        UrlMode urlMode = UrlMode.Default)
    {
        if (mediaWithCrops == null)
        {
            throw new ArgumentNullException(nameof(mediaWithCrops));
        }

        return mediaWithCrops.Content.GetCropUrl(
            imageUrlGenerator,
            publishedValueFallback,
            publishedUrlProvider,
            mediaWithCrops.LocalCrops,
            false,
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
            urlMode);
    }

    /// <summary>
    ///     Gets the underlying image processing service URL from the image path.
    /// </summary>
    /// <param name="imageUrl">The image URL.</param>
    /// <param name="imageUrlGenerator">The image URL generator.</param>
    /// <param name="width">The width of the output image.</param>
    /// <param name="height">The height of the output image.</param>
    /// <param name="imageCropperValue">The Json data from the Umbraco Core Image Cropper property editor.</param>
    /// <param name="cropAlias">The crop alias.</param>
    /// <param name="quality">Quality percentage of the output image.</param>
    /// <param name="imageCropMode">The image crop mode.</param>
    /// <param name="imageCropAnchor">The image crop anchor.</param>
    /// <param name="preferFocalPoint">
    ///     Use focal point to generate an output image using the focal point instead of the
    ///     predefined crop if there is one.
    /// </param>
    /// <param name="useCropDimensions">
    ///     Use crop dimensions to have the output image sized according to the predefined crop
    ///     sizes, this will override the width and height parameters.
    /// </param>
    /// <param name="cacheBusterValue">
    ///     Add a serialized date of the last edit of the item to ensure client cache refresh when
    ///     updated.
    /// </param>
    /// <param name="furtherOptions">
    ///     These are any query string parameters (formatted as query strings) that the underlying image processing service
    ///     supports. For example:
    ///     <example><![CDATA[
    /// furtherOptions: "bgcolor=fff"
    /// ]]></example>
    /// </param>
    /// <returns>
    ///     The URL of the cropped image.
    /// </returns>
    public static string? GetCropUrl(
        this string imageUrl,
        IImageUrlGenerator imageUrlGenerator,
        int? width = null,
        int? height = null,
        string? imageCropperValue = null,
        string? cropAlias = null,
        int? quality = null,
        ImageCropMode? imageCropMode = null,
        ImageCropAnchor? imageCropAnchor = null,
        bool preferFocalPoint = false,
        bool useCropDimensions = false,
        string? cacheBusterValue = null,
        string? furtherOptions = null)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        ImageCropperValue? cropDataSet = null;
        if (string.IsNullOrEmpty(imageCropperValue) == false && imageCropperValue.DetectIsJson() &&
            (imageCropMode == ImageCropMode.Crop || imageCropMode == null))
        {
            cropDataSet = imageCropperValue.DeserializeImageCropperValue();
        }

        return GetCropUrl(
            imageUrl,
            imageUrlGenerator,
            cropDataSet,
            width,
            height,
            cropAlias,
            quality,
            imageCropMode,
            imageCropAnchor,
            preferFocalPoint,
            useCropDimensions,
            cacheBusterValue,
            furtherOptions);
    }

    /// <summary>
    ///     Gets the underlying image processing service URL from the image path.
    /// </summary>
    /// <param name="imageUrl">The image URL.</param>
    /// <param name="imageUrlGenerator">
    ///     The generator that will process all the options and the image URL to return a full
    ///     image URLs with all processing options appended.
    /// </param>
    /// <param name="cropDataSet">The crop data set.</param>
    /// <param name="width">The width of the output image.</param>
    /// <param name="height">The height of the output image.</param>
    /// <param name="cropAlias">The crop alias.</param>
    /// <param name="quality">Quality percentage of the output image.</param>
    /// <param name="imageCropMode">The image crop mode.</param>
    /// <param name="imageCropAnchor">The image crop anchor.</param>
    /// <param name="preferFocalPoint">
    ///     Use focal point to generate an output image using the focal point instead of the
    ///     predefined crop if there is one.
    /// </param>
    /// <param name="useCropDimensions">
    ///     Use crop dimensions to have the output image sized according to the predefined crop
    ///     sizes, this will override the width and height parameters.
    /// </param>
    /// <param name="cacheBusterValue">
    ///     Add a serialized date of the last edit of the item to ensure client cache refresh when
    ///     updated.
    /// </param>
    /// <param name="furtherOptions">
    ///     These are any query string parameters (formatted as query strings) that the underlying image processing service
    ///     supports. For example:
    ///     <example><![CDATA[
    /// furtherOptions: "bgcolor=fff"
    /// ]]></example>
    /// </param>
    /// <returns>
    ///     The URL of the cropped image.
    /// </returns>
    public static string? GetCropUrl(
        this string imageUrl,
        IImageUrlGenerator imageUrlGenerator,
        ImageCropperValue? cropDataSet,
        int? width = null,
        int? height = null,
        string? cropAlias = null,
        int? quality = null,
        ImageCropMode? imageCropMode = null,
        ImageCropAnchor? imageCropAnchor = null,
        bool preferFocalPoint = false,
        bool useCropDimensions = false,
        string? cacheBusterValue = null,
        string? furtherOptions = null)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        ImageUrlGenerationOptions options;
        if (cropDataSet != null && (imageCropMode == ImageCropMode.Crop || imageCropMode == null))
        {
            ImageCropperValue.ImageCropperCrop? crop = cropDataSet.GetCrop(cropAlias);

            // If a crop was specified, but not found, return null
            if (crop == null && !string.IsNullOrWhiteSpace(cropAlias))
            {
                return null;
            }

            options = cropDataSet.GetCropBaseOptions(imageUrl, crop, preferFocalPoint || string.IsNullOrWhiteSpace(cropAlias));

            if (crop != null && useCropDimensions)
            {
                width = crop.Width;
                height = crop.Height;
            }

            // Calculate missing dimension if a predefined crop has been specified, but has no coordinates
            if (crop != null && string.IsNullOrEmpty(cropAlias) == false && crop.Coordinates == null)
            {
                if (width != null && height == null)
                {
                    height = (int)MathF.Round(width.Value * ((float)crop.Height / crop.Width));
                }
                else if (width == null && height != null)
                {
                    width = (int)MathF.Round(height.Value * ((float)crop.Width / crop.Height));
                }
            }
        }
        else
        {
            options = new ImageUrlGenerationOptions(imageUrl)
            {
                ImageCropMode = imageCropMode ?? ImageCropMode.Pad, // Not sure why we default to Pad
                ImageCropAnchor = imageCropAnchor,
            };
        }

        options.Quality = quality;
        options.Width = width;
        options.Height = height;
        options.FurtherOptions = furtherOptions;
        options.CacheBusterValue = cacheBusterValue;

        return imageUrlGenerator.GetImageUrl(options);
    }

    private static string? GetCropUrl(
        this IPublishedContent mediaItem,
        IImageUrlGenerator imageUrlGenerator,
        IPublishedValueFallback publishedValueFallback,
        IPublishedUrlProvider publishedUrlProvider,
        ImageCropperValue? localCrops,
        bool localCropsOnly,
        int? width = null,
        int? height = null,
        string propertyAlias = Constants.Conventions.Media.File,
        string? cropAlias = null,
        int? quality = null,
        ImageCropMode? imageCropMode = null,
        ImageCropAnchor? imageCropAnchor = null,
        bool preferFocalPoint = false,
        bool useCropDimensions = false,
        bool cacheBuster = true,
        string? furtherOptions = null,
        UrlMode urlMode = UrlMode.Default)
    {
        if (mediaItem == null)
        {
            throw new ArgumentNullException(nameof(mediaItem));
        }

        if (mediaItem.HasProperty(propertyAlias) == false || mediaItem.HasValue(propertyAlias) == false)
        {
            return null;
        }

        var mediaItemUrl = mediaItem.MediaUrl(publishedUrlProvider, propertyAlias: propertyAlias, mode: urlMode);

        // Only get crops from media when required and used
        if (localCropsOnly == false && (imageCropMode == ImageCropMode.Crop || imageCropMode == null))
        {
            // Get the default cropper value from the value converter
            var cropperValue = mediaItem.Value(publishedValueFallback, propertyAlias);

            var mediaCrops = cropperValue as ImageCropperValue;

            if (mediaCrops == null && cropperValue is JObject jobj)
            {
                mediaCrops = jobj.ToObject<ImageCropperValue>();
            }

            if (mediaCrops == null && cropperValue is string imageCropperValue &&
                string.IsNullOrEmpty(imageCropperValue) == false && imageCropperValue.DetectIsJson())
            {
                mediaCrops = imageCropperValue.DeserializeImageCropperValue();
            }

            // Merge crops
            if (localCrops == null)
            {
                localCrops = mediaCrops;
            }
            else if (mediaCrops != null)
            {
                localCrops = localCrops.Merge(mediaCrops);
            }
        }

        var cacheBusterValue =
            cacheBuster ? mediaItem.UpdateDate.ToFileTimeUtc().ToString(CultureInfo.InvariantCulture) : null;

        return GetCropUrl(
            mediaItemUrl,
            imageUrlGenerator,
            localCrops,
            width,
            height,
            cropAlias,
            quality,
            imageCropMode,
            imageCropAnchor,
            preferFocalPoint,
            useCropDimensions,
            cacheBusterValue,
            furtherOptions);
    }
}
