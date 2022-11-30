using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.Headless;

public class HeadlessContentBuilder : IHeadlessContentBuilder
{
    private readonly IHeadlessPropertyMapper _propertyMapper;
    private readonly IHeadlessContentNameProvider _nameProvider;
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public HeadlessContentBuilder(IHeadlessPropertyMapper propertyMapper, IHeadlessContentNameProvider nameProvider, IPublishedUrlProvider publishedUrlProvider)
    {
        _propertyMapper = propertyMapper;
        _nameProvider = nameProvider;
        _publishedUrlProvider = publishedUrlProvider;
    }

    public IHeadlessContent Build(IPublishedContent content) => new HeadlessContent(content.Key,
        _nameProvider.GetName(content),
        content.ContentType.Alias, Url(content), _propertyMapper.Map(content));

    private string Url(IPublishedContent content)
        => content.ItemType == PublishedItemType.Content
            ? _publishedUrlProvider.GetUrl(content, UrlMode.Relative)
            : _publishedUrlProvider.GetMediaUrl(content, UrlMode.Relative);
}
