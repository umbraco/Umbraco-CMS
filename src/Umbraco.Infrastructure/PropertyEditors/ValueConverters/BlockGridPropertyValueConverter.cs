// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.PropertyEditors.BlockGridConfiguration;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter(typeof(JsonValueConverter))]
    public class BlockGridPropertyValueConverter : BlockPropertyValueConverterBase<BlockGridModel, BlockGridItem, BlockGridLayoutItem, BlockGridBlockConfiguration>
    {
        private readonly IProfilingLogger _proflog;
        private readonly IJsonSerializer _jsonSerializer;

        // Niels, Change: I would love if this could be general, so we don't need a specific one for each block property editor....
        public BlockGridPropertyValueConverter(IProfilingLogger proflog, BlockEditorConverter blockConverter, IJsonSerializer jsonSerializer) : base(blockConverter)
        {
            _proflog = proflog;
            _jsonSerializer = jsonSerializer;
        }

        /// <inheritdoc />
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.BlockGrid);

        public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        {
            using (_proflog.DebugDuration<BlockGridPropertyValueConverter>($"ConvertPropertyToBlockGrid ({propertyType.DataType.Id})"))
            {
                // Get configuration
                var configuration = propertyType.DataType.ConfigurationAs<BlockGridConfiguration>();
                if (configuration is null)
                {
                    return null;
                }

                BlockGridModel CreateEmptyModel() => BlockGridModel.Empty;

                BlockGridModel CreateModel(IList<BlockGridItem> items) => new BlockGridModel(items, configuration.GridColumns);

                BlockGridItem? EnrichBlockItem(BlockGridItem blockItem, BlockGridLayoutItem layoutItem, BlockGridBlockConfiguration blockConfig, CreateBlockItemModelFromLayout createBlockItem)
                {
                    // enrich block item with additional configs + setup areas
                    var blockConfigAreaMap = blockConfig.Areas.ToDictionary(area => area.Key);

                    blockItem.RowSpan = layoutItem.RowSpan!.Value;
                    blockItem.ColumnSpan = layoutItem.ColumnSpan!.Value;
                    blockItem.ForceLeft = layoutItem.ForceLeft;
                    blockItem.ForceRight = layoutItem.ForceRight;
                    blockItem.AreaGridColumns = blockConfig.AreaGridColumns;
                    blockItem.GridColumns = configuration.GridColumns;
                    blockItem.Areas = layoutItem.Areas.Select(area =>
                    {
                        if (!blockConfigAreaMap.TryGetValue(area.Key, out BlockGridAreaConfiguration? areaConfig))
                        {
                            return null;
                        }

                        var items = area.Items.Select(item => createBlockItem(item)).WhereNotNull().ToList();
                        return new BlockGridArea(items, areaConfig.Alias!, areaConfig.RowSpan!.Value, areaConfig.ColumnSpan!.Value);
                    }).WhereNotNull().ToArray();

                    return blockItem;
                }

                BlockGridModel blockModel = UnwrapBlockModel(
                    referenceCacheLevel,
                    inter,
                    preview,
                    configuration.Blocks,
                    CreateEmptyModel,
                    CreateModel,
                    EnrichBlockItem
                );

                return blockModel;
            }
        }

        protected override BlockEditorDataConverter CreateBlockEditorDataConverter() => new BlockGridEditorDataConverter(_jsonSerializer);

        protected override BlockItemActivator<BlockGridItem> CreateBlockItemActivator() => new BlockGridItemActivator(BlockEditorConverter);

        private class BlockGridItemActivator : BlockItemActivator<BlockGridItem>
        {
            public BlockGridItemActivator(BlockEditorConverter blockConverter) : base(blockConverter)
            {
            }

            protected override Type GenericItemType => typeof(BlockGridItem<,>);
        }
    }
}
