using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class CheckboxListValueConverter : PropertyValueConverterBase
    {
        private static readonly char[] Comma = { ',' };

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

            return sourceString.Split(Comma, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
        }
    }
}
