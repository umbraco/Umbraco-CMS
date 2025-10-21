using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Provides a property value converter for time only datetime property editors.
/// </summary>
/// <remarks>
/// This is one of four property value converters derived from <see cref="DateTimeValueConverterBase"/> and storing their
/// value as JSON with timezone information.
/// </remarks>
public class TimeOnlyValueConverter : DateTimeValueConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeOnlyValueConverter"/> class.
    /// </summary>
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
        => TimeOnly.FromDateTime(dateTimeDto.Date.DateTime);
}
