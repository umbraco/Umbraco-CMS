using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a renderer that converts published property values for Delivery API responses.
/// </summary>
public interface IApiPropertyRenderer
{
    /// <summary>
    ///     Gets the rendered value of a property for the Delivery API response.
    /// </summary>
    /// <param name="property">The published property to render.</param>
    /// <param name="expanding">Whether the property value should be expanded.</param>
    /// <returns>The rendered property value.</returns>
    object? GetPropertyValue(IPublishedProperty property, bool expanding);
}
