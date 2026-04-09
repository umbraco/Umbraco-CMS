using System.Xml.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for property editors with the XML value type.
/// </summary>
[DefaultValueTypePropertyValueConverter]
public class XmlValueTypeConverter : ValueTypePropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    /// <inheritdoc />
    protected override string[] SupportedValueTypes => new[] { ValueTypes.Xml };

    /// <summary>
    ///     Initializes a new instance of the <see cref="XmlValueTypeConverter" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editor collection.</param>
    public XmlValueTypeConverter(PropertyEditorCollection propertyEditors)
        : base(propertyEditors)
    {
    }

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(XDocument);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType)
        => GetPropertyCacheLevel(propertyType);

    /// <inheritdoc />
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(string);

    /// <inheritdoc />
    /// <remarks>
    ///     System.Text.Json does not appreciate serializing XDocument because of parent/child node references.
    ///     The raw XML is output as a string instead.
    /// </remarks>
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        => inter is XDocument xDocumentValue
            ? xDocumentValue.ToString(SaveOptions.DisableFormatting)
            : null;
}
