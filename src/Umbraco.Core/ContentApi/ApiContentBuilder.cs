using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public class ApiContentBuilder : IApiContentBuilder
{
    private readonly IApiContentNameProvider _apiContentNameProvider;
    private readonly IApiContentRouteBuilder _apiContentRouteBuilder;
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;

    public ApiContentBuilder(IApiContentNameProvider apiContentNameProvider, IApiContentRouteBuilder apiContentRouteBuilder, IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
    {
        _apiContentNameProvider = apiContentNameProvider;
        _apiContentRouteBuilder = apiContentRouteBuilder;
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
            _apiContentRouteBuilder.Build(content),
            properties);
    }
}
