// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter(typeof(JsonValueConverter))]
    public class BlockListPropertyValueConverter : PropertyValueConverterBase
    {
        private readonly IProfilingLogger _proflog;
        private readonly BlockEditorConverter _blockConverter;
        private readonly BlockListEditorDataConverter _blockListEditorDataConverter;
        private readonly ModelsBuilderSettings _modelsBuilderSettings;

        [Obsolete("Use ctor injecting ModelsBuilderSettings")]
        public BlockListPropertyValueConverter(IProfilingLogger proflog, BlockEditorConverter blockConverter)
            : this(proflog, blockConverter,StaticServiceProvider.Instance.GetRequiredService<IOptions<ModelsBuilderSettings>>())
        {
        }

        public BlockListPropertyValueConverter(IProfilingLogger proflog, BlockEditorConverter blockConverter, IOptions<ModelsBuilderSettings> modelsBuilderOptions)
        {
            _proflog = proflog;
            _blockConverter = blockConverter;
            _blockListEditorDataConverter = new BlockListEditorDataConverter();
            _modelsBuilderSettings = modelsBuilderOptions?.Value;
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
                var value = (string)inter;

                // Short-circuit on empty values
                if (string.IsNullOrWhiteSpace(value)) return BlockListModel.Empty;

                var converted = _blockListEditorDataConverter.Deserialize(value);
                if (converted.BlockValue.ContentData.Count == 0) return BlockListModel.Empty;

                var blockListLayout = converted.Layout.ToObject<IEnumerable<BlockListLayoutItem>>();

                // Get configuration
                var configuration = propertyType.DataType.ConfigurationAs<BlockListConfiguration>();
                var blockConfigMap = configuration.Blocks.ToDictionary(x => x.ContentElementTypeKey);
                var validSettingsElementTypes = blockConfigMap.Values.Select(x => x.SettingsElementTypeKey).Where(x => x.HasValue).Distinct().ToList();

                // Convert the content data
                var contentPublishedElements = new Dictionary<Guid, IPublishedElement>();
                foreach (var data in converted.BlockValue.ContentData)
                {
                    if (!blockConfigMap.ContainsKey(data.ContentTypeKey)) continue;

                    var element = _blockConverter.ConvertToElement(data, referenceCacheLevel, preview);
                    if (element == null) continue;

                    contentPublishedElements[element.Key] = element;
                }

                // If there are no content elements, it doesn't matter what is stored in layout
                if (contentPublishedElements.Count == 0) return BlockListModel.Empty;

                // Convert the settings data
                var settingsPublishedElements = new Dictionary<Guid, IPublishedElement>();
                foreach (var data in converted.BlockValue.SettingsData)
                {
                    if (!validSettingsElementTypes.Contains(data.ContentTypeKey)) continue;

                    var element = _blockConverter.ConvertToElement(data, referenceCacheLevel, preview);
                    if (element == null) continue;

                    settingsPublishedElements[element.Key] = element;
                }

                var layout = new List<BlockListItem>();
                foreach (var layoutItem in blockListLayout)
                {
                    // Get the content reference
                    var contentGuidUdi = (GuidUdi)layoutItem.ContentUdi;
                    if (!contentPublishedElements.TryGetValue(contentGuidUdi.Guid, out var contentData))
                        continue;

                    if (!blockConfigMap.TryGetValue(contentData.ContentType.Key, out var blockConfig))
                        continue;

                    // Get the setting reference
                    IPublishedElement settingsData = null;
                    var settingGuidUdi = layoutItem.SettingsUdi != null ? (GuidUdi)layoutItem.SettingsUdi : null;
                    if (settingGuidUdi != null)
                        settingsPublishedElements.TryGetValue(settingGuidUdi.Guid, out settingsData);

                    // This can happen if they have a settings type, save content, remove the settings type, and display the front-end page before saving the content again
                    // We also ensure that the content types match, since maybe the settings type has been changed after this has been persisted
                    if (settingsData != null && (!blockConfig.SettingsElementTypeKey.HasValue || settingsData.ContentType.Key != blockConfig.SettingsElementTypeKey))
                    {
                        settingsData = null;
                    }

                    // Get settings type from configuration
                    var settingsType = blockConfig.SettingsElementTypeKey.HasValue && _modelsBuilderSettings.ModelsMode != ModelsMode.Nothing
                        ? _blockConverter.GetModelType(blockConfig.SettingsElementTypeKey.Value)
                        : typeof(IPublishedElement);

                    // TODO: This should be optimized/cached, as calling Activator.CreateInstance is slow
                    var layoutType = typeof(BlockListItem<,>).MakeGenericType(contentData.GetType(), settingsType);
                    var layoutRef = (BlockListItem)Activator.CreateInstance(layoutType, contentGuidUdi, contentData, settingGuidUdi, settingsData);

                    layout.Add(layoutRef);
                }

                var model = new BlockListModel(layout);
                return model;
            }
        }


    }
}
