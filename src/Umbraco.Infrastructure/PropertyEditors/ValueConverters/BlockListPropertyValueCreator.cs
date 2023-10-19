using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal class BlockListPropertyValueCreator : BlockPropertyValueCreatorBase<BlockListModel, BlockListItem, BlockListLayoutItem, BlockListConfiguration.BlockConfiguration>
{
    public BlockListPropertyValueCreator(BlockEditorConverter blockEditorConverter)
        : base(blockEditorConverter)
    {
    }

    public BlockListModel CreateBlockModel(PropertyCacheLevel referenceCacheLevel, string intermediateBlockModelValue, bool preview, BlockListConfiguration.BlockConfiguration[] blockConfigurations)
    {
        BlockListModel CreateEmptyModel() => BlockListModel.Empty;

        BlockListModel CreateModel(IList<BlockListItem> items) => new BlockListModel(items);

        BlockListModel blockModel = CreateBlockModel(referenceCacheLevel, intermediateBlockModelValue, preview, blockConfigurations, CreateEmptyModel, CreateModel);

        return blockModel;
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
