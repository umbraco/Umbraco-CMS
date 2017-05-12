using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class TagsValueConverter : PropertyValueConverterBase
    {
        private readonly IDataTypeService _dataTypeService;

        public TagsValueConverter(IDataTypeService dataTypeService)
        {
            _dataTypeService = dataTypeService ?? throw new ArgumentNullException(nameof(dataTypeService));
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.TagsAlias);
        }

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof(IEnumerable<string>);
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
        {
            return PropertyCacheLevel.Content;
        }

        public override object ConvertSourceToInter(PublishedPropertyType propertyType, object source, bool preview)
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

        public override object ConvertInterToObject(PublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            return (string[]) source;
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
        private bool JsonStorageType(int dataTypeId)
        {
            // GetPreValuesCollectionByDataTypeId is cached at repository level;
            // still, the collection is deep-cloned so this is kinda expensive,
            // better to cache here + trigger refresh in DataTypeCacheRefresher

            return Storages.GetOrAdd(dataTypeId, id =>
            {
                var preValue = _dataTypeService.GetPreValuesCollectionByDataTypeId(id)
                    .PreValuesAsDictionary
                    .FirstOrDefault(x => string.Equals(x.Key, "storageType", StringComparison.InvariantCultureIgnoreCase))
                    .Value;

                return preValue != null && preValue.Value.InvariantEquals("json");
            });
        }

        private static readonly ConcurrentDictionary<int, bool> Storages = new ConcurrentDictionary<int, bool>();

        internal static void ClearCaches()
        {
            Storages.Clear();
        }
    }
}
