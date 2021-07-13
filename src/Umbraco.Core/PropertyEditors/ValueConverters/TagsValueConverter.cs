using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class TagsValueConverter : PropertyValueConverterBase
    {
        public TagsValueConverter()
        { }

        [Obsolete("This constructor isn't required anymore, because we don't need services to lookup the data type configuration.")]
        public TagsValueConverter(IDataTypeService dataTypeService)
        { }

        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.Tags);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(IEnumerable<string>);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            var sourceString = source?.ToString();
            if (string.IsNullOrEmpty(sourceString))
            {
                return Array.Empty<string>();
            }

            switch (propertyType.DataType.ConfigurationAs<TagConfiguration>().StorageType)
            {
                case TagsStorageType.Csv:
                    return sourceString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                case TagsStorageType.Json:
                    return JsonConvert.DeserializeObject<string[]>(sourceString);
                default:
                    throw new InvalidOperationException("Invalid storage type.");
            }
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
            => (string[])source;
    }
}
