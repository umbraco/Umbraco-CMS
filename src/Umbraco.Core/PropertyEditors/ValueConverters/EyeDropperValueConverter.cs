using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for eye dropper color picker properties.
/// </summary>
[DefaultPropertyValueConverter]
public class EyeDropperValueConverter : PropertyValueConverterBase
{
    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.ColorPickerEyeDropper);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(string);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object? source, bool preview)
        => source?.ToString() ?? string.Empty;
}
