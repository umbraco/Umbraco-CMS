using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Blocks;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{

    [DefaultPropertyValueConverter(typeof(JsonValueConverter))]
    public class BlockListPropertyValueConverter : PropertyValueConverterBase
    {
        private readonly IProfilingLogger _proflog;
        private readonly IPublishedModelFactory _publishedModelFactory;
        private readonly BlockEditorConverter _blockConverter;

        public BlockListPropertyValueConverter(IProfilingLogger proflog, IPublishedModelFactory publishedModelFactory, BlockEditorConverter blockConverter)
        {
            _proflog = proflog;
            _publishedModelFactory = publishedModelFactory;
            _blockConverter = blockConverter;
        }

        /// <inheritdoc />
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.BlockList);

        /// <inheritdoc />
        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        {
            var contentTypes = propertyType.DataType.ConfigurationAs<BlockListConfiguration>().ElementTypes;
            return contentTypes.Length == 1
                ? typeof(IEnumerable<>).MakeGenericType(ModelType.For(contentTypes[0].Alias))
                : typeof(IEnumerable<IPublishedElement>);
        }

        /// <inheritdoc />
        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        /// <inheritdoc />
        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            return source?.ToString();
        }

        /// <inheritdoc />
        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            using (_proflog.DebugDuration<BlockListPropertyValueConverter>($"ConvertPropertyToBlockList ({propertyType.DataType.Id})"))
            {
                var configuration = propertyType.DataType.ConfigurationAs<BlockListConfiguration>();
                var contentTypes = configuration.ElementTypes;
                var elements = contentTypes.Length == 1
                    ? (IList<IPublishedElement>)_publishedModelFactory.CreateModelList(contentTypes[0].Alias)
                    : new List<IPublishedElement>();

                var layout = new List<BlockListLayoutReference>();
                var model = new BlockListModel(elements, layout);

                var value = (string)inter;
                if (string.IsNullOrWhiteSpace(value)) return model;

                var objects = JsonConvert.DeserializeObject<JObject>(value);
                if (objects.Count == 0) return model;

                var jsonLayout = objects["layout"] as JObject;
                if (jsonLayout == null) return model;

                var jsonData = objects["data"] as JArray;
                if (jsonData == null) return model;

                var blockListLayouts = jsonLayout[Constants.PropertyEditors.Aliases.BlockList] as JArray;
                if (blockListLayouts == null) return model;

                foreach(var blockListLayout in blockListLayouts)
                {
                    var settingsJson = blockListLayout["settings"] as JObject;
                    if (settingsJson == null) continue;

                    var element = _blockConverter.ConvertToElement(settingsJson, BlockEditorPropertyEditor.ContentTypeAliasPropertyKey, referenceCacheLevel, preview);
                    if (element == null) continue;

                    var layoutRef = new BlockListLayoutReference(blockListLayout.Value<Udi>("udi"), element);
                    layout.Add(layoutRef);
                }

                foreach (var data in jsonData.Cast<JObject>())
                {
                    var element = _blockConverter.ConvertToElement(data, BlockEditorPropertyEditor.ContentTypeAliasPropertyKey, referenceCacheLevel, preview);
                    if (element == null) continue;
                    elements.Add(element);
                }

                return model;
            }
        }

       
    }
}
