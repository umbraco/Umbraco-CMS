using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.DeliveryApi;

// NOTE: left unsealed on purpose so it is extendable.
public class ApiContentPathProvider : IApiContentPathProvider
{
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public ApiContentPathProvider(IPublishedUrlProvider publishedUrlProvider)
        => _publishedUrlProvider = publishedUrlProvider;

    public virtual string? GetContentPath(IPublishedContent content, string? culture)
        => _publishedUrlProvider.GetUrl(content, UrlMode.Relative, culture);
}
