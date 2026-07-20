using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Base class for building <see cref="IApiContent"/> instances from published content.
/// </summary>
/// <typeparam name="T">The type of API content to build.</typeparam>
public abstract class ApiContentBuilderBase<T>
    where T : IApiContent
{
    private readonly IApiContentNameProvider _apiContentNameProvider;
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiContentBuilderBase{T}"/> class.
    /// </summary>
    /// <param name="apiContentNameProvider">The API content name provider.</param>
    /// <param name="apiContentRouteBuilder">The API content route builder.</param>
    /// <param name="outputExpansionStrategyAccessor">The output expansion strategy accessor.</param>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
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

    /// <summary>
    ///     Gets the API content route builder.
    /// </summary>
    protected IApiContentRouteBuilder ApiContentRouteBuilder { get; }

    /// <summary>
    ///     Gets the variation context accessor.
    /// </summary>
    protected IVariationContextAccessor VariationContextAccessor { get; }

    /// <summary>
    ///     Creates an API content instance from the specified parameters.
    /// </summary>
    /// <param name="content">The published content.</param>
    /// <param name="name">The name of the content.</param>
    /// <param name="route">The route of the content.</param>
    /// <param name="properties">The properties of the content.</param>
    /// <returns>An API content instance.</returns>
    protected abstract T Create(IPublishedContent content, string name, IApiContentRoute route, IDictionary<string, object?> properties);

    /// <summary>
    ///     Builds an API content instance from the specified published content.
    /// </summary>
    /// <param name="content">The published content to build from.</param>
    /// <returns>An API content instance, or <c>null</c> if the content cannot be built.</returns>
    public virtual T? Build(IPublishedContent content)
    {
        IApiContentRoute? route = ApiContentRouteBuilder.Build(content, VariationContextAccessor.VariationContext?.Culture);
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
