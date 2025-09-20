using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The TimeOnly property value converter.
/// </summary>
public class TimeOnlyValueConverter : DateTime2ValueConverterBase
{
    public TimeOnlyValueConverter(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias == Constants.PropertyEditors.Aliases.TimeOnly;

    /// <inheritdoc/>
    internal override object ConvertToObject(DateTime2Dto dateTimeDto)
        => TimeOnly.FromDateTime(dateTimeDto.Date.UtcDateTime);

    /// <inheritdoc />
    protected override Type GetPropertyValueType() => typeof(TimeOnly?);
}
