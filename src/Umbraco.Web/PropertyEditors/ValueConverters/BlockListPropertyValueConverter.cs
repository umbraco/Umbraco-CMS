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
        private readonly BlockListEditorDataConverter _blockListEditorDataConverter;

        public BlockListPropertyValueConverter(IProfilingLogger proflog, BlockEditorConverter blockConverter)
        {
            _proflog = proflog;
            _blockConverter = blockConverter;
            _blockListEditorDataConverter = new BlockListEditorDataConverter();
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
                var blockConfigMap = configuration.Blocks.ToDictionary(x => x.ContentElementTypeKey);
                var validSettingElementTypes = blockConfigMap.Values.Select(x => x.SettingsElementTypeKey).Where(x => x.HasValue).Distinct().ToList();

                var contentPublishedElements = new Dictionary<Guid, IPublishedElement>();
                var settingsPublishedElements = new Dictionary<Guid, IPublishedElement>();

                var layout = new List<BlockListLayoutReference>();

                var value = (string)inter;
                if (string.IsNullOrWhiteSpace(value)) return BlockListModel.Empty;

                var converted = _blockListEditorDataConverter.Deserialize(value);
                if (converted.BlockValue.ContentData.Count == 0) return BlockListModel.Empty;

                var blockListLayout = converted.Layout.ToObject<IEnumerable<BlockListLayoutItem>>();

                // convert the content data
                foreach (var data in converted.BlockValue.ContentData)
                {
                    if (!blockConfigMap.ContainsKey(data.ContentTypeKey)) continue;

                    var element = _blockConverter.ConvertToElement(data, referenceCacheLevel, preview);
                    if (element == null) continue;
                    contentPublishedElements[element.Key] = element;
                }
                // convert the settings data
                foreach (var data in converted.BlockValue.SettingsData)
                {
                    if (!validSettingElementTypes.Contains(data.ContentTypeKey)) continue;

                    var element = _blockConverter.ConvertToElement(data, referenceCacheLevel, preview);
                    if (element == null) continue;
                    settingsPublishedElements[element.Key] = element;
                }

                // if there's no elements just return since if there's no data it doesn't matter what is stored in layout
                if (contentPublishedElements.Count == 0) return BlockListModel.Empty;

                foreach (var layoutItem in blockListLayout)
                {
                    // get the content reference
                    var contentGuidUdi = (GuidUdi)layoutItem.ContentUdi;                    
                    if (!contentPublishedElements.TryGetValue(contentGuidUdi.Guid, out var contentData))
                        continue;

                    // get the setting reference
                    IPublishedElement settingsData = null;
                    var settingGuidUdi = layoutItem.SettingsUdi != null ? (GuidUdi)layoutItem.SettingsUdi : null;
                    if (settingGuidUdi != null)
                        settingsPublishedElements.TryGetValue(settingGuidUdi.Guid, out settingsData);

                    if (!contentData.ContentType.TryGetKey(out var contentTypeKey))
                        throw new InvalidOperationException("The content type was not of type " + typeof(IPublishedContentType2));

                    if (!blockConfigMap.TryGetValue(contentTypeKey, out var blockConfig))
                        continue;

                    // this can happen if they have a settings type, save content, remove the settings type, and display the front-end page before saving the content again
                    // we also ensure that the content type's match since maybe the settings type has been changed after this has been persisted.
                    if (settingsData != null)
                    {
                        if (!settingsData.ContentType.TryGetKey(out var settingsElementTypeKey))
                            throw new InvalidOperationException("The settings element type was not of type " + typeof(IPublishedContentType2));

                        if (!blockConfig.SettingsElementTypeKey.HasValue || settingsElementTypeKey != blockConfig.SettingsElementTypeKey)
                            settingsData = null;
                    }

                    var layoutRef = new BlockListLayoutReference(contentGuidUdi, contentData, settingGuidUdi, settingsData);
                    layout.Add(layoutRef);
                }

                var model = new BlockListModel(contentPublishedElements.Values, settingsPublishedElements.Values, layout);
                return model;
            }
        }


    }
}
