using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.ImageProcessors;
using static Umbraco.Cms.Core.Models.ImageUrlGenerationOptions;

namespace Umbraco.Cms.Web.Common.Media;

/// <summary>
///     Exposes a method that generates an image URL based on the specified options that can be processed by ImageSharp.
/// </summary>
/// <seealso cref="IImageUrlGenerator" />
public class ImageSharpImageUrlGenerator : IImageUrlGenerator
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageSharpImageUrlGenerator" /> class.
    /// </summary>
    /// <param name="configuration">The ImageSharp configuration.</param>
    public ImageSharpImageUrlGenerator(Configuration configuration)
        : this(configuration.ImageFormats.SelectMany(f => f.FileExtensions).ToArray())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageSharpImageUrlGenerator" /> class.
    /// </summary>
    /// <param name="supportedImageFileTypes">The supported image file types/extensions.</param>
    /// <remarks>
    ///     This constructor is only used for testing.
    /// </remarks>
    internal ImageSharpImageUrlGenerator(IEnumerable<string> supportedImageFileTypes) =>
        SupportedImageFileTypes = supportedImageFileTypes;

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

        if (options.Crop is not null)
        {
            CropCoordinates? crop = options.Crop;
            queryString.Add(
                CropWebProcessor.Coordinates,
                FormattableString.Invariant($"{crop.Left},{crop.Top},{crop.Right},{crop.Bottom}"));
        }

        if (options.FocalPoint is not null)
        {
            queryString.Add(ResizeWebProcessor.Xy, FormattableString.Invariant($"{options.FocalPoint.Left},{options.FocalPoint.Top}"));
        }

        if (options.ImageCropMode is not null)
        {
            queryString.Add(ResizeWebProcessor.Mode, options.ImageCropMode.ToString()?.ToLowerInvariant());
        }

        if (options.ImageCropAnchor is not null)
        {
            queryString.Add(ResizeWebProcessor.Anchor, options.ImageCropAnchor.ToString()?.ToLowerInvariant());
        }

        if (options.Width is not null)
        {
            queryString.Add(ResizeWebProcessor.Width, options.Width?.ToString(CultureInfo.InvariantCulture));
        }

        if (options.Height is not null)
        {
            queryString.Add(ResizeWebProcessor.Height, options.Height?.ToString(CultureInfo.InvariantCulture));
        }

        if (options.Quality is not null)
        {
            queryString.Add(QualityWebProcessor.Quality, options.Quality?.ToString(CultureInfo.InvariantCulture));
        }

        foreach (KeyValuePair<string, StringValues> kvp in QueryHelpers.ParseQuery(options.FurtherOptions))
        {
            queryString.Add(kvp.Key, kvp.Value);
        }

        if (options.CacheBusterValue is not null && !string.IsNullOrWhiteSpace(options.CacheBusterValue))
        {
            queryString.Add("rnd", options.CacheBusterValue);
        }

        return QueryHelpers.AddQueryString(options.ImageUrl, queryString);
    }
}
