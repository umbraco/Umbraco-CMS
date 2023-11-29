using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ApiElementBuilder : IApiElementBuilder
{
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;

    public ApiElementBuilder(IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
    {
        _outputExpansionStrategyAccessor = outputExpansionStrategyAccessor;
    }

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
