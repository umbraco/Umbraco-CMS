using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Cms.Web.Common.ImageProcessors;
using static Umbraco.Cms.Core.Models.ImageUrlGenerationOptions;

namespace Umbraco.Cms.Web.Common.Media;

/// <summary>
/// Exposes a method that generates an image URL based on the specified options that can be processed by ImageSharp.
/// </summary>
/// <seealso cref="IImageUrlGenerator" />
public class ImageSharpImageUrlGenerator : IImageUrlGenerator
{
    private readonly ImageSharpRequestAuthorizationUtilities? _requestAuthorizationUtilities;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSharpImageUrlGenerator" /> class.
    /// </summary>
    /// <param name="configuration">The ImageSharp configuration.</param>
    /// <param name="requestAuthorizationUtilities">Contains helpers that allow authorization of image requests.</param>
    public ImageSharpImageUrlGenerator(Configuration configuration, ImageSharpRequestAuthorizationUtilities? requestAuthorizationUtilities)
        : this(configuration.ImageFormats.SelectMany(f => f.FileExtensions).ToArray(), requestAuthorizationUtilities)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSharpImageUrlGenerator" /> class.
    /// </summary>
    /// <param name="configuration">The ImageSharp configuration.</param>
    [Obsolete("Use ctor with all params - This will be removed in Umbraco 12.")]
    public ImageSharpImageUrlGenerator(Configuration configuration)
        : this(configuration, StaticServiceProvider.Instance.GetService<ImageSharpRequestAuthorizationUtilities>())
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSharpImageUrlGenerator" /> class.
    /// </summary>
    /// <param name="supportedImageFileTypes">The supported image file types/extensions.</param>
    /// <param name="requestAuthorizationUtilities">Contains helpers that allow authorization of image requests.</param>
    /// <remarks>
    /// This constructor is only used for testing.
    /// </remarks>
    internal ImageSharpImageUrlGenerator(IEnumerable<string> supportedImageFileTypes, ImageSharpRequestAuthorizationUtilities? requestAuthorizationUtilities = null)
    {
        SupportedImageFileTypes = supportedImageFileTypes;
        _requestAuthorizationUtilities = requestAuthorizationUtilities;
    }

    /// <inheritdoc />
    public IEnumerable<string> SupportedImageFileTypes { get; }

    /// <inheritdoc />
    public string? GetImageUrl(ImageUrlGenerationOptions? options)
    {
        if (options?.ImageUrl == null)
        {
            return null;
        }

        var queryString = new Dictionary<string, string?>();

        if (options.Crop is CropCoordinates crop)
        {
            queryString.Add(CropWebProcessor.Coordinates, FormattableString.Invariant($"{crop.Left},{crop.Top},{crop.Right},{crop.Bottom}"));
        }

        if (options.FocalPoint is FocalPointPosition focalPoint)
        {
            queryString.Add(ResizeWebProcessor.Xy, FormattableString.Invariant($"{focalPoint.Left},{focalPoint.Top}"));
        }

        if (options.ImageCropMode is ImageCropMode imageCropMode)
        {
            queryString.Add(ResizeWebProcessor.Mode, imageCropMode.ToString().ToLowerInvariant());
        }

        if (options.ImageCropAnchor is ImageCropAnchor imageCropAnchor)
        {
            queryString.Add(ResizeWebProcessor.Anchor, imageCropAnchor.ToString().ToLowerInvariant());
        }

        if (options.Width is int width)
        {
            queryString.Add(ResizeWebProcessor.Width, width.ToString(CultureInfo.InvariantCulture));
        }

        if (options.Height is int height)
        {
            queryString.Add(ResizeWebProcessor.Height, height.ToString(CultureInfo.InvariantCulture));
        }

        if (options.Quality is int quality)
        {
            queryString.Add(QualityWebProcessor.Quality, quality.ToString(CultureInfo.InvariantCulture));
        }

        foreach (KeyValuePair<string, StringValues> kvp in QueryHelpers.ParseQuery(options.FurtherOptions))
        {
            queryString.Add(kvp.Key, kvp.Value);
        }

        if (options.CacheBusterValue is string cacheBusterValue && !string.IsNullOrEmpty(cacheBusterValue))
        {
            queryString.Add("v", cacheBusterValue);
        }

        if (_requestAuthorizationUtilities is not null)
        {
            var uri = QueryHelpers.AddQueryString(options.ImageUrl, queryString);
            if (_requestAuthorizationUtilities.ComputeHMAC(uri, CommandHandling.Sanitize) is string token && !string.IsNullOrEmpty(token))
            {
                queryString.Add(ImageSharpRequestAuthorizationUtilities.TokenCommand, token);
            }
        }

        return QueryHelpers.AddQueryString(options.ImageUrl, queryString);
    }
}
