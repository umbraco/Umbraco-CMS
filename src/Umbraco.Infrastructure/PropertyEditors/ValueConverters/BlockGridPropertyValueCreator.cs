﻿using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal sealed class BlockGridPropertyValueCreator : BlockPropertyValueCreatorBase<BlockGridModel, BlockGridItem, BlockGridLayoutItem, BlockGridConfiguration.BlockGridBlockConfiguration, BlockGridValue>
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly BlockGridPropertyValueConstructorCache _constructorCache;

    public BlockGridPropertyValueCreator(
        BlockEditorConverter blockEditorConverter,
        IVariationContextAccessor variationContextAccessor,
        BlockEditorVarianceHandler blockEditorVarianceHandler,
        IJsonSerializer jsonSerializer,
        BlockGridPropertyValueConstructorCache constructorCache)
        : base(blockEditorConverter, variationContextAccessor, blockEditorVarianceHandler)
    {
        _jsonSerializer = jsonSerializer;
        _constructorCache = constructorCache;
    }

    public BlockGridModel CreateBlockModel(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, string intermediateBlockModelValue, bool preview, BlockGridConfiguration.BlockGridBlockConfiguration[] blockConfigurations, int? gridColumns)
    {
        BlockGridModel CreateEmptyModel() => BlockGridModel.Empty;

        BlockGridModel CreateModel(IList<BlockGridItem> items) => new BlockGridModel(items, gridColumns);

        BlockGridItem? EnrichBlockItem(BlockGridItem blockItem, BlockGridLayoutItem layoutItem, BlockGridConfiguration.BlockGridBlockConfiguration blockConfig, CreateBlockItemModelFromLayout createBlockItem)
        {
            // enrich block item with additional configs + setup areas
            var blockConfigAreaMap = blockConfig.Areas.ToDictionary(area => area.Key);

            blockItem.RowSpan = layoutItem.RowSpan!.Value;
            blockItem.ColumnSpan = layoutItem.ColumnSpan!.Value;
            blockItem.AreaGridColumns = blockConfig.AreaGridColumns;
            blockItem.GridColumns = gridColumns;
            blockItem.Areas = layoutItem.Areas.Select(area =>
            {
                if (!blockConfigAreaMap.TryGetValue(area.Key, out BlockGridConfiguration.BlockGridAreaConfiguration? areaConfig))
                {
                    return null;
                }

                var items = area.Items.Select(item => createBlockItem(item)).WhereNotNull().ToList();
                return new BlockGridArea(items, areaConfig.Alias!, areaConfig.RowSpan!.Value, areaConfig.ColumnSpan!.Value);
            }).WhereNotNull().ToArray();

            return blockItem;
        }

        BlockGridModel blockModel = CreateBlockModel(
            owner,
            referenceCacheLevel,
            intermediateBlockModelValue,
            preview,
            blockConfigurations,
            CreateEmptyModel,
            CreateModel,
            EnrichBlockItem);

        return blockModel;
    }

    protected override BlockEditorDataConverter<BlockGridValue, BlockGridLayoutItem> CreateBlockEditorDataConverter() => new BlockGridEditorDataConverter(_jsonSerializer);

    protected override BlockItemActivator<BlockGridItem> CreateBlockItemActivator() => new BlockGridItemActivator(BlockEditorConverter, _constructorCache);

    private sealed class BlockGridItemActivator : BlockItemActivator<BlockGridItem>
    {
        public BlockGridItemActivator(BlockEditorConverter blockConverter, BlockGridPropertyValueConstructorCache constructorCache)
            : base(blockConverter, constructorCache)
        {
        }

        protected override Type GenericItemType => typeof(BlockGridItem<,>);
    }
}
