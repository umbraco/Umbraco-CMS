using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ApiMediaUrlProvider : IApiMediaUrlProvider
{
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public ApiMediaUrlProvider(IPublishedUrlProvider publishedUrlProvider)
        => _publishedUrlProvider = publishedUrlProvider;

    public string GetUrl(IPublishedContent media)
    {
        if (media.ItemType != PublishedItemType.Media)
        {
            throw new ArgumentException("Media URLs can only be generated from Media items.", nameof(media));
        }

        return _publishedUrlProvider.GetMediaUrl(media);
    }
}
