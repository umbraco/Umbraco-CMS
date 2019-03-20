﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class CheckboxListValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.CheckBoxList);

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (IEnumerable<string>);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertIntermediateToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            var sourceString = source?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(sourceString))
                return Enumerable.Empty<string>();

            return JsonConvert.DeserializeObject<string[]>(sourceString);
        }
    }
}
