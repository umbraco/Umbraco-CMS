using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for property editors with the time value type.
/// </summary>
[DefaultValueTypePropertyValueConverter]
public class TimeValueTypeConverter : ValueTypePropertyValueConverterBase
{
    /// <inheritdoc />
    protected override string[] SupportedValueTypes => new[] { ValueTypes.Time };

    /// <summary>
    ///     Initializes a new instance of the <see cref="TimeValueTypeConverter" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editor collection.</param>
    public TimeValueTypeConverter(PropertyEditorCollection propertyEditors)
        : base(propertyEditors)
    {
    }

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(TimeSpan);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source is DateTime dateTimeValue ? dateTimeValue.ToUniversalTime().TimeOfDay : null;
}
