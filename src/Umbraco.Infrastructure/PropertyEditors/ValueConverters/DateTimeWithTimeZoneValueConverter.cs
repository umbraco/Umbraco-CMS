using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The DateTimeWithTimeZone property value converter.
/// </summary>
public class DateTimeWithTimeZoneValueConverter : DateTime2ValueConverterBase
{
    public DateTimeWithTimeZoneValueConverter(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias == Constants.PropertyEditors.Aliases.DateTimeWithTimeZone;

    /// <inheritdoc/>
    internal override object ConvertToObject(DateTime2Dto dateTimeDto)
        => dateTimeDto.Date;

    /// <inheritdoc />
    protected override Type GetPropertyValueType() => typeof(DateTimeOffset?);
}
