using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for property editors with the bigint value type.
/// </summary>
[DefaultValueTypePropertyValueConverter]
public class BigintValueTypeConverter : ValueTypePropertyValueConverterBase
{
    /// <inheritdoc />
    protected override string[] SupportedValueTypes => new[] { ValueTypes.Bigint };

    /// <summary>
    ///     Initializes a new instance of the <see cref="BigintValueTypeConverter" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editor collection.</param>
    public BigintValueTypeConverter(PropertyEditorCollection propertyEditors)
        : base(propertyEditors)
    {
    }

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(long);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source.TryConvertTo<long>().Result;
}
