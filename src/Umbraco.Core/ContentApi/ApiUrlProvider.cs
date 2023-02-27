using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.ContentApi;

public class ApiUrlProvider : IApiUrlProvider
{
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IRequestStartNodeServiceAccessor _requestStartNodeServiceAccessor;

    public ApiUrlProvider(IPublishedUrlProvider publishedUrlProvider, IRequestStartNodeServiceAccessor requestStartNodeServiceAccessor)
    {
        _publishedUrlProvider = publishedUrlProvider;
        _requestStartNodeServiceAccessor = requestStartNodeServiceAccessor;
    }

    public string Url(IPublishedContent content)
        => content.ItemType switch
        {
            PublishedItemType.Content => ContentUrl(content),
            PublishedItemType.Media => MediaUrl(content),
            _ => throw new ArgumentException($"Unsupported {nameof(IPublishedContent.ItemType)}: {content.ItemType}", nameof(content))
        };

    private string MediaUrl(IPublishedContent content) => _publishedUrlProvider.GetMediaUrl(content, UrlMode.Relative);

    private string ContentUrl(IPublishedContent content)
    {
        if (_requestStartNodeServiceAccessor.TryGetValue(out IRequestStartNodeService? requestStartNodeService) == false)
        {
            throw new InvalidOperationException($"Could not obtain {nameof(IRequestStartNodeService)} from the current request context");
        }

        var contentUrl = _publishedUrlProvider.GetUrl(content, UrlMode.Relative);
        var requestStartNodePath = requestStartNodeService.GetRequestedStartNodePath();

        return requestStartNodePath.IsNullOrWhiteSpace()
            ? contentUrl
            : contentUrl.ReplaceFirst($"/{requestStartNodePath}/", "/");
    }
}
