using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class DropdownListMultipleWithKeysValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.DropdownlistMultiplePublishKeys);

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (IEnumerable<int>);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null)
                return new int[] { };

            var prevalueIds = source.ToString()
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(p => p.Trim())
                                    .Select(int.Parse)
                                    .ToArray();

            return prevalueIds;
        }
    }
}
