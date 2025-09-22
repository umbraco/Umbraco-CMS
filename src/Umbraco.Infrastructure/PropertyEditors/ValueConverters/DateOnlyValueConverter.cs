using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The DateOnly property value converter.
/// </summary>
public class DateOnlyValueConverter : DateTimeValueConverterBase
{
    public DateOnlyValueConverter(IJsonSerializer jsonSerializer, ILogger<DateOnlyValueConverter> logger)
        : base(jsonSerializer, logger)
    {
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias == Constants.PropertyEditors.Aliases.DateOnly;

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => typeof(DateOnly?);

    /// <inheritdoc/>
    protected override object ConvertToObject(DateTimeDto dateTimeDto)
        => DateOnly.FromDateTime(dateTimeDto.Date.UtcDateTime);
}
