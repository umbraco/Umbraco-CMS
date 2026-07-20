using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.PropertyValueHandlers;
using Umbraco.Cms.Search.Core.PropertyValueHandlers.Collection;

namespace Umbraco.Cms.Search.Core.Extensions;

internal static class PropertyValueHandlerCollectionExtensions
{
    public static IPropertyValueHandler? GetPropertyValueHandler(this PropertyValueHandlerCollection collection, IPropertyType propertyType)
    {
        IPropertyValueHandler[] applicableHandlers = collection
            .Where(handler => handler.CanHandle(propertyType.PropertyEditorAlias))
            .ToArray();

        // always prioritize custom value handlers over the built-in ones
        return applicableHandlers.FirstOrDefault(handler => handler is not ICorePropertyValueHandler)
                      ?? applicableHandlers.FirstOrDefault();
    }
}
