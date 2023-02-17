using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.ContentApi;

public class ApiContentBuilder : IApiContentBuilder
{
    private readonly IPublishedContentNameProvider _publishedContentNameProvider;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;

    public ApiContentBuilder(IPublishedContentNameProvider publishedContentNameProvider, IPublishedUrlProvider publishedUrlProvider, IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
    {
        _publishedContentNameProvider = publishedContentNameProvider;
        _publishedUrlProvider = publishedUrlProvider;
        _outputExpansionStrategyAccessor = outputExpansionStrategyAccessor;
    }

    public IApiContent Build(IPublishedContent content)
    {
        IDictionary<string, object?> properties =
            _outputExpansionStrategyAccessor.TryGetValue(out IOutputExpansionStrategy? outputExpansionStrategy)
                ? outputExpansionStrategy.MapContentProperties(content)
                : new Dictionary<string, object?>();

        return new ApiContent(
            content.Key,
            _publishedContentNameProvider.GetName(content),
            content.ContentType.Alias,
            Url(content),
            properties);
    }

    private string Url(IPublishedContent content)
        => content.ItemType == PublishedItemType.Content
            ? _publishedUrlProvider.GetUrl(content, UrlMode.Relative)
            : _publishedUrlProvider.GetMediaUrl(content, UrlMode.Relative);
}
