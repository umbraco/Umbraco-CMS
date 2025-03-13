using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal class BlockListPropertyValueCreator : BlockPropertyValueCreatorBase<BlockListModel, BlockListItem, BlockListLayoutItem, BlockListConfiguration.BlockConfiguration>
{
    private readonly BlockListPropertyValueConstructorCache _constructorCache;

    public BlockListPropertyValueCreator(BlockEditorConverter blockEditorConverter, BlockListPropertyValueConstructorCache constructorCache)
        : base(blockEditorConverter) =>
        _constructorCache = constructorCache;

    public BlockListModel CreateBlockModel(PropertyCacheLevel referenceCacheLevel, string intermediateBlockModelValue, bool preview, BlockListConfiguration.BlockConfiguration[] blockConfigurations)
    {
        BlockListModel CreateEmptyModel() => BlockListModel.Empty;

        BlockListModel CreateModel(IList<BlockListItem> items) => new BlockListModel(items);

        BlockListModel blockModel = CreateBlockModel(referenceCacheLevel, intermediateBlockModelValue, preview, blockConfigurations, CreateEmptyModel, CreateModel);

        return blockModel;
    }

    protected override BlockEditorDataConverter CreateBlockEditorDataConverter() => new BlockListEditorDataConverter();

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
