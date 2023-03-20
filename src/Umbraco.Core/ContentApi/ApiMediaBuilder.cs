using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.ContentApi;

public class ApiMediaBuilder : IApiMediaBuilder
{
    private readonly IApiContentNameProvider _apiContentNameProvider;
    private readonly IApiMediaUrlProvider _apiMediaUrlProvider;
    private readonly IPublishedValueFallback _publishedValueFallback;
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;

    public ApiMediaBuilder(
        IApiContentNameProvider apiContentNameProvider,
        IApiMediaUrlProvider apiMediaUrlProvider,
        IPublishedValueFallback publishedValueFallback,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
    {
        _apiContentNameProvider = apiContentNameProvider;
        _apiMediaUrlProvider = apiMediaUrlProvider;
        _publishedValueFallback = publishedValueFallback;
        _outputExpansionStrategyAccessor = outputExpansionStrategyAccessor;
    }

    public IApiMedia Build(IPublishedContent media) =>
        new ApiMedia(
            media.Key,
            _apiContentNameProvider.GetName(media),
            media.ContentType.Alias,
            _apiMediaUrlProvider.GetUrl(media),
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
