using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The DateTimeWithTimeZone property value converter.
/// </summary>
public class DateTimeWithTimeZoneValueConverter : DateTimeValueConverterBase
{
    public DateTimeWithTimeZoneValueConverter(IJsonSerializer jsonSerializer, ILogger<DateTimeWithTimeZoneValueConverter> logger)
        : base(jsonSerializer, logger)
    {
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias == Constants.PropertyEditors.Aliases.DateTimeWithTimeZone;

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => typeof(DateTimeOffset?);

    /// <inheritdoc/>
    protected override object ConvertToObject(DateTimeDto dateTimeDto)
        => dateTimeDto.Date;
}
