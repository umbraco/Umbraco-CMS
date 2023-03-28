using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public class ApiMediaBuilder : IApiMediaBuilder
{
    private readonly IApiContentNameProvider _apiContentNameProvider;
    private readonly IApiMediaUrlProvider _apiMediaUrlProvider;
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;

    public ApiMediaBuilder(
        IApiContentNameProvider apiContentNameProvider,
        IApiMediaUrlProvider apiMediaUrlProvider,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
    {
        _apiContentNameProvider = apiContentNameProvider;
        _apiMediaUrlProvider = apiMediaUrlProvider;
        _outputExpansionStrategyAccessor = outputExpansionStrategyAccessor;
    }

    public IApiMedia Build(IPublishedContent media) =>
        new ApiMedia(
            media.Key,
            _apiContentNameProvider.GetName(media),
            media.ContentType.Alias,
            _apiMediaUrlProvider.GetUrl(media),
            Properties(media));

    private IDictionary<string, object?> Properties(IPublishedContent media) =>
        _outputExpansionStrategyAccessor.TryGetValue(out IOutputExpansionStrategy? outputExpansionStrategy)
            ? outputExpansionStrategy.MapProperties(media.Properties)
            : new Dictionary<string, object?>();
}
