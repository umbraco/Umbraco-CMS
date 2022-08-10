// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter(typeof(JsonValueConverter))]
    public class BlockGridPropertyValueConverter : PropertyValueConverterBase
    {
        private readonly IProfilingLogger _proflog;
        private readonly BlockEditorConverter _blockConverter;
        private readonly BlockGridEditorDataConverter _blockGridEditorDataConverter;

        // Niels, Change: I would love if this could be general, so we don't need a specific one for each block property editor....
        public BlockGridPropertyValueConverter(IProfilingLogger proflog, BlockEditorConverter blockConverter, IJsonSerializer jsonSerializer)
        {
            _proflog = proflog;
            _blockConverter = blockConverter;
            _blockGridEditorDataConverter = new BlockGridEditorDataConverter(jsonSerializer);
        }

        /// <inheritdoc />
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.BlockGrid);

        /// <inheritdoc />
        public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => typeof(BlockGridModel);

        /// <inheritdoc />
        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        /// <inheritdoc />
        public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        {
            return source?.ToString();
        }

        /// <inheritdoc />
        public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        {
            // NOTE: The intermediate object is just a json string, we don't actually convert from source -> intermediate since source is always just a json string

            using (_proflog.DebugDuration<BlockGridPropertyValueConverter>($"ConvertPropertyToBlockGrid ({propertyType.DataType.Id})"))
            {
                var value = (string?)inter;

                // Short-circuit on empty values
                if (string.IsNullOrWhiteSpace(value))
                {
                    return BlockGridModel.Empty;
                }

                var converted = _blockGridEditorDataConverter.Deserialize(value);
                if (converted.BlockValue.ContentData.Count == 0)
                {
                    return BlockGridModel.Empty;
                }

                var blockGridLayout = converted.Layout?.ToObject<IEnumerable<BlockGridLayoutItem>>();
                if (blockGridLayout is null)
                {
                    return BlockGridModel.Empty;
                }

                // Get configuration
                var configuration = propertyType.DataType.ConfigurationAs<BlockGridConfiguration>();
                if (configuration is null)
                {
                    return null;
                }

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
                if (contentPublishedElements.Count == 0) return BlockGridModel.Empty;

                // Convert the settings data
                var settingsPublishedElements = new Dictionary<Guid, IPublishedElement>();
                foreach (var data in converted.BlockValue.SettingsData)
                {
                    if (!validSettingsElementTypes.Contains(data.ContentTypeKey)) continue;

                    var element = _blockConverter.ConvertToElement(data, referenceCacheLevel, preview);
                    if (element == null) continue;

                    settingsPublishedElements[element.Key] = element;
                }

                BlockGridItem? CreateItem(BlockGridLayoutItem layoutItem)
                {
                    // Get the content reference
                    var contentGuidUdi = (GuidUdi?)layoutItem.ContentUdi;
                    if (contentGuidUdi is null || !contentPublishedElements.TryGetValue(contentGuidUdi.Guid, out var contentData))
                    {
                        return null;
                    }

                    if (!blockConfigMap.TryGetValue(contentData.ContentType.Key, out var blockConfig))
                    {
                        return null;
                    }

                    // Get the setting reference
                    IPublishedElement? settingsData = null;
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
                    var settingsType = blockConfig.SettingsElementTypeKey.HasValue
                        ? _blockConverter.GetModelType(blockConfig.SettingsElementTypeKey.Value)
                        : typeof(IPublishedElement);

                    // TODO: This should be optimized/cached, as calling Activator.CreateInstance is slow
                    var gridItemType = typeof(BlockGridItem<,>).MakeGenericType(contentData.GetType(), settingsType);
                    var gridItem = (BlockGridItem?)Activator.CreateInstance(gridItemType, contentGuidUdi, contentData, settingGuidUdi, settingsData, layoutItem.RowSpan!, layoutItem.ColumnSpan!);

                    var blockConfigAreaMap = blockConfig.Areas.ToDictionary(area => area.Key, area => area.Alias!);
                    gridItem!.Areas = layoutItem.Areas.Select(area =>
                    {
                        if (!blockConfigAreaMap.TryGetValue(area.Key, out var alias))
                        {
                            return null;
                        }
                        //var alias = "TODO";
                        var items = area.Items.Select(CreateItem).WhereNotNull().ToList();
                        return new BlockGridArea(items, alias);
                    }).WhereNotNull().ToArray();

                    return gridItem;
                }

                var items = blockGridLayout.Select(CreateItem).WhereNotNull().ToList();
                var model = new BlockGridModel(items);
                return model;
            }
        }
    }
}
