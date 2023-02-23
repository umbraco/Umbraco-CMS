using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.ContentApi;

public class ApiMediaBuilder : IApiMediaBuilder
{
    private readonly IPublishedContentNameProvider _publishedContentNameProvider;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IPublishedValueFallback _publishedValueFallback;
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;

    public ApiMediaBuilder(
        IPublishedContentNameProvider publishedContentNameProvider,
        IPublishedUrlProvider publishedUrlProvider,
        IPublishedValueFallback publishedValueFallback,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
    {
        _publishedContentNameProvider = publishedContentNameProvider;
        _publishedUrlProvider = publishedUrlProvider;
        _publishedValueFallback = publishedValueFallback;
        _outputExpansionStrategyAccessor = outputExpansionStrategyAccessor;
    }

    public IApiMedia Build(IPublishedContent media) =>
        new ApiMedia(
            media.Key,
            _publishedContentNameProvider.GetName(media),
            media.ContentType.Alias,
            _publishedUrlProvider.GetMediaUrl(media, UrlMode.Relative),
            Extension(media),
            Width(media),
            Height(media),
            CustomProperties(media));

    private string? Extension(IPublishedContent media)
        => media.Value<string>(_publishedValueFallback, Constants.Conventions.Media.Extension);

    private int? Width(IPublishedContent media)
        => media.Value<int?>(_publishedValueFallback, Constants.Conventions.Media.Width);

    private int? Height(IPublishedContent media)
        => media.Value<int?>(_publishedValueFallback, Constants.Conventions.Media.Height);

    private IDictionary<string, object?> CustomProperties(IPublishedContent media)
    {
        IPublishedProperty[] customProperties = media
            .Properties
            .Where(p => p.Alias.StartsWith("umbraco") == false)
            .ToArray();

        return customProperties.Any() &&
               _outputExpansionStrategyAccessor.TryGetValue(out IOutputExpansionStrategy? outputExpansionStrategy)
            ? outputExpansionStrategy.MapProperties(customProperties)
            : new Dictionary<string, object?>();
    }
}
