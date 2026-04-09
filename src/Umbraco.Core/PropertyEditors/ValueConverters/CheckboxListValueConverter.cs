using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for checkbox list properties.
/// </summary>
[DefaultPropertyValueConverter]
public class CheckboxListValueConverter : PropertyValueConverterBase
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CheckboxListValueConverter" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public CheckboxListValueConverter(IJsonSerializer jsonSerializer) => _jsonSerializer = jsonSerializer;

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.CheckBoxList);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IEnumerable<string>);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object? source, bool preview)
    {
        var sourceString = source?.ToString() ?? string.Empty;

        if (string.IsNullOrEmpty(sourceString))
        {
            return Enumerable.Empty<string>();
        }

        return _jsonSerializer.Deserialize<string[]>(sourceString);
    }
}
