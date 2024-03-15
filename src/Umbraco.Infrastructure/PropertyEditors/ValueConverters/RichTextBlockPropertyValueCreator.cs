// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal class RichTextBlockPropertyValueCreator : BlockPropertyValueCreatorBase<RichTextBlockModel, RichTextBlockItem, RichTextBlockLayoutItem, RichTextConfiguration.RichTextBlockConfiguration>
{
    private readonly RichTextBlockPropertyValueConstructorCache _constructorCache;

    public RichTextBlockPropertyValueCreator(BlockEditorConverter blockEditorConverter, RichTextBlockPropertyValueConstructorCache constructorCache)
        : base(blockEditorConverter)
        => _constructorCache = constructorCache;

    public RichTextBlockModel CreateBlockModel(PropertyCacheLevel referenceCacheLevel, BlockValue blockValue, bool preview, RichTextConfiguration.RichTextBlockConfiguration[] blockConfigurations)
    {
        RichTextBlockModel CreateEmptyModel() => RichTextBlockModel.Empty;

        RichTextBlockModel CreateModel(IList<RichTextBlockItem> items) => new RichTextBlockModel(items);

        RichTextBlockModel blockModel = CreateBlockModel(referenceCacheLevel, blockValue, preview, blockConfigurations, CreateEmptyModel, CreateModel);

        return blockModel;
    }

    protected override BlockEditorDataConverter CreateBlockEditorDataConverter() => new RichTextEditorBlockDataConverter();

    protected override BlockItemActivator<RichTextBlockItem> CreateBlockItemActivator() => new RichTextBlockItemActivator(BlockEditorConverter, _constructorCache);

    private class RichTextBlockItemActivator : BlockItemActivator<RichTextBlockItem>
    {
        public RichTextBlockItemActivator(BlockEditorConverter blockConverter, RichTextBlockPropertyValueConstructorCache constructorCache)
            : base(blockConverter, constructorCache)
        {
        }

        protected override Type GenericItemType => typeof(RichTextBlockItem<,>);
    }
}
