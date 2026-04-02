using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for property editors with the integer value type.
/// </summary>
[DefaultValueTypePropertyValueConverter]
public class IntegerValueTypeConverter : ValueTypePropertyValueConverterBase
{
    /// <inheritdoc />
    protected override string[] SupportedValueTypes => new[] { ValueTypes.Integer };

    /// <summary>
    ///     Initializes a new instance of the <see cref="IntegerValueTypeConverter" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editor collection.</param>
    public IntegerValueTypeConverter(PropertyEditorCollection propertyEditors)
        : base(propertyEditors)
    {
    }

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(int);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source.TryConvertTo<int>().Result;
}
