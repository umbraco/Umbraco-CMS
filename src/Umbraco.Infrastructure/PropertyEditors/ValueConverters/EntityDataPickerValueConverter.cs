using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Converts the value stored by an entity data picker property editor into a strongly-typed object
/// that can be used within Umbraco, such as an entity identifier or reference.
/// </summary>
/// <remarks>NOTE: this class is made public on purpose because all value converters should be public</remarks>
[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public sealed class EntityDataPickerValueConverter : PropertyValueConverterBase
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.PropertyEditors.ValueConverters.EntityDataPickerValueConverter"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer used to deserialize property values.</param>
    public EntityDataPickerValueConverter(IJsonSerializer jsonSerializer)
        => _jsonSerializer = jsonSerializer;

    /// <summary>
    /// Determines whether this value converter is applicable to the specified property type based on its editor alias.
    /// </summary>
    /// <param name="propertyType">The published property type to evaluate.</param>
    /// <returns><c>true</c> if the property type uses the Entity Data Picker editor; otherwise, <c>false</c>.</returns>
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => Constants.PropertyEditors.Aliases.EntityDataPicker.Equals(propertyType.EditorAlias);

    /// <summary>
    /// Returns the CLR type used to represent the value of the specified published property type
    /// when using the Entity Data Picker value converter.
    /// </summary>
    /// <param name="propertyType">The published property type for which to retrieve the value type.</param>
    /// <returns>The <see cref="Type"/> representing <see cref="EntityDataPickerValue"/>.</returns>
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(EntityDataPickerValue);

    /// <summary>
    /// Gets the cache level for the property, which is always <see cref="PropertyCacheLevel.Element"/> for this value converter.
    /// </summary>
    /// <param name="propertyType">The published property type.</param>
    /// <returns>The property cache level, always <see cref="PropertyCacheLevel.Element"/>.</returns
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <summary>
    /// Converts the source value for an entity data picker property to an intermediate <see cref="EntityDataPickerValue"/> object.
    /// </summary>
    /// <param name="owner">The <see cref="IPublishedElement"/> that owns the property.</param>
    /// <param name="propertyType">The <see cref="IPublishedPropertyType"/> metadata for the property.</param>
    /// <param name="source">The source value to convert, expected to be a JSON string representing entity data picker values.</param>
    /// <param name="preview">A value indicating whether the conversion is for preview mode.</param>
    /// <returns>
    /// An <see cref="EntityDataPickerValue"/> object representing the intermediate value, or <c>null</c> if the source cannot be converted.
    /// </returns>
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
        /// <summary>
        /// Gets or sets the collection of selected entity identifiers.
        /// Each identifier is represented as a string.
        /// </summary>
        public required string[] Ids { get; init; }
    }
}
