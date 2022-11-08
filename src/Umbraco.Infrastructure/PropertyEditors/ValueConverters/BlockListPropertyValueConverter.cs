// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public class BlockListPropertyValueConverter : PropertyValueConverterBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly BlockEditorConverter _blockConverter;
    private readonly BlockListEditorDataConverter _blockListEditorDataConverter;
    private readonly IProfilingLogger _proflog;

    [Obsolete("Use the constructor with the IContentTypeService")]
    public BlockListPropertyValueConverter(IProfilingLogger proflog, BlockEditorConverter blockConverter) : this(proflog, blockConverter, StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>()) { }

    public BlockListPropertyValueConverter(IProfilingLogger proflog, BlockEditorConverter blockConverter, IContentTypeService contentTypeService)
    {
        _proflog = proflog;
        _blockConverter = blockConverter;
        _blockListEditorDataConverter = new BlockListEditorDataConverter();
        _contentTypeService = contentTypeService;
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.BlockList);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
    {
        var isSingleBlockMode = IsSingleBlockMode(propertyType.DataType);
        if (isSingleBlockMode)
        {
            BlockListConfiguration.BlockConfiguration? block =
                ConfigurationEditor.ConfigurationAs<BlockListConfiguration>(propertyType.DataType.Configuration)?.Blocks.FirstOrDefault();

            ModelType? contentElementType = block?.ContentElementTypeKey is Guid contentElementTypeKey && _contentTypeService.Get(contentElementTypeKey) is IContentType contentType ? ModelType.For(contentType.Alias) : null;
            ModelType? settingsElementType = block?.SettingsElementTypeKey is Guid settingsElementTypeKey && _contentTypeService.Get(settingsElementTypeKey) is IContentType settingsType ? ModelType.For(settingsType.Alias) : null;

            if (contentElementType is not null)
            {
                if (settingsElementType is not null)
                {
                    return typeof(BlockListItem<,>).MakeGenericType(contentElementType, settingsElementType);
                }

                return typeof(BlockListItem<>).MakeGenericType(contentElementType);
            }

            return typeof(BlockListItem);
        }

        return typeof(BlockListModel);
    }

    private bool IsSingleBlockMode(PublishedDataType dataType)
    {
        BlockListConfiguration? config =
            ConfigurationEditor.ConfigurationAs<BlockListConfiguration>(dataType.Configuration);
        return (config?.UseSingleBlockMode ?? false) && config?.Blocks.Length == 1 && config?.ValidationLimit?.Min == 1 && config?.ValidationLimit?.Max == 1;
    }

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source?.ToString();

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        // NOTE: The intermediate object is just a JSON string, we don't actually convert from source -> intermediate since source is always just a JSON string
        using (_proflog.DebugDuration<BlockListPropertyValueConverter>(
                   $"ConvertPropertyToBlockList ({propertyType.DataType.Id})"))
        {
            var value = (string?)inter;

            var defaultValue = IsSingleBlockMode(propertyType.DataType) ? null : BlockListModel.Empty;

            // Short-circuit on empty values
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            BlockEditorData converted = _blockListEditorDataConverter.Deserialize(value);
            if (converted.BlockValue.ContentData.Count == 0)
            {
                return defaultValue;
            }

            IEnumerable<BlockListLayoutItem>? blockListLayout =
                converted.Layout?.ToObject<IEnumerable<BlockListLayoutItem>>();
            if (blockListLayout is null)
            {
                return defaultValue;
            }

            // Get configuration
            BlockListConfiguration? configuration = propertyType.DataType.ConfigurationAs<BlockListConfiguration>();
            if (configuration is null)
            {
                return null;
            }

            var blockConfigMap = configuration.Blocks.ToDictionary(x => x.ContentElementTypeKey);

            // Convert the content data
            var contentPublishedElements = new Dictionary<Guid, IPublishedElement>();
            foreach (BlockItemData data in converted.BlockValue.ContentData)
            {
                if (!blockConfigMap.ContainsKey(data.ContentTypeKey))
                {
                    continue;
                }

                IPublishedElement? element = _blockConverter.ConvertToElement(data, referenceCacheLevel, preview);
                if (element == null)
                {
                    continue;
                }

                contentPublishedElements[element.Key] = element;
            }

            // If there are no content elements, it doesn't matter what is stored in layout
            if (contentPublishedElements.Count == 0)
            {
                return defaultValue;
            }

            // Convert the settings data
            var settingsPublishedElements = new Dictionary<Guid, IPublishedElement>();
            var validSettingsElementTypes = blockConfigMap.Values.Select(x => x.SettingsElementTypeKey)
                .Where(x => x.HasValue).Distinct().ToList();
            foreach (BlockItemData data in converted.BlockValue.SettingsData)
            {
                if (!validSettingsElementTypes.Contains(data.ContentTypeKey))
                {
                    continue;
                }

                IPublishedElement? element = _blockConverter.ConvertToElement(data, referenceCacheLevel, preview);
                if (element is null)
                {
                    continue;
                }

                settingsPublishedElements[element.Key] = element;
            }

            // Cache constructors locally (it's tied to the current IPublishedSnapshot and IPublishedModelFactory)
            var blockListItemActivator = new BlockListItemActivator(_blockConverter);

            var list = new List<BlockListItem>();
            foreach (BlockListLayoutItem layoutItem in blockListLayout)
            {
                // Get the content reference
                var contentGuidUdi = (GuidUdi?)layoutItem.ContentUdi;
                if (contentGuidUdi is null ||
                    !contentPublishedElements.TryGetValue(contentGuidUdi.Guid, out IPublishedElement? contentData))
                {
                    continue;
                }

                if (!blockConfigMap.TryGetValue(
                    contentData.ContentType.Key,
                    out BlockListConfiguration.BlockConfiguration? blockConfig))
                {
                    continue;
                }

                // Get the setting reference
                IPublishedElement? settingsData = null;
                var settingGuidUdi = (GuidUdi?)layoutItem.SettingsUdi;
                if (settingGuidUdi is not null)
                {
                    settingsPublishedElements.TryGetValue(settingGuidUdi.Guid, out settingsData);
                }

                // This can happen if they have a settings type, save content, remove the settings type, and display the front-end page before saving the content again
                // We also ensure that the content types match, since maybe the settings type has been changed after this has been persisted
                if (settingsData is not null && (!blockConfig.SettingsElementTypeKey.HasValue ||
                                                 settingsData.ContentType.Key != blockConfig.SettingsElementTypeKey))
                {
                    settingsData = null;
                }

                // Create instance (use content/settings type from configuration)
                BlockListItem layoutRef = blockListItemActivator.CreateInstance(blockConfig.ContentElementTypeKey, blockConfig.SettingsElementTypeKey, contentGuidUdi, contentData, settingGuidUdi, settingsData);

                list.Add(layoutRef);
            }

            return IsSingleBlockMode(propertyType.DataType) ? list.FirstOrDefault() : new BlockListModel(list);
        }
    }

    private class BlockListItemActivator
    {
        private readonly BlockEditorConverter _blockConverter;

        private readonly
            Dictionary<(Guid, Guid?), Func<Udi, IPublishedElement, Udi?, IPublishedElement?, BlockListItem>>
            _contructorCache = new();

        public BlockListItemActivator(BlockEditorConverter blockConverter)
            => _blockConverter = blockConverter;

        public BlockListItem CreateInstance(Guid contentTypeKey, Guid? settingsTypeKey, Udi contentUdi, IPublishedElement contentData, Udi? settingsUdi, IPublishedElement? settingsData)
        {
            if (!_contructorCache.TryGetValue(
                (contentTypeKey, settingsTypeKey),
                out Func<Udi, IPublishedElement, Udi?, IPublishedElement?, BlockListItem>? constructor))
            {
                constructor = _contructorCache[(contentTypeKey, settingsTypeKey)] =
                    EmitConstructor(contentTypeKey, settingsTypeKey);
            }

            return constructor(contentUdi, contentData, settingsUdi, settingsData);
        }

        private Func<Udi, IPublishedElement, Udi?, IPublishedElement?, BlockListItem> EmitConstructor(
            Guid contentTypeKey, Guid? settingsTypeKey)
        {
            Type contentType = _blockConverter.GetModelType(contentTypeKey);
            Type settingsType = settingsTypeKey.HasValue
                ? _blockConverter.GetModelType(settingsTypeKey.Value)
                : typeof(IPublishedElement);
            Type type = typeof(BlockListItem<,>).MakeGenericType(contentType, settingsType);

            ConstructorInfo? constructor =
                type.GetConstructor(new[] { typeof(Udi), contentType, typeof(Udi), settingsType });
            if (constructor == null)
            {
                throw new InvalidOperationException($"Could not find the required public constructor on {type}.");
            }

            // We use unsafe here, because we know the contructor parameter count and types match
            return ReflectionUtilities
                .EmitConstructorUnsafe<Func<Udi, IPublishedElement, Udi?, IPublishedElement?, BlockListItem>>(
                    constructor);
        }
    }
}
