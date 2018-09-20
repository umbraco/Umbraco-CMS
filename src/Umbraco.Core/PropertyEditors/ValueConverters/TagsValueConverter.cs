using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
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
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.Tags);

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (IEnumerable<string>);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            // if Json storage type deserialzie and return as string array
            if (JsonStorageType(propertyType.DataType.Id))
            {
                var jArray = JsonConvert.DeserializeObject<JArray>(source.ToString());
                return jArray.ToObject<string[]>() ?? Array.Empty<string>();
            }

            // Otherwise assume CSV storage type and return as string array
            var sourceString = source?.ToString() ?? string.Empty;
            var csvTags = sourceString
                .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .ToArray();
            return csvTags;
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
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
            // fixme
            // GetPreValuesCollectionByDataTypeId is cached at repository level;
            // still, the collection is deep-cloned so this is kinda expensive,
            // better to cache here + trigger refresh in DataTypeCacheRefresher

            return Storages.GetOrAdd(dataTypeId, id =>
            {
                var configuration = _dataTypeService.GetDataType(id).ConfigurationAs<TagConfiguration>();
                return configuration.StorageType == TagsStorageType.Json;
            });
        }

        private static readonly ConcurrentDictionary<int, bool> Storages = new ConcurrentDictionary<int, bool>();

        internal static void ClearCaches()
        {
            Storages.Clear();
        }
    }
}
