using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Extensions;

public static class FriendlyImageCropperTemplateExtensions
{
    private static IImageUrlGenerator ImageUrlGenerator { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IImageUrlGenerator>();

    private static IPublishedValueFallback PublishedValueFallback { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IPublishedValueFallback>();

    private static IPublishedUrlProvider PublishedUrlProvider { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IPublishedUrlProvider>();

    /// <summary>
    ///     Gets the underlying image processing service URL by the crop alias (from the "umbracoFile" property alias) on the
    ///     IPublishedContent item.
    /// </summary>
    /// <param name="mediaItem">The IPublishedContent item.</param>
    /// <param name="cropAlias">The crop alias e.g. thumbnail.</param>
    /// <param name="urlMode">The url mode.</param>
    /// <returns>
    ///     The URL of the cropped image.
    /// </returns>
    public static string? GetCropUrl(
        this IPublishedContent mediaItem,
        string cropAlias,
        UrlMode urlMode = UrlMode.Default) =>
        mediaItem.GetCropUrl(cropAlias, ImageUrlGenerator, PublishedValueFallback, PublishedUrlProvider, urlMode);

    /// <summary>
    ///     Gets the underlying image processing service URL by the crop alias (from the "umbracoFile" property alias in the
    ///     MediaWithCrops content item) on the MediaWithCrops item.
    /// </summary>
    /// <param name="mediaWithCrops">The MediaWithCrops item.</param>
    /// <param name="cropAlias">The crop alias e.g. thumbnail.</param>
    /// <param name="urlMode">The url mode.</param>
    /// <returns>
    ///     The URL of the cropped image.
    /// </returns>
    public static string? GetCropUrl(this MediaWithCrops mediaWithCrops, string cropAlias, UrlMode urlMode = UrlMode.Default)
        => mediaWithCrops.GetCropUrl(cropAlias, ImageUrlGenerator, PublishedValueFallback, PublishedUrlProvider, urlMode);

    /// <summary>
    ///     Gets the crop URL by using only the specified <paramref name="imageCropperValue" />.
    /// </summary>
    /// <param name="mediaItem">The media item.</param>
    /// <param name="imageCropperValue">The image cropper value.</param>
    /// <param name="cropAlias">The crop alias.</param>
    /// <param name="urlMode">The url mode.</param>
    /// <returns>
    ///     The image crop URL.
    /// </returns>
    public static string? GetCropUrl(
        this IPublishedContent mediaItem,
        ImageCropperValue imageCropperValue,
        string cropAlias,
        UrlMode urlMode = UrlMode.Default)
        => mediaItem.GetCropUrl(imageCropperValue, cropAlias, ImageUrlGenerator, PublishedValueFallback, PublishedUrlProvider, urlMode);

    /// <summary>
    ///     Gets the underlying image processing service URL by the crop alias using the specified property containing the
    ///     image cropper JSON data on the IPublishedContent item.
    /// </summary>
    /// <param name="mediaItem">The IPublishedContent item.</param>
    /// <param name="propertyAlias">The property alias of the property containing the JSON data e.g. umbracoFile.</param>
    /// <param name="cropAlias">The crop alias e.g. thumbnail.</param>
    /// <param name="urlMode">The url mode.</param>
    /// <returns>
    ///     The URL of the cropped image.
    /// </returns>
    public static string? GetCropUrl(
        this IPublishedContent mediaItem,
        string propertyAlias,
        string cropAlias,
        UrlMode urlMode = UrlMode.Default) =>
        mediaItem.GetCropUrl(propertyAlias, cropAlias, ImageUrlGenerator, PublishedValueFallback, PublishedUrlProvider, urlMode);

    /// <summary>
    ///     Gets the underlying image processing service URL by the crop alias using the specified property containing the
    ///     image cropper JSON data on the MediaWithCrops content item.
    /// </summary>
    /// <param name="mediaWithCrops">The MediaWithCrops item.</param>
    /// <param name="propertyAlias">The property alias of the property containing the JSON data e.g. umbracoFile.</param>
    /// <param name="cropAlias">The crop alias e.g. thumbnail.</param>
    /// <param name="urlMode">The url mode.</param>
    /// <returns>
    ///     The URL of the cropped image.
    /// </returns>
    public static string? GetCropUrl(this MediaWithCrops mediaWithCrops, string propertyAlias, string cropAlias, UrlMode urlMode = UrlMode.Default)
        => mediaWithCrops.GetCropUrl(propertyAlias, cropAlias, ImageUrlGenerator, PublishedValueFallback, PublishedUrlProvider, urlMode);

    /// <summary>
    ///     Gets the underlying image processing service URL from the IPublishedContent item.
    /// </summary>
    /// <param name="mediaItem">The IPublishedContent item.</param>
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
    ///     These are any query string parameters (formatted as query strings) that the underlying image processing service
    ///     supports. For example:
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
            urlMode);

    /// <summary>
    ///     Gets the underlying image processing service URL from the MediaWithCrops item.
    /// </summary>
    /// <param name="mediaWithCrops">The MediaWithCrops item.</param>
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
    ///     These are any query string parameters (formatted as query strings) that the underlying image processing service
    ///     supports. For example:
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
        => mediaWithCrops.GetCropUrl(
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
            urlMode);

    /// <summary>
    ///     Gets the underlying image processing service URL from the image path.
    /// </summary>
    /// <param name="imageUrl">The image URL.</param>
    /// <param name="width">The width of the output image.</param>
    /// <param name="height">The height of the output image.</param>
    /// <param name="imageCropperValue">The JSON data from the Umbraco Core Image Cropper property editor.</param>
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
            furtherOptions);

    /// <summary>
    ///     Gets the underlying image processing service URL from the image path.
    /// </summary>
    /// <param name="imageUrl">The image URL.</param>
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
        ImageCropperValue cropDataSet,
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
        => imageUrl.GetCropUrl(
            ImageUrlGenerator,
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

    [Obsolete(
        "Use GetCropUrl to merge local and media crops, get automatic cache buster value and have more parameters.")]
    public static string GetLocalCropUrl(
        this MediaWithCrops mediaWithCrops,
        string alias,
        string? cacheBusterValue = null) => mediaWithCrops.LocalCrops.Src +
                                            mediaWithCrops.LocalCrops.GetCropUrl(alias, ImageUrlGenerator, cacheBusterValue: cacheBusterValue);
}
