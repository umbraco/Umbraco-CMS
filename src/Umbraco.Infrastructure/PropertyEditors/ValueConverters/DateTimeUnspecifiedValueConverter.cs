using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The DateTimeWithTimeZone property value converter.
/// </summary>
public class DateTimeUnspecifiedValueConverter : DateTime2ValueConverterBase
{
    public DateTimeUnspecifiedValueConverter(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias == Constants.PropertyEditors.Aliases.DateTimeUnspecified;

    /// <inheritdoc/>
    internal override object ConvertToObject(DateTime2Dto dateTimeDto)
        => DateTime.SpecifyKind(dateTimeDto.Date.UtcDateTime, DateTimeKind.Unspecified);

    /// <inheritdoc />
    protected override Type GetPropertyValueType() => typeof(DateTime?);
}
