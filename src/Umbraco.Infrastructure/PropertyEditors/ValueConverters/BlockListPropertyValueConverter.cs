// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.PropertyEditors.BlockListConfiguration;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public class BlockListPropertyValueConverter : BlockPropertyValueConverterBase<BlockListModel, BlockListItem, BlockListLayoutItem, BlockConfiguration>, IDeliveryApiPropertyValueConverter
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IProfilingLogger _proflog;
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
        : base(blockConverter)
    {
        _proflog = proflog;
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
        => source?.ToString();

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        // NOTE: The intermediate object is just a JSON string, we don't actually convert from source -> intermediate since source is always just a JSON string
        using (_proflog.DebugDuration<BlockListPropertyValueConverter>(
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
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IEnumerable<ApiBlockListModel>);

    /// <inheritdoc />
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        BlockListModel? model = ConvertIntermediateToBlockListModel(owner, propertyType, referenceCacheLevel, inter, preview);

        return new ApiBlockListModel(
            model != null
                ? model
                    .Select(item => new ApiBlockItem(
                        _apiElementBuilder.Build(item.Content),
                        item.Settings != null ? _apiElementBuilder.Build(item.Settings) : null))
                    .ToArray()
                : Array.Empty<ApiBlockItem>());
    }

    private BlockListModel? ConvertIntermediateToBlockListModel(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        // NOTE: The intermediate object is just a JSON string, we don't actually convert from source -> intermediate since source is always just a JSON string
        using (_proflog.DebugDuration<BlockListPropertyValueConverter>(
                   $"ConvertPropertyToBlockList ({propertyType.DataType.Id})"))
        {
            // Get configuration
            BlockListConfiguration? configuration = propertyType.DataType.ConfigurationAs<BlockListConfiguration>();
            if (configuration is null)
            {
                return null;
            }

            BlockListModel CreateEmptyModel() => BlockListModel.Empty;

            BlockListModel CreateModel(IList<BlockListItem> items) => new BlockListModel(items);

            BlockListModel blockModel = UnwrapBlockModel(referenceCacheLevel, inter, preview, configuration.Blocks, CreateEmptyModel, CreateModel);

            return blockModel;
        }
    }

    protected override BlockEditorDataConverter CreateBlockEditorDataConverter() => new BlockListEditorDataConverter();

    protected override BlockItemActivator<BlockListItem> CreateBlockItemActivator() => new BlockListItemActivator(BlockEditorConverter);

    private class BlockListItemActivator : BlockItemActivator<BlockListItem>
    {
        public BlockListItemActivator(BlockEditorConverter blockConverter) : base(blockConverter)
        {
        }

        protected override Type GenericItemType => typeof(BlockListItem<,>);
    }
}
