using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

public abstract class ApiContentBuilderBase<T>
    where T : IApiContent
{
    private readonly IApiContentNameProvider _apiContentNameProvider;
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;

    protected ApiContentBuilderBase(
        IApiContentNameProvider apiContentNameProvider,
        IApiContentRouteBuilder apiContentRouteBuilder,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor,
        IVariationContextAccessor variationContextAccessor)
    {
        _apiContentNameProvider = apiContentNameProvider;
        ApiContentRouteBuilder = apiContentRouteBuilder;
        _outputExpansionStrategyAccessor = outputExpansionStrategyAccessor;
        VariationContextAccessor = variationContextAccessor;
    }

    protected IApiContentRouteBuilder ApiContentRouteBuilder { get; }

    protected IVariationContextAccessor VariationContextAccessor { get; }

    protected abstract T Create(IPublishedContent content, string name, IApiContentRoute route, IDictionary<string, object?> properties);

    public virtual T? Build(IPublishedContent content)
    {
        IApiContentRoute? route = ApiContentRouteBuilder.Build(content);
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
            _apiContentNameProvider.GetName(content),
            route,
            properties);
    }
}
