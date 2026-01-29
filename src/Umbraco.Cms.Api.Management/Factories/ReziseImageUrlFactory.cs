using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class ReziseImageUrlFactory : IReziseImageUrlFactory
{
    private readonly IImageUrlGenerator _imageUrlGenerator;
    private readonly ContentSettings _contentSettings;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IAbsoluteUrlBuilder _absoluteUrlBuilder;

    public ReziseImageUrlFactory(IImageUrlGenerator imageUrlGenerator, IOptions<ContentSettings> contentSettings, MediaUrlGeneratorCollection mediaUrlGenerators, IAbsoluteUrlBuilder absoluteUrlBuilder)
    {
        _imageUrlGenerator = imageUrlGenerator;
        _contentSettings = contentSettings.Value;
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
            // Have to remove first char here, as it always contains the "."
            var extension = Path.GetExtension(url).Remove(0, 1);
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
                FurtherOptions = string.IsNullOrWhiteSpace(options.Format) ? null : $"format={options.Format}",
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
}
