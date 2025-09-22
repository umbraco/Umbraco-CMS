using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The DateTimeWithTimeZone property value converter.
/// </summary>
public class DateTimeUnspecifiedValueConverter : DateTimeValueConverterBase
{
    public DateTimeUnspecifiedValueConverter(IJsonSerializer jsonSerializer, ILogger<DateTimeUnspecifiedValueConverter> logger)
        : base(jsonSerializer, logger)
    {
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias == Constants.PropertyEditors.Aliases.DateTimeUnspecified;

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => typeof(DateTime?);

    /// <inheritdoc/>
    protected override object ConvertToObject(DateTimeDto dateTimeDto)
        => DateTime.SpecifyKind(dateTimeDto.Date.UtcDateTime, DateTimeKind.Unspecified);

}
