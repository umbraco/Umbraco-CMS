using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A no-operation implementation of <see cref="IOutputExpansionStrategy"/> that maps all properties without expansion.
/// </summary>
internal sealed class NoopOutputExpansionStrategy : IOutputExpansionStrategy
{
    /// <inheritdoc />
    public IDictionary<string, object?> MapElementProperties(IPublishedElement element)
        => MapProperties(element.Properties);

    /// <inheritdoc />
    public IDictionary<string, object?> MapContentProperties(IPublishedContent content)
        => MapProperties(content.Properties);

    /// <inheritdoc />
    public IDictionary<string, object?> MapMediaProperties(IPublishedContent media, bool skipUmbracoProperties = true)
        => MapProperties(media.Properties.Where(p => skipUmbracoProperties is false || p.Alias.StartsWith("umbraco") is false));

    private IDictionary<string, object?> MapProperties(IEnumerable<IPublishedProperty> properties)
        => properties.ToDictionary(p => p.Alias, p => p.GetDeliveryApiValue(false));
}
