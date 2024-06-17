// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter]
public class FlexibleDropdownPropertyValueConverter : PropertyValueConverterBase
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;

    // TODO KJA: constructor breakage
    public FlexibleDropdownPropertyValueConverter(IJsonSerializer jsonSerializer, IDataTypeConfigurationCache dataTypeConfigurationCache)
    {
        _jsonSerializer = jsonSerializer;
        _dataTypeConfigurationCache = dataTypeConfigurationCache;
    }

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

        var multiple = _dataTypeConfigurationCache.GetConfigurationAs<DropDownFlexibleConfiguration>(propertyType.DataType.Key)!.Multiple;
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
        _dataTypeConfigurationCache.GetConfigurationAs<DropDownFlexibleConfiguration>(propertyType.DataType.Key)!.Multiple
            ? typeof(IEnumerable<string>)
            : typeof(string);
}
