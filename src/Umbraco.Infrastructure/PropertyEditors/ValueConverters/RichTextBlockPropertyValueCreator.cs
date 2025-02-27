// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal class RichTextBlockPropertyValueCreator : BlockPropertyValueCreatorBase<RichTextBlockModel, RichTextBlockItem, RichTextBlockLayoutItem, RichTextConfiguration.RichTextBlockConfiguration, RichTextBlockValue>
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly RichTextBlockPropertyValueConstructorCache _constructorCache;

    public RichTextBlockPropertyValueCreator(
        BlockEditorConverter blockEditorConverter,
        IVariationContextAccessor variationContextAccessor,
        BlockEditorVarianceHandler blockEditorVarianceHandler,
        IJsonSerializer jsonSerializer,
        RichTextBlockPropertyValueConstructorCache constructorCache)
        : base(blockEditorConverter, variationContextAccessor, blockEditorVarianceHandler)
    {
        _jsonSerializer = jsonSerializer;
        _constructorCache = constructorCache;
    }

    public RichTextBlockModel CreateBlockModel(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, RichTextBlockValue blockValue, bool preview, RichTextConfiguration.RichTextBlockConfiguration[] blockConfigurations)
    {
        RichTextBlockModel CreateEmptyModel() => RichTextBlockModel.Empty;

        RichTextBlockModel CreateModel(IList<RichTextBlockItem> items) => new RichTextBlockModel(items);

        RichTextBlockModel blockModel = CreateBlockModel(owner, referenceCacheLevel, blockValue, preview, blockConfigurations, CreateEmptyModel, CreateModel);

        return blockModel;
    }

    protected override BlockEditorDataConverter<RichTextBlockValue, RichTextBlockLayoutItem> CreateBlockEditorDataConverter() => new RichTextEditorBlockDataConverter(_jsonSerializer);

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
