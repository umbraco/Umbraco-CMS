// Copyright (c) Umbraco.
// See LICENSE for more details.

using Newtonsoft.Json;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter]
public class FlexibleDropdownPropertyValueConverter : PropertyValueConverterBase
{
    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.DropDownListFlexible);

    public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source == null)
        {
            return Array.Empty<string>();
        }

        return JsonConvert.DeserializeObject<string[]>(source.ToString()!) ?? Array.Empty<string>();
    }

    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        if (inter == null)
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
