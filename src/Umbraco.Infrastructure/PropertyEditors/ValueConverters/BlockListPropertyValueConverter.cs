// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Extensions;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public class BlockListPropertyValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IProfilingLogger _proflog;
    private readonly BlockEditorConverter _blockConverter;
    private readonly IApiElementBuilder _apiElementBuilder;

    [Obsolete("Use the constructor that takes all parameters, scheduled for removal in V14")]
    public BlockListPropertyValueConverter(IProfilingLogger proflog, BlockEditorConverter blockConverter)
        : this(proflog, blockConverter, StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>())
    {
    }

    [Obsolete("Use the constructor that takes all parameters, scheduled for removal in V14")]
    public BlockListPropertyValueConverter(IProfilingLogger proflog, BlockEditorConverter blockConverter, IContentTypeService contentTypeService)
        : this(proflog, blockConverter, contentTypeService, StaticServiceProvider.Instance.GetRequiredService<IApiElementBuilder>())
    {
    }

    public BlockListPropertyValueConverter(IProfilingLogger proflog, BlockEditorConverter blockConverter, IContentTypeService contentTypeService, IApiElementBuilder apiElementBuilder)
    {
        _proflog = proflog;
        _blockConverter = blockConverter;
        _contentTypeService = contentTypeService;
        _apiElementBuilder = apiElementBuilder;
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
        => ConvertSourceToBlockListModel(propertyType, source, preview);

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        if (inter is not BlockListModel blockListModel)
        {
            return null;
        }

        return IsSingleBlockMode(propertyType.DataType) ? blockListModel.FirstOrDefault() : blockListModel;
    }

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

    /// <inheritdoc />
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(ApiBlockListModel);

    /// <inheritdoc />
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        if (inter is not BlockListModel blockListModel)
        {
            return null;
        }

        return new ApiBlockListModel(blockListModel
            .Select(item => item.CreateApiBlockItem(_apiElementBuilder))
            .ToArray());
    }

    private BlockListModel? ConvertSourceToBlockListModel(IPublishedPropertyType propertyType, object? source, bool preview)
    {
        using (!_proflog.IsEnabled(LogLevel.Debug) ? null : _proflog.DebugDuration<BlockListPropertyValueConverter>(
                   $"ConvertPropertyToBlockList ({propertyType.DataType.Id})"))
        {
            // NOTE: this is to retain backwards compatability
            if (source is null)
            {
                return BlockListModel.Empty;
            }

            // NOTE: The intermediate object is just a JSON string, we don't actually convert from source -> intermediate since source is always just a JSON string
            if (source is not string intermediateBlockModelValue)
            {
                return null;
            }

            // Get configuration
            BlockListConfiguration? configuration = propertyType.DataType.ConfigurationAs<BlockListConfiguration>();
            if (configuration is null)
            {
                return null;
            }

            var creator = new BlockListPropertyValueCreator(_blockConverter);
            // the reference cache level is .Element here, as is also the case when rendering at property level.
            return creator.CreateBlockModel(PropertyCacheLevel.Element, intermediateBlockModelValue, preview, configuration.Blocks);
        }
    }
}
