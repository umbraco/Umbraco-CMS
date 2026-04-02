// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for flexible dropdown list properties.
/// </summary>
[DefaultPropertyValueConverter]
public class FlexibleDropdownPropertyValueConverter : PropertyValueConverterBase
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlexibleDropdownPropertyValueConverter" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public FlexibleDropdownPropertyValueConverter(IJsonSerializer jsonSerializer) => _jsonSerializer = jsonSerializer;

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.DropDownListFlexible);

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source is null)
        {
            return Array.Empty<string>();
        }

        var sourceString = source.ToString();

        return string.IsNullOrWhiteSpace(sourceString)
            ? Array.Empty<string>()
            : _jsonSerializer.Deserialize<string[]>(source.ToString()!) ?? Array.Empty<string>();
    }

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        if (inter is null)
        {
            return null;
        }

        var multiple = propertyType.DataType.ConfigurationAs<DropDownFlexibleConfiguration>()!.Multiple;
        var selectedValues = (string[])inter;
        if (selectedValues.Length > 0)
        {
            return multiple
                ? selectedValues
                : selectedValues[0];
        }

        return multiple
            ? inter
            : string.Empty;
    }

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) =>
        propertyType.DataType.ConfigurationAs<DropDownFlexibleConfiguration>()!.Multiple
            ? typeof(IEnumerable<string>)
            : typeof(string);
}
