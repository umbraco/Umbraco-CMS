using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public class ApiContentBuilder : IApiContentBuilder
{
    private readonly IApiContentNameProvider _apiContentNameProvider;
    private readonly IApiUrlProvider _apiUrlProvider;
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;

    public ApiContentBuilder(IApiContentNameProvider apiContentNameProvider, IApiUrlProvider apiUrlProvider, IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
    {
        _apiContentNameProvider = apiContentNameProvider;
        _apiUrlProvider = apiUrlProvider;
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
            _apiContentNameProvider.GetName(content),
            content.ContentType.Alias,
            _apiUrlProvider.Url(content),
            properties);
    }
}
