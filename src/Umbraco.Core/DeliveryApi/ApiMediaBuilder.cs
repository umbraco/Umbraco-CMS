using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ApiMediaBuilder : IApiMediaBuilder
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
            Bytes(media),
            Properties(media));

    private string? Extension(IPublishedContent media)
        => media.Value<string>(_publishedValueFallback, Constants.Conventions.Media.Extension);

    private int? Width(IPublishedContent media)
        => media.Value<int?>(_publishedValueFallback, Constants.Conventions.Media.Width);

    private int? Height(IPublishedContent media)
        => media.Value<int?>(_publishedValueFallback, Constants.Conventions.Media.Height);

    private int? Bytes(IPublishedContent media)
        => media.Value<int?>(_publishedValueFallback, Constants.Conventions.Media.Bytes);

    // map all media properties except the umbraco ones, as we've already included those in the output
    private IDictionary<string, object?> Properties(IPublishedContent media)
        => _outputExpansionStrategyAccessor.TryGetValue(out IOutputExpansionStrategy? outputExpansionStrategy)
            ? outputExpansionStrategy.MapMediaProperties(media)
            : new Dictionary<string, object?>();
}
