using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Default implementation of <see cref="IApiElementBuilder"/> that builds API element objects for the Delivery API.
/// </summary>
public sealed class ApiElementBuilder : IApiElementBuilder
{
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiElementBuilder"/> class.
    /// </summary>
    /// <param name="outputExpansionStrategyAccessor">The output expansion strategy accessor.</param>
    public ApiElementBuilder(IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
    {
        _outputExpansionStrategyAccessor = outputExpansionStrategyAccessor;
    }

    /// <inheritdoc />
    public IApiElement Build(IPublishedElement element)
    {
        IDictionary<string, object?> properties =
            _outputExpansionStrategyAccessor.TryGetValue(out IOutputExpansionStrategy? outputExpansionStrategy)
                ? outputExpansionStrategy.MapElementProperties(element)
                : new Dictionary<string, object?>();

        return new ApiElement(
            element.Key,
            element.ContentType.Alias,
            properties);
    }
}
