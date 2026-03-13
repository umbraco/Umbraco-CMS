using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods for generating URLs for media items within the Umbraco CMS.
/// </summary>
public class MediaUrlFactory : IMediaUrlFactory
{
    private readonly ContentSettings _contentSettings;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IAbsoluteUrlBuilder _absoluteUrlBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaUrlFactory"/> class with the specified content settings, media URL generators, and absolute URL builder.
    /// </summary>
    /// <param name="contentSettings">The options containing content settings used for media URL generation.</param>
    /// <param name="mediaUrlGenerators">A collection of <see cref="IMediaUrlGenerator"/> instances used to generate media URLs.</param>
    /// <param name="absoluteUrlBuilder">The service used to build absolute URLs for media items.</param>
    public MediaUrlFactory(
        IOptions<ContentSettings> contentSettings,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IAbsoluteUrlBuilder absoluteUrlBuilder)
    {
        _contentSettings = contentSettings.Value;
        _mediaUrlGenerators = mediaUrlGenerators;
        _absoluteUrlBuilder = absoluteUrlBuilder;
    }

    /// <summary>
    /// Generates a collection of <see cref="MediaUrlInfo"/> objects containing URLs for the specified media item.
    /// </summary>
    /// <param name="media">The <see cref="IMedia"/> item for which to generate URLs.</param>
    /// <returns>An <see cref="IEnumerable{MediaUrlInfo}"/> containing the URLs associated with the media item. The <c>Culture</c> property of each <see cref="MediaUrlInfo"/> is set to <c>null</c>. If the media is trashed, the URLs are generated accordingly.</returns>
    public IEnumerable<MediaUrlInfo> CreateUrls(IMedia media) =>
        media
            .GetUrls(_contentSettings, _mediaUrlGenerators)
            .WhereNotNull()
            .Select(mediaUrl => new MediaUrlInfo
            {
                Culture = null,
                Url = CreateMediaUrl(mediaUrl, media.Trashed),
            })
            .ToArray();

    private string CreateMediaUrl(string mediaUrl, bool isTrashed)
    {
        var url = _absoluteUrlBuilder.ToAbsoluteUrl(mediaUrl).ToString();

        return isTrashed && _contentSettings.EnableMediaRecycleBinProtection
            ? AddProtectedSuffixToMediaUrl(url)
            : url;
    }

    private static string AddProtectedSuffixToMediaUrl(string url) => Path.ChangeExtension(url, Constants.Conventions.Media.TrashedMediaSuffix + Path.GetExtension(url));

    /// <summary>
    /// Generates a collection of <see cref="MediaUrlInfoResponseModel"/> instances containing URL sets for the specified media items.
    /// </summary>
    /// <param name="mediaItems">The media items for which to generate URL sets.</param>
    /// <returns>An <see cref="IEnumerable{MediaUrlInfoResponseModel}"/> with URL information for each media item.</returns>
    public IEnumerable<MediaUrlInfoResponseModel> CreateUrlSets(IEnumerable<IMedia> mediaItems) =>
        mediaItems.Select(media => new MediaUrlInfoResponseModel(media.Key, CreateUrls(media))).ToArray();
}
