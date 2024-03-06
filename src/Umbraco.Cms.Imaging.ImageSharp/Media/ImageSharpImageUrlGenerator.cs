using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Imaging.ImageSharp.ImageProcessors;
using static Umbraco.Cms.Core.Models.ImageUrlGenerationOptions;

namespace Umbraco.Cms.Imaging.ImageSharp.Media;

/// <summary>
/// Exposes a method that generates an image URL based on the specified options that can be processed by ImageSharp.
/// </summary>
/// <seealso cref="IImageUrlGenerator" />
public sealed class ImageSharpImageUrlGenerator : IImageUrlGenerator
{
    private readonly RequestAuthorizationUtilities? _requestAuthorizationUtilities;
    private readonly ImageSharpMiddlewareOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSharpImageUrlGenerator" /> class.
    /// </summary>
    /// <param name="configuration">The ImageSharp configuration.</param>
    /// <param name="requestAuthorizationUtilities">Contains helpers that allow authorization of image requests.</param>
    /// <param name="options"></param>
    public ImageSharpImageUrlGenerator(
        Configuration configuration,
        RequestAuthorizationUtilities? requestAuthorizationUtilities,
        IOptions<ImageSharpMiddlewareOptions> options)
        : this(configuration.ImageFormats.SelectMany(f => f.FileExtensions).ToArray(), options, requestAuthorizationUtilities)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSharpImageUrlGenerator" /> class.
    /// </summary>
    /// <param name="configuration">The ImageSharp configuration.</param>
    [Obsolete("Use ctor with all params - This will be removed in Umbraco 13.")]
    public ImageSharpImageUrlGenerator(Configuration configuration)
        : this(configuration, StaticServiceProvider.Instance.GetService<RequestAuthorizationUtilities>(), StaticServiceProvider.Instance.GetRequiredService<IOptions<ImageSharpMiddlewareOptions>>())
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSharpImageUrlGenerator" /> class.
    /// </summary>
    /// <param name="supportedImageFileTypes">The supported image file types/extensions.</param>
    /// <param name="options">The ImageSharp middleware options.</param>
    /// <param name="requestAuthorizationUtilities">Contains helpers that allow authorization of image requests.</param>
    /// <remarks>
    /// This constructor is only used for testing.
    /// </remarks>
    internal ImageSharpImageUrlGenerator(IEnumerable<string> supportedImageFileTypes, IOptions<ImageSharpMiddlewareOptions> options, RequestAuthorizationUtilities? requestAuthorizationUtilities = null)
    {
        SupportedImageFileTypes = supportedImageFileTypes;
        _requestAuthorizationUtilities = requestAuthorizationUtilities;
        _options = options.Value;
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
        Dictionary<string, StringValues> furtherOptions = QueryHelpers.ParseQuery(options.FurtherOptions);

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

        if (furtherOptions.Remove(FormatWebProcessor.Format, out StringValues format))
        {
            queryString.Add(FormatWebProcessor.Format, format.ToString());
        }

        if (options.Quality is int quality)
        {
            queryString.Add(QualityWebProcessor.Quality, quality.ToString(CultureInfo.InvariantCulture));
        }

        foreach (KeyValuePair<string, StringValues> kvp in furtherOptions)
        {
            queryString.Add(kvp.Key, kvp.Value);
        }

        if (options.CacheBusterValue is string cacheBusterValue && !string.IsNullOrEmpty(cacheBusterValue))
        {
            queryString.Add("v", cacheBusterValue);
        }

        // If no secret is we'll completely skip this whole thing, in theory the ComputeHMACAsync should just return null imidiately, but still no reason to create
        if (_options.HMACSecretKey.Length != 0 && _requestAuthorizationUtilities is not null)
        {
            var uri = QueryHelpers.AddQueryString(options.ImageUrl, queryString);

            var token = _requestAuthorizationUtilities.ComputeHMAC(uri, CommandHandling.Sanitize);
            if (string.IsNullOrEmpty(token) is false)
            {
                queryString.Add(RequestAuthorizationUtilities.TokenCommand, token);
            }
        }

        return QueryHelpers.AddQueryString(options.ImageUrl, queryString);
    }
}
