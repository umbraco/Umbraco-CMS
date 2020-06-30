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
        private readonly BlockEditorConverter _blockConverter;

        public BlockListPropertyValueConverter(IProfilingLogger proflog, BlockEditorConverter blockConverter)
        {
            _proflog = proflog;
            _blockConverter = blockConverter;
        }

        /// <inheritdoc />
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.BlockList);

        /// <inheritdoc />
        public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => typeof(BlockListModel);

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
            // NOTE: The intermediate object is just a json string, we don't actually convert from source -> intermediate since source is always just a json string

            using (_proflog.DebugDuration<BlockListPropertyValueConverter>($"ConvertPropertyToBlockList ({propertyType.DataType.Id})"))
            {
                var configuration = propertyType.DataType.ConfigurationAs<BlockListConfiguration>();
                var contentTypes = configuration.Blocks;
                var contentTypeMap = contentTypes.ToDictionary(x => x.Key);

                var elements = new Dictionary<Guid, IPublishedElement>();

                var layout = new List<BlockListLayoutReference>();
                var model = new BlockListModel(elements.Values, layout);

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

                // parse the data elements
                foreach (var data in jsonData.Cast<JObject>())
                {
                    var element = _blockConverter.ConvertToElement(data, BlockEditorPropertyEditor.ContentTypeKeyPropertyKey, referenceCacheLevel, preview);
                    if (element == null) continue;
                    elements[element.Key] = element;
                }

                // if there's no elements just return since if there's no data it doesn't matter what is stored in layout
                if (elements.Count == 0) return model;

                foreach (var blockListLayout in blockListLayouts)
                {
                    var settingsJson = blockListLayout["settings"] as JObject;
                    // the result of this can be null, that's ok
                    var element = settingsJson != null ? _blockConverter.ConvertToElement(settingsJson, BlockEditorPropertyEditor.ContentTypeKeyPropertyKey, referenceCacheLevel, preview) : null;

                    if (!Udi.TryParse(blockListLayout.Value<string>("udi"), out var udi) || !(udi is GuidUdi guidUdi))
                        continue;

                    // get the data reference
                    if (!elements.TryGetValue(guidUdi.Guid, out var data))
                        continue;

                    if (!data.ContentType.TryGetKey(out var contentTypeKey))
                        throw new InvalidOperationException("The content type was not of type " + typeof(IPublishedContentType2));

                    if (!contentTypeMap.TryGetValue(contentTypeKey, out var blockConfig))
                        continue;

                    // this can happen if they have a settings type, save content, remove the settings type, and display the front-end page before saving the content again
                    if (element != null && string.IsNullOrWhiteSpace(blockConfig.SettingsElementTypeKey))
                        element = null;

                    var layoutRef = new BlockListLayoutReference(udi, data, element);
                    layout.Add(layoutRef);
                }

                return model;
            }
        }


    }
}
