using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public abstract class ApiContentBuilderBase<T>
    where T : IApiContent
{
    private readonly IApiContentNameProvider _apiContentNameProvider;
    private readonly IApiContentRouteBuilder _apiContentRouteBuilder;
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;

    protected ApiContentBuilderBase(IApiContentNameProvider apiContentNameProvider, IApiContentRouteBuilder apiContentRouteBuilder, IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
    {
        _apiContentNameProvider = apiContentNameProvider;
        _apiContentRouteBuilder = apiContentRouteBuilder;
        _outputExpansionStrategyAccessor = outputExpansionStrategyAccessor;
    }

    protected abstract T Create(IPublishedContent content, Guid id, string name, string contentType, IApiContentRoute route, IDictionary<string, object?> properties);

    public virtual T? Build(IPublishedContent content)
    {
        IApiContentRoute? route = _apiContentRouteBuilder.Build(content);
        if (route is null)
        {
            return default;
        }

        IDictionary<string, object?> properties =
            _outputExpansionStrategyAccessor.TryGetValue(out IOutputExpansionStrategy? outputExpansionStrategy)
                ? outputExpansionStrategy.MapContentProperties(content)
                : new Dictionary<string, object?>();

        return Create(
            content,
            content.Key,
            _apiContentNameProvider.GetName(content),
            content.ContentType.Alias,
            route,
            properties);
    }
}
