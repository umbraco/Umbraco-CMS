// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public class BlockListPropertyValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IProfilingLogger _proflog;
    private readonly BlockEditorConverter _blockConverter;
    private readonly IApiElementBuilder _apiElementBuilder;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly BlockListPropertyValueConstructorCache _constructorCache;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly BlockEditorVarianceHandler _blockEditorVarianceHandler;

    [Obsolete("Use the constructor that takes all parameters, scheduled for removal in V16")]
    public BlockListPropertyValueConverter(IProfilingLogger proflog, BlockEditorConverter blockConverter, IContentTypeService contentTypeService, IApiElementBuilder apiElementBuilder, IJsonSerializer jsonSerializer, BlockListPropertyValueConstructorCache constructorCache)
        : this(proflog, blockConverter, contentTypeService, apiElementBuilder, jsonSerializer, constructorCache, StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(), StaticServiceProvider.Instance.GetRequiredService<BlockEditorVarianceHandler>())
    {
    }

    public BlockListPropertyValueConverter(
        IProfilingLogger proflog,
        BlockEditorConverter blockConverter,
        IContentTypeService contentTypeService,
        IApiElementBuilder apiElementBuilder,
        IJsonSerializer jsonSerializer,
        BlockListPropertyValueConstructorCache constructorCache,
        IVariationContextAccessor variationContextAccessor,
        BlockEditorVarianceHandler blockEditorVarianceHandler)
    {
        _proflog = proflog;
        _blockConverter = blockConverter;
        _contentTypeService = contentTypeService;
        _apiElementBuilder = apiElementBuilder;
        _jsonSerializer = jsonSerializer;
        _constructorCache = constructorCache;
        _variationContextAccessor = variationContextAccessor;
        _blockEditorVarianceHandler = blockEditorVarianceHandler;
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
                ConfigurationEditor.ConfigurationAs<BlockListConfiguration>(propertyType.DataType.ConfigurationObject)?.Blocks.FirstOrDefault();

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
            ConfigurationEditor.ConfigurationAs<BlockListConfiguration>(dataType.ConfigurationObject);
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
        using (!_proflog.IsEnabled(Core.Logging.LogLevel.Debug) ? null : _proflog.DebugDuration<BlockListPropertyValueConverter>(
                   $"ConvertPropertyToBlockList ({propertyType.DataType.Id})"))
        {
            BlockListModel? blockListModel = ConvertIntermediateToBlockListModel(owner, propertyType, referenceCacheLevel, inter, preview);
            if (blockListModel == null)
            {
                return null;
            }

            return IsSingleBlockMode(propertyType.DataType) ? blockListModel.FirstOrDefault() : blockListModel;
        }
    }

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => GetPropertyCacheLevel(propertyType);

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

    /// <inheritdoc />
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(ApiBlockListModel);

    /// <inheritdoc />
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        BlockListModel? model = ConvertIntermediateToBlockListModel(owner, propertyType, referenceCacheLevel, inter, preview);

        return new ApiBlockListModel(
            model != null
                ? model.Select(item => item.CreateApiBlockItem(_apiElementBuilder)).ToArray()
                : Array.Empty<ApiBlockItem>());
    }

    private BlockListModel? ConvertIntermediateToBlockListModel(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        using (!_proflog.IsEnabled(LogLevel.Debug) ? null : _proflog.DebugDuration<BlockListPropertyValueConverter>(
                   $"ConvertPropertyToBlockList ({propertyType.DataType.Id})"))
        {
            // NOTE: this is to retain backwards compatability
            if (inter is null)
            {
                return BlockListModel.Empty;
            }

            // NOTE: The intermediate object is just a JSON string, we don't actually convert from source -> intermediate since source is always just a JSON string
            if (inter is not string intermediateBlockModelValue)
            {
                return null;
            }

            // Get configuration
            BlockListConfiguration? configuration = propertyType.DataType.ConfigurationAs<BlockListConfiguration>();
            if (configuration is null)
            {
                return null;
            }

            var creator = new BlockListPropertyValueCreator(_blockConverter, _variationContextAccessor, _blockEditorVarianceHandler, _jsonSerializer, _constructorCache);
            return creator.CreateBlockModel(owner, referenceCacheLevel, intermediateBlockModelValue, preview, configuration.Blocks);
        }
    }
}
