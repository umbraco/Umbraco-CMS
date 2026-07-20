using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a strategy for mapping and expanding properties in Delivery API responses.
/// </summary>
public interface IOutputExpansionStrategy
{
    /// <summary>
    ///     Maps the properties of an element for the API response.
    /// </summary>
    /// <param name="element">The published element to map properties for.</param>
    /// <returns>A dictionary of property aliases to their values.</returns>
    IDictionary<string, object?> MapElementProperties(IPublishedElement element);

    /// <summary>
    ///     Maps the properties of content for the API response.
    /// </summary>
    /// <param name="content">The published content to map properties for.</param>
    /// <returns>A dictionary of property aliases to their values.</returns>
    IDictionary<string, object?> MapContentProperties(IPublishedContent content);

    /// <summary>
    ///     Maps the properties of media for the API response.
    /// </summary>
    /// <param name="media">The published media to map properties for.</param>
    /// <param name="skipUmbracoProperties">Whether to skip built-in Umbraco properties that start with "umbraco".</param>
    /// <returns>A dictionary of property aliases to their values.</returns>
    IDictionary<string, object?> MapMediaProperties(IPublishedContent media, bool skipUmbracoProperties = true);
}
