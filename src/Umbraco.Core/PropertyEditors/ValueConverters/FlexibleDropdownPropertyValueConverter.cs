// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter]
public class FlexibleDropdownPropertyValueConverter : PropertyValueConverterBase
{
    private readonly IJsonSerializer _jsonSerializer;

    public FlexibleDropdownPropertyValueConverter(IJsonSerializer jsonSerializer) => _jsonSerializer = jsonSerializer;

    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.DropDownListFlexible);

    public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
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

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) =>
        propertyType.DataType.ConfigurationAs<DropDownFlexibleConfiguration>()!.Multiple
            ? typeof(IEnumerable<string>)
            : typeof(string);
}
