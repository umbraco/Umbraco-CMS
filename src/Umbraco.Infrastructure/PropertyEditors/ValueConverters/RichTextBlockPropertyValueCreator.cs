// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal class RichTextBlockPropertyValueCreator : BlockPropertyValueCreatorBase<RichTextBlockModel, RichTextBlockItem, RichTextBlockLayoutItem, RichTextConfiguration.RichTextBlockConfiguration>
{
    public RichTextBlockPropertyValueCreator(BlockEditorConverter blockEditorConverter)
        : base(blockEditorConverter)
    {
    }

    public RichTextBlockModel CreateBlockModel(PropertyCacheLevel referenceCacheLevel, BlockValue blockValue, bool preview, RichTextConfiguration.RichTextBlockConfiguration[] blockConfigurations)
    {
        RichTextBlockModel CreateEmptyModel() => RichTextBlockModel.Empty;

        RichTextBlockModel CreateModel(IList<RichTextBlockItem> items) => new RichTextBlockModel(items);

        RichTextBlockModel blockModel = CreateBlockModel(referenceCacheLevel, blockValue, preview, blockConfigurations, CreateEmptyModel, CreateModel);

        return blockModel;
    }

    protected override BlockEditorDataConverter CreateBlockEditorDataConverter() => new RichTextEditorBlockDataConverter();

    protected override BlockItemActivator<RichTextBlockItem> CreateBlockItemActivator() => new RichTextBlockItemActivator(BlockEditorConverter);

    private class RichTextBlockItemActivator : BlockItemActivator<RichTextBlockItem>
    {
        public RichTextBlockItemActivator(BlockEditorConverter blockConverter) : base(blockConverter)
        {
        }

        protected override Type GenericItemType => typeof(RichTextBlockItem<,>);
    }
}
