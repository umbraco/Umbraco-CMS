using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.PropertyValueHandlers;
using Umbraco.Cms.Search.Core.PropertyValueHandlers.Collection;

namespace Umbraco.Cms.Search.Core.Extensions;

/// <summary>
/// Provides extension methods for <see cref="PropertyValueHandlerCollection"/>.
/// </summary>
public static class PropertyValueHandlerCollectionExtensions
{
    /// <summary>
    /// Finds the handler that applies to the given property type, preferring a custom handler over a built-in one.
    /// </summary>
    /// <param name="collection">The property value handler collection.</param>
    /// <param name="propertyType">The property type to find a handler for.</param>
    /// <returns>The applicable handler, or null if none can handle the property type.</returns>
    public static IPropertyValueHandler? GetPropertyValueHandler(this PropertyValueHandlerCollection collection, IPropertyType propertyType)
    {
        IPropertyValueHandler[] applicableHandlers = collection
            .Where(handler => handler.CanHandle(propertyType))
            .ToArray();

        // always prioritize custom value handlers over the built-in ones
        return applicableHandlers.FirstOrDefault(handler => handler is not ICorePropertyValueHandler)
                      ?? applicableHandlers.FirstOrDefault();
    }
}
