using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal sealed class BlockListPropertyValueCreator : BlockPropertyValueCreatorBase<BlockListModel, BlockListItem, BlockListLayoutItem, BlockListConfiguration.BlockConfiguration, BlockListValue>
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly BlockListPropertyValueConstructorCache _constructorCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockListPropertyValueCreator"/> class, responsible for creating property values for block list editors.
    /// </summary>
    /// <param name="blockEditorConverter">The service used to convert block editor data into strongly typed objects.</param>
    /// <param name="variationContextAccessor">Provides access to the current variation context, used for handling content variations.</param>
    /// <param name="blockEditorVarianceHandler">Handles variance logic for block editors, determining how values vary by culture or segment.</param>
    /// <param name="jsonSerializer">The serializer used for serializing and deserializing JSON data related to block list properties.</param>
    /// <param name="constructorCache">A cache that stores constructors for block list property values to improve performance.</param>
    public BlockListPropertyValueCreator(
        BlockEditorConverter blockEditorConverter,
        IVariationContextAccessor variationContextAccessor,
        BlockEditorVarianceHandler blockEditorVarianceHandler,
        IJsonSerializer jsonSerializer,
        BlockListPropertyValueConstructorCache constructorCache)
        : base(blockEditorConverter, variationContextAccessor, blockEditorVarianceHandler)
    {
        _jsonSerializer = jsonSerializer;
        _constructorCache = constructorCache;
    }

    /// <summary>
    /// Creates a <see cref="BlockListModel"/> from the provided intermediate block model value and configuration.
    /// </summary>
    /// <param name="owner">The published element that owns the property for which the block model is being created.</param>
    /// <param name="referenceCacheLevel">The cache level to use for property references during model creation.</param>
    /// <param name="intermediateBlockModelValue">A string containing the serialized intermediate value representing the block list.</param>
    /// <param name="preview">True if the model should be created in preview mode; otherwise, false.</param>
    /// <param name="blockConfigurations">An array of <see cref="BlockListConfiguration.BlockConfiguration"/> objects used to configure the blocks.</param>
    /// <returns>A <see cref="BlockListModel"/> instance representing the constructed block list.</returns>
    public BlockListModel CreateBlockModel(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, string intermediateBlockModelValue, bool preview, BlockListConfiguration.BlockConfiguration[] blockConfigurations)
    {
        BlockListModel CreateEmptyModel() => BlockListModel.Empty;

        BlockListModel CreateModel(IList<BlockListItem> items) => new BlockListModel(items);

        BlockListModel blockModel = CreateBlockModel(owner, referenceCacheLevel, intermediateBlockModelValue, preview, blockConfigurations, CreateEmptyModel, CreateModel);

        return blockModel;
    }

    protected override BlockEditorDataConverter<BlockListValue, BlockListLayoutItem> CreateBlockEditorDataConverter() => new BlockListEditorDataConverter(_jsonSerializer);

    protected override BlockItemActivator<BlockListItem> CreateBlockItemActivator() => new BlockListItemActivator(BlockEditorConverter, _constructorCache);

    private sealed class BlockListItemActivator : BlockItemActivator<BlockListItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockListItemActivator"/> class.
        /// </summary>
        /// <param name="blockConverter">The <see cref="BlockEditorConverter"/> used to convert block editor data.</param>
        /// <param name="constructorCache">The <see cref="BlockListPropertyValueConstructorCache"/> that provides cached constructors for block list property values.</param>
        public BlockListItemActivator(BlockEditorConverter blockConverter, BlockListPropertyValueConstructorCache constructorCache)
            : base(blockConverter, constructorCache)
        {
        }

        protected override Type GenericItemType => typeof(BlockListItem<,>);
    }
}
