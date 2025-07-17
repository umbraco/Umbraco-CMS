using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The date time with timezone property value converter.
/// </summary>
[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public class DateTimeWithTimeZoneValueConverter : PropertyValueConverterBase
{
    private readonly IJsonSerializer _jsonSerializer;

    public DateTimeWithTimeZoneValueConverter(IJsonSerializer jsonSerializer)
        => _jsonSerializer = jsonSerializer;

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(DateTimeOffset);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object ConvertSourceToIntermediate(
        IPublishedElement owner,
        IPublishedPropertyType propertyType,
        object? source,
        bool preview)
    {
        var sourceStr = source?.ToString();
        if (sourceStr is not null && _jsonSerializer.TryDeserialize(sourceStr, out DateTimeWithTimeZone? dateTimeWithTimezone))
        {
            return dateTimeWithTimezone.Date;
        }

        return DateTimeOffset.MinValue;
    }

    internal class DateTimeWithTimeZone
    {
        public required DateTimeOffset Date { get; set; }

        public required string TimeZone { get; set; }
    }
}
