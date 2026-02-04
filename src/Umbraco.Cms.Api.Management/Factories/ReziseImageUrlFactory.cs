using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class ReziseImageUrlFactory : IReziseImageUrlFactory
{
    private readonly IImageUrlGenerator _imageUrlGenerator;
    private readonly ContentSettings _contentSettings;
    private readonly ContentImagingSettings _imagingSettings;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IAbsoluteUrlBuilder _absoluteUrlBuilder;

    [Obsolete("Use the constructor with IOptions<ContentImagingSettings> parameter. This constructor will be removed in v19.")]
    public ReziseImageUrlFactory(
        IImageUrlGenerator imageUrlGenerator,
        IOptions<ContentSettings> contentSettings,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IAbsoluteUrlBuilder absoluteUrlBuilder)
        : this(
            imageUrlGenerator,
            contentSettings,
            StaticServiceProvider.Instance.GetRequiredService<IOptions<ContentImagingSettings>>(),
            mediaUrlGenerators,
            absoluteUrlBuilder)
    {
    }

    public ReziseImageUrlFactory(
        IImageUrlGenerator imageUrlGenerator,
        IOptions<ContentSettings> contentSettings,
        IOptions<ContentImagingSettings> imagingSettings,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IAbsoluteUrlBuilder absoluteUrlBuilder)
    {
        _imageUrlGenerator = imageUrlGenerator;
        _contentSettings = contentSettings.Value;
        _imagingSettings = imagingSettings.Value;
        _mediaUrlGenerators = mediaUrlGenerators;
        _absoluteUrlBuilder = absoluteUrlBuilder;
    }

    /// <inheritdoc />
    [Obsolete("Use the overload that accepts ImageResizeOptions instead. This method will be removed in v19.")]
    public IEnumerable<MediaUrlInfoResponseModel> CreateUrlSets(IEnumerable<IMedia> mediaItems, int height, int width, ImageCropMode? mode)
        => CreateUrlSets(mediaItems, new ImageResizeOptions(height, width, mode));

    /// <inheritdoc />
    public IEnumerable<MediaUrlInfoResponseModel> CreateUrlSets(IEnumerable<IMedia> mediaItems, ImageResizeOptions options)
        => mediaItems.Select(media => new MediaUrlInfoResponseModel(media.Key, CreateUrls(media, options))).ToArray();

    private IEnumerable<MediaUrlInfo> CreateUrls(IMedia media, ImageResizeOptions options)
    {
        IEnumerable<string> urls = media
            .GetUrls(_contentSettings, _mediaUrlGenerators)
            .WhereNotNull();

        return CreateThumbnailUrls(urls, options);
    }

    private IEnumerable<MediaUrlInfo> CreateThumbnailUrls(IEnumerable<string> urls, ImageResizeOptions options)
    {
        foreach (var url in urls)
        {
            var path = url.Split('?')[0];
            var extension = Path.GetExtension(path).TrimStart('.');
            if (_imageUrlGenerator.SupportedImageFileTypes.InvariantContains(extension) is false)
            {
                // It's okay to return just the image URL for SVGs, as they are always scalable.
                if (extension == "svg")
                {
                    yield return new MediaUrlInfo
                    {
                        Culture = null,
                        Url = _absoluteUrlBuilder.ToAbsoluteUrl(url).ToString(),
                    };
                }

                continue;
            }

            var imageOptions = new ImageUrlGenerationOptions(url)
            {
                Height = options.Height,
                Width = options.Width,
                ImageCropMode = options.Mode,
                Format = DetermineOutputFormat(url, options.Format),
            };

            var relativeUrl = _imageUrlGenerator.GetImageUrl(imageOptions);
            if (relativeUrl is null)
            {
                continue;
            }

            yield return new MediaUrlInfo
            {
                Culture = null,
                Url = _absoluteUrlBuilder.ToAbsoluteUrl(relativeUrl).ToString(),
            };
        }
    }

    /// <summary>
    /// Determines the appropriate output format for an image based on the source URL and requested format.
    /// </summary>
    /// <param name="imageUrl">The source image URL.</param>
    /// <param name="requestedFormat">The explicitly requested format (from API request), or null to use automatic determination.</param>
    /// <returns>The format to use for the output image, or null to keep the original format.</returns>
    /// <remarks>
    /// Format determination logic:
    /// 1. If a format is explicitly requested, use it
    /// 2. If the source file is not a native image format (e.g., PDF), convert to WebP
    /// 3. For native image formats (JPG, PNG, etc.), keep original format (null = no conversion)
    /// </remarks>
    private string? DetermineOutputFormat(string imageUrl, string? requestedFormat)
    {
        // If user explicitly requested a format, honor it
        if (!string.IsNullOrWhiteSpace(requestedFormat))
        {
            return requestedFormat;
        }

        // Extract extension from URL
        try
        {
            var uri = new Uri(imageUrl, UriKind.RelativeOrAbsolute);
            var path = uri.IsAbsoluteUri ? uri.LocalPath : imageUrl.Split('?')[0];
            var extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();

            if (string.IsNullOrEmpty(extension))
            {
                return null;
            }

            // Check if this is a native image format
            var isNativeImageFormat = _imagingSettings.ImageFileTypes
                .Any(format => format.Equals(extension, StringComparison.OrdinalIgnoreCase));

            // If not a native image format (e.g., PDF), convert to WebP
            // For native image formats, return null to keep original
            return isNativeImageFormat ? null : "webp";
        }
        catch (UriFormatException)
        {
            // If URL parsing fails, treat as a native image and don't convert
            return null;
        }
    }
}
