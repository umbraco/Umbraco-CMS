using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal sealed class BlockGridPropertyValueCreator : BlockPropertyValueCreatorBase<BlockGridModel, BlockGridItem, BlockGridLayoutItem, BlockGridConfiguration.BlockGridBlockConfiguration, BlockGridValue>
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly BlockGridPropertyValueConstructorCache _constructorCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockGridPropertyValueCreator"/> class, which is responsible for creating property values for block grid editors.
    /// </summary>
    /// <param name="blockEditorConverter">The service used to convert block editor data into strongly typed objects.</param>
    /// <param name="variationContextAccessor">Provides access to the current variation context for content.</param>
    /// <param name="blockEditorVarianceHandler">Handles variance logic for block editors, such as culture or segment variations.</param>
    /// <param name="jsonSerializer">The serializer used to handle JSON data for block grid properties.</param>
    /// <param name="constructorCache">A cache for constructors used when creating block grid property values, improving performance.</param>
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

    /// <summary>
    /// Constructs a <see cref="BlockGridModel"/> instance from an intermediate serialized block grid value, using the provided configuration and context.
    /// </summary>
    /// <param name="owner">The <see cref="IPublishedElement"/> that owns the property for which the block grid model is being created.</param>
    /// <param name="referenceCacheLevel">The <see cref="PropertyCacheLevel"/> to use when resolving references within the block grid.</param>
    /// <param name="intermediateBlockModelValue">A string containing the intermediate (typically JSON) representation of the block grid's data.</param>
    /// <param name="preview">True if the model should be created in preview mode; otherwise, false.</param>
    /// <param name="blockConfigurations">An array of <see cref="BlockGridBlockConfiguration"/> objects that define the available block types and their settings.</param>
    /// <param name="gridColumns">The number of columns in the grid layout, or null to use the default configuration.</param>
    /// <returns>A <see cref="BlockGridModel"/> representing the structured block grid data for the given property and configuration.</returns>
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
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockGridItemActivator"/> class.
        /// </summary>
        /// <param name="blockConverter">The <see cref="BlockEditorConverter"/> used to convert block editor values.</param>
        /// <param name="constructorCache">The <see cref="BlockGridPropertyValueConstructorCache"/> used to cache constructors for block grid property values.</param>
        public BlockGridItemActivator(BlockEditorConverter blockConverter, BlockGridPropertyValueConstructorCache constructorCache)
            : base(blockConverter, constructorCache)
        {
        }

        protected override Type GenericItemType => typeof(BlockGridItem<,>);
    }
}
