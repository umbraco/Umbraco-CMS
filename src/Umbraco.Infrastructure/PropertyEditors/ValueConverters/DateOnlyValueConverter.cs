using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The DateOnly property value converter.
/// </summary>
public class DateOnlyValueConverter : DateTime2ValueConverterBase
{
    public DateOnlyValueConverter(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias == Constants.PropertyEditors.Aliases.DateOnly;

    /// <inheritdoc/>
    internal override object ConvertToObject(DateTime2Dto dateTimeDto)
        => DateOnly.FromDateTime(dateTimeDto.Date.UtcDateTime);

    /// <inheritdoc />
    protected override Type GetPropertyValueType() => typeof(DateOnly?);
}
