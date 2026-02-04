using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Imaging.ImageSharp.ImageProcessors;
using static Umbraco.Cms.Core.Models.ImageUrlGenerationOptions;

namespace Umbraco.Cms.Imaging.ImageSharp.Media;

/// <summary>
///     Exposes a method that generates an image URL based on the specified options that can be processed by ImageSharp.
/// </summary>
/// <seealso cref="IImageUrlGenerator" />
public sealed class ImageSharpImageUrlGenerator : IImageUrlGenerator
{
    /// <inheritdoc />
    public IEnumerable<string> SupportedImageFileTypes { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageSharpImageUrlGenerator" /> class.
    /// </summary>
    /// <param name="configuration">The ImageSharp configuration.</param>
    public ImageSharpImageUrlGenerator(Configuration configuration)
        : this(configuration.ImageFormats.SelectMany(f => f.FileExtensions).ToArray())
    { }

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
    public string? GetImageUrl(ImageUrlGenerationOptions? options)
    {
        if (options?.ImageUrl == null)
        {
            return null;
        }

        var queryString = new Dictionary<string, string?>();
        Dictionary<string, StringValues> furtherOptions = QueryHelpers.ParseQuery(options.FurtherOptions);

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

        // Determine format: explicit > furtherOptions > default for non-images
        string? formatValue = options.Format;
        if (string.IsNullOrWhiteSpace(formatValue) && furtherOptions.Remove(FormatWebProcessor.Format, out StringValues format))
        {
            formatValue = format[0];
        }

        if (string.IsNullOrWhiteSpace(formatValue) && RequiresFormatConversion(options.ImageUrl))
        {
            formatValue = "webp"; // Default for processable non-image files (PDF, etc.)
        }

        if (!string.IsNullOrWhiteSpace(formatValue))
        {
            queryString.Add(FormatWebProcessor.Format, formatValue);
        }

        if (options.Quality is not null)
        {
            queryString.Add(QualityWebProcessor.Quality, options.Quality?.ToString(CultureInfo.InvariantCulture));
        }

        foreach (KeyValuePair<string, StringValues> kvp in furtherOptions)
        {
            queryString.Add(kvp.Key, kvp.Value);
        }

        if (options.CacheBusterValue is not null && !string.IsNullOrWhiteSpace(options.CacheBusterValue))
        {
            queryString.Add("rnd", options.CacheBusterValue);
        }

        return QueryHelpers.AddQueryString(options.ImageUrl, queryString);
    }

    private static readonly string[] TrueImageFormats = { "jpg", "jpeg", "png", "gif", "webp", "bmp", "tif", "tiff" };

    /// <summary>
    /// Determines if the source file requires format conversion because it's not a true image format.
    /// </summary>
    /// <param name="imageUrl">The source image URL.</param>
    /// <returns>True if the file needs conversion to an image format; otherwise, false.</returns>
    private static bool RequiresFormatConversion(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return false;
        }

        try
        {
            // Extract extension from URL (handle query strings)
            var uri = new Uri(imageUrl, UriKind.RelativeOrAbsolute);
            var path = uri.IsAbsoluteUri ? uri.LocalPath : imageUrl.Split('?')[0];
            var extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();

            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }

            // True image formats that don't require conversion
            return !TrueImageFormats.Contains(extension);
        }
        catch (UriFormatException)
        {
            // If URL parsing fails, treat as non-image requiring conversion
            return false;
        }
    }
}
