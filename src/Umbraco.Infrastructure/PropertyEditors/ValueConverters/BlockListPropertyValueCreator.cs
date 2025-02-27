using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal class BlockListPropertyValueCreator : BlockPropertyValueCreatorBase<BlockListModel, BlockListItem, BlockListLayoutItem, BlockListConfiguration.BlockConfiguration, BlockListValue>
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly BlockListPropertyValueConstructorCache _constructorCache;

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

    public BlockListModel CreateBlockModel(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, string intermediateBlockModelValue, bool preview, BlockListConfiguration.BlockConfiguration[] blockConfigurations)
    {
        BlockListModel CreateEmptyModel() => BlockListModel.Empty;

        BlockListModel CreateModel(IList<BlockListItem> items) => new BlockListModel(items);

        BlockListModel blockModel = CreateBlockModel(owner, referenceCacheLevel, intermediateBlockModelValue, preview, blockConfigurations, CreateEmptyModel, CreateModel);

        return blockModel;
    }

    protected override BlockEditorDataConverter<BlockListValue, BlockListLayoutItem> CreateBlockEditorDataConverter() => new BlockListEditorDataConverter(_jsonSerializer);

    protected override BlockItemActivator<BlockListItem> CreateBlockItemActivator() => new BlockListItemActivator(BlockEditorConverter, _constructorCache);

    private class BlockListItemActivator : BlockItemActivator<BlockListItem>
    {
        public BlockListItemActivator(BlockEditorConverter blockConverter, BlockListPropertyValueConstructorCache constructorCache)
            : base(blockConverter, constructorCache)
        {
        }

        protected override Type GenericItemType => typeof(BlockListItem<,>);
    }
}
