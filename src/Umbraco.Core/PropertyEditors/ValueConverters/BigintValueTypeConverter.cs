using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultValueTypePropertyValueConverter]
public class BigintValueTypeConverter : ValueTypePropertyValueConverterBase
{
    protected override string[] SupportedValueTypes => new[] { ValueTypes.Bigint };

    public BigintValueTypeConverter(PropertyEditorCollection propertyEditors)
        : base(propertyEditors)
    {
    }

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(long);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source.TryConvertTo<long>().Result;
}
