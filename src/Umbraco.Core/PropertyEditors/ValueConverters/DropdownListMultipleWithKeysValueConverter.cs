using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    public class DropdownListMultipleWithKeysValueConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            if (UmbracoConfig.For.UmbracoSettings().Content.EnablePropertyValueConverters)
            {
                return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.DropdownlistMultiplePublishKeysAlias);
            }
            return false;
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
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

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof(IEnumerable<int>);
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType,
                                                        PropertyCacheValue cacheValue)
        {
            return PropertyCacheLevel.Content;
        }
    }
}
