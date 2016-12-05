using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    public class TagsValueConverter: PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            if (UmbracoConfig.For.UmbracoSettings().Content.EnablePropertyValueConverters)
            {
                return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.TagsAlias);
            }
            return false;
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            // if Json storage type deserialzie and return as string array
            if (JsonStorageType(propertyType.DataTypeId))
            {
                var jArray = JsonConvert.DeserializeObject<JArray>(source.ToString());
                return jArray.ToObject<string[]>();
            }

            // Otherwise assume CSV storage type and return as string array
            var csvTags =
                source.ToString()
                    .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .ToArray();
            return csvTags;
        }

        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null)
            {
                return null;
            }
            return (string[]) source;
        }

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof(IEnumerable<string>);
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType, PropertyCacheValue cacheValue)
        {
            return PropertyCacheLevel.Content;
        }

        /// <summary>
        /// Discovers if the tags data type is storing its data in a Json format
        /// </summary>
        /// <param name="dataTypeId">
        /// The data type id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool JsonStorageType(int dataTypeId)
        {
            var dts = ApplicationContext.Current.Services.DataTypeService;
            var storageType =
                dts.GetPreValuesCollectionByDataTypeId(dataTypeId)
                    .PreValuesAsDictionary.FirstOrDefault(
                        x => string.Equals(x.Key, "storageType", StringComparison.InvariantCultureIgnoreCase)).Value;

            if (storageType.Value.InvariantEquals("Json"))
            {
                return true;
            }

            return false;
        }
    }
}
