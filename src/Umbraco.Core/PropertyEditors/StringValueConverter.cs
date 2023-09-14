using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.Templates;

namespace Umbraco.Cms.Core.PropertyEditors;

[DefaultPropertyValueConverter]
public class StringValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private static readonly string[] PropertyTypeAliases =
    {
        Constants.PropertyEditors.Aliases.String
    };

    public override bool IsConverter(IPublishedPropertyType propertyType)
        => PropertyTypeAliases.Contains(propertyType.EditorAlias);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(string);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Snapshot;

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source == null)
        {
            return null;
        }

        return source.ToString();
    }

    public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview) =>

        // source should come from ConvertSource and be a string (or null) already
        inter ?? string.Empty;

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => GetPropertyValueType(propertyType);

    public object ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        => ConvertIntermediateToObject(owner, propertyType, referenceCacheLevel, inter, preview);
}
