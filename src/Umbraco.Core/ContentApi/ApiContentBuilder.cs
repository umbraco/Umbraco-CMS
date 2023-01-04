using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.ContentApi;

public class ApiContentBuilder : IApiContentBuilder
{
    private readonly IPropertyMapper _propertyMapper;
    private readonly IPublishedContentNameProvider _publishedContentNameProvider;
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public ApiContentBuilder(IPropertyMapper propertyMapper, IPublishedContentNameProvider publishedContentNameProvider, IPublishedUrlProvider publishedUrlProvider)
    {
        _propertyMapper = propertyMapper;
        _publishedContentNameProvider = publishedContentNameProvider;
        _publishedUrlProvider = publishedUrlProvider;
    }

    public IApiContent Build(IPublishedContent content, bool expand = true) => new ApiContent(
        content.Key,
        _publishedContentNameProvider.GetName(content),
        content.ContentType.Alias,
        Url(content),
        expand ? _propertyMapper.Map(content) : new Dictionary<string, object?>());

    private string Url(IPublishedContent content)
        => content.ItemType == PublishedItemType.Content
            ? _publishedUrlProvider.GetUrl(content, UrlMode.Relative)
            : _publishedUrlProvider.GetMediaUrl(content, UrlMode.Relative);
}
