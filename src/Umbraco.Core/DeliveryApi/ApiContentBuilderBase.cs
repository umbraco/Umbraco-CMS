using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

public abstract class ApiContentBuilderBase<T>
    where T : IApiContent
{
    private readonly IApiContentNameProvider _apiContentNameProvider;
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;

    [Obsolete("Please use the constructor that takes all parameters. Scheduled for removal in Umbraco 17.")]
    protected ApiContentBuilderBase(
        IApiContentNameProvider apiContentNameProvider,
        IApiContentRouteBuilder apiContentRouteBuilder,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
        : this(
              apiContentNameProvider,
              apiContentRouteBuilder,
              outputExpansionStrategyAccessor,
              StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>())
    {
    }

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
        IApiContentRoute? route = ApiContentRouteBuilder.Build(content, VariationContextAccessor.VariationContext?.Culture);
        if (route is null)
        {
            return default;
        }

        // If a segment is requested and no segmented properties have any values, we consider the segment as not created or non-existing and return null.
        // This aligns the behaviour of the API when it comes to "Accept-Segment" and "Accept-Language" requests, so 404 is returned for both when
        // the segment or language is not created or does not exist.
        // It also aligns with what we show in the backoffice for whether a segment is "Published" or "Not created".
        // Requested languages that aren't created or don't exist will already have exited early in the route builder.
        var segment = VariationContextAccessor.VariationContext?.Segment;
        if (segment.IsNullOrWhiteSpace() is false
            && content.ContentType.VariesBySegment()
            && content
                .Properties
                .Where(p => p.PropertyType.VariesBySegment())
                .All(p => p.HasValue(VariationContextAccessor.VariationContext?.Culture, segment) is false))
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
