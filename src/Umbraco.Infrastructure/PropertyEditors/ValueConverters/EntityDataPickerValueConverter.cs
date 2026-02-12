using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

// NOTE: this class is made public on purpose because all value converters should be public
[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public sealed class EntityDataPickerValueConverter : PropertyValueConverterBase
{
    private readonly IJsonSerializer _jsonSerializer;

    public EntityDataPickerValueConverter(IJsonSerializer jsonSerializer)
        => _jsonSerializer = jsonSerializer;

    public override bool IsConverter(IPublishedPropertyType propertyType)
        => Constants.PropertyEditors.Aliases.EntityDataPicker.Equals(propertyType.EditorAlias);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(EntityDataPickerValue);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source is not string sourceString
            || propertyType.DataType.ConfigurationObject is not EntityDataPickerConfiguration dataTypeConfiguration)
        {
            return null;
        }

        EntityDataPickerDto? dto = _jsonSerializer.Deserialize<EntityDataPickerDto>(sourceString);
        return dto is not null
            ? new EntityDataPickerValue { Ids = dto.Ids, DataSource = dataTypeConfiguration.DataSource }
            : null;
    }

    private class EntityDataPickerDto
    {
        public required string[] Ids { get; init; }
    }
}
