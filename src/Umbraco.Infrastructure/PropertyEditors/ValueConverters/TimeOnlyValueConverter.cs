using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The TimeOnly property value converter.
/// </summary>
public class TimeOnlyValueConverter : DateTimeValueConverterBase
{
    public TimeOnlyValueConverter(IJsonSerializer jsonSerializer, ILogger<TimeOnlyValueConverter> logger)
        : base(jsonSerializer, logger)
    {
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias == Constants.PropertyEditors.Aliases.TimeOnly;

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => typeof(TimeOnly?);

    /// <inheritdoc/>
    protected override object ConvertToObject(DateTimeDto dateTimeDto)
        => TimeOnly.FromDateTime(dateTimeDto.Date.UtcDateTime);
}
