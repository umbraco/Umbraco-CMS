using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal sealed class SingleBlockPropertyValueCreator : BlockPropertyValueCreatorBase<BlockListModel, BlockListItem, SingleBlockLayoutItem, BlockListConfiguration.BlockConfiguration, SingleBlockValue>
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly BlockListPropertyValueConstructorCache _constructorCache;

    public SingleBlockPropertyValueCreator(
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

    // The underlying Value is still stored as an array to allow for code reuse and easier migration
    public BlockListItem? CreateBlockModel(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, string intermediateBlockModelValue, bool preview, BlockListConfiguration.BlockConfiguration[] blockConfigurations)
    {
        BlockListModel CreateEmptyModel() => BlockListModel.Empty;

        BlockListModel CreateModel(IList<BlockListItem> items) => new BlockListModel(items);

        BlockListItem? blockModel = CreateBlockModel(owner, referenceCacheLevel, intermediateBlockModelValue, preview, blockConfigurations, CreateEmptyModel, CreateModel).SingleOrDefault();

        return blockModel;
    }

    protected override BlockEditorDataConverter<SingleBlockValue, SingleBlockLayoutItem> CreateBlockEditorDataConverter() => new SingleBlockEditorDataConverter(_jsonSerializer);

    protected override BlockItemActivator<BlockListItem> CreateBlockItemActivator() => new BlockListItemActivator(BlockEditorConverter, _constructorCache);

    private sealed class BlockListItemActivator : BlockItemActivator<BlockListItem>
    {
        public BlockListItemActivator(BlockEditorConverter blockConverter, BlockListPropertyValueConstructorCache constructorCache)
            : base(blockConverter, constructorCache)
        {
        }

        protected override Type GenericItemType => typeof(BlockListItem<,>);
    }
}
