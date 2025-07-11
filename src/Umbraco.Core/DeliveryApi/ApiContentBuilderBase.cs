using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

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
        IApiContentRoute? route = ApiContentRouteBuilder.Build(content);
        if (route is null)
        {
            return default;
        }

        IDictionary<string, object?> properties =
            _outputExpansionStrategyAccessor.TryGetValue(out IOutputExpansionStrategy? outputExpansionStrategy)
                ? outputExpansionStrategy.MapContentProperties(content)
                : new Dictionary<string, object?>();

        // If a segment is requested and all segmented properties are null, we consider the segment as not created or non-existing and return null.
        // This aligns the behaviour of the API when it comes to "Accept-Segment" and "Accept-Language" requests, so 404 is returned for both when
        // the segment or language is not created or does not exist.
        // It also aligns with what we show in the backoffice for whether a segment is "Published" or "Not created".
        // Requested languages that aren't created or don't exist will already have exited early in the route builder.
        if (IsSegmentRequested() && AreAllSegmentedPropertiesNull(content, properties))
        {
            return default;
        }

        return Create(
            content,
            _apiContentNameProvider.GetName(content),
            route,
            properties);
    }

    private bool IsSegmentRequested() => string.IsNullOrWhiteSpace(VariationContextAccessor.VariationContext?.Segment) is false;

    private static bool AreAllSegmentedPropertiesNull(
        IPublishedContent content,
        IDictionary<string, object?> properties)
    {
        IEnumerable<string> segmentedProperties = content.Properties
            .Where(x => x.PropertyType.Variations.HasFlag(Models.ContentVariation.Segment))
            .Select(x => x.Alias);
        return segmentedProperties.All(x => properties.ContainsKey(x) is false || properties[x] is null);
    }
}
