using System.Xml.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultValueTypePropertyValueConverter]
public class XmlValueTypeConverter : ValueTypePropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    protected override string[] SupportedValueTypes => new[] { ValueTypes.Xml };

    public XmlValueTypeConverter(PropertyEditorCollection propertyEditors)
        : base(propertyEditors)
    {
    }

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(XDocument);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source is not string stringValue)
        {
            return null;
        }

        try
        {
            return XDocument.Parse(stringValue);
        }
        catch
        {
            return null;
        }
    }

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType)
        => GetPropertyCacheLevel(propertyType);

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(string);

    // System.Text.Json does not appreciate serializing XDocument because of parent/child node references. Let's settle for outputting the raw XML as a string, then.
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        => inter is XDocument xDocumentValue
            ? xDocumentValue.ToString(SaveOptions.DisableFormatting)
            : null;
}
