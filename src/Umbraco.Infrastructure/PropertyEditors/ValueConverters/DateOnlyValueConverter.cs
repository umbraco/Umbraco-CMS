using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Provides a property value converter for date only datetime property editors.
/// </summary>
/// <remarks>
/// This is one of four property value converters derived from <see cref="DateTimeValueConverterBase"/> and storing their
/// value as JSON with timezone information.
/// </remarks>
public class DateOnlyValueConverter : DateTimeValueConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateOnlyValueConverter"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The serializer used for JSON operations.</param>
    /// <param name="logger">The logger used for logging converter operations.</param>
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
        => DateOnly.FromDateTime(dateTimeDto.Date.DateTime);
}
