using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for integer properties.
/// </summary>
[DefaultPropertyValueConverter]
public class IntegerValueConverter : PropertyValueConverterBase
{
    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => Constants.PropertyEditors.Aliases.Integer.Equals(propertyType.EditorAlias);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(int);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(
        IPublishedElement owner,
        IPublishedPropertyType propertyType,
        object? source,
        bool preview) =>
        source.TryConvertTo<int>().Result;
}
