using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Provides a property value converter for unspecified datetime property editors.
/// </summary>
/// <remarks>
/// This is one of four property value converters derived from <see cref="DateTimeValueConverterBase"/> and storing their
/// value as JSON with timezone information.
/// </remarks>
public class DateTimeUnspecifiedValueConverter : DateTimeValueConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeUnspecifiedValueConverter"/> class.
    /// </summary>
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
        => DateTime.SpecifyKind(dateTimeDto.Date.Date, DateTimeKind.Unspecified);
}
