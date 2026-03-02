using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class MediaUrlFactory : IMediaUrlFactory
{
    private readonly ContentSettings _contentSettings;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IAbsoluteUrlBuilder _absoluteUrlBuilder;

    public MediaUrlFactory(
        IOptions<ContentSettings> contentSettings,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IAbsoluteUrlBuilder absoluteUrlBuilder)
    {
        _contentSettings = contentSettings.Value;
        _mediaUrlGenerators = mediaUrlGenerators;
        _absoluteUrlBuilder = absoluteUrlBuilder;
    }

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

    public IEnumerable<MediaUrlInfoResponseModel> CreateUrlSets(IEnumerable<IMedia> mediaItems) =>
        mediaItems.Select(media => new MediaUrlInfoResponseModel(media.Key, CreateUrls(media))).ToArray();
}
