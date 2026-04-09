// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal sealed class RichTextBlockPropertyValueCreator : BlockPropertyValueCreatorBase<RichTextBlockModel, RichTextBlockItem, RichTextBlockLayoutItem, RichTextConfiguration.RichTextBlockConfiguration, RichTextBlockValue>
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly RichTextBlockPropertyValueConstructorCache _constructorCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.PropertyEditors.ValueConverters.RichTextBlockPropertyValueCreator"/> class.
    /// </summary>
    /// <param name="blockEditorConverter">The <see cref="BlockEditorConverter"/> used to convert block editor values.</param>
    /// <param name="variationContextAccessor">The <see cref="IVariationContextAccessor"/> providing access to the variation context.</param>
    /// <param name="blockEditorVarianceHandler">The <see cref="BlockEditorVarianceHandler"/> that handles block editor variance.</param>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/> used for JSON serialization and deserialization.</param>
    /// <param name="constructorCache">The <see cref="RichTextBlockPropertyValueConstructorCache"/> used to cache rich text block property value constructors.</param>
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

    /// <summary>
    /// Creates a <see cref="RichTextBlockModel"/> instance from the specified rich text block value and configuration.
    /// </summary>
    /// <param name="owner">The <see cref="IPublishedElement"/> that owns the property containing the rich text block.</param>
    /// <param name="referenceCacheLevel">The cache level to use for property references during model creation.</param>
    /// <param name="blockValue">The <see cref="RichTextBlockValue"/> representing the value to convert.</param>
    /// <param name="preview">True to create the model in preview mode; otherwise, false.</param>
    /// <param name="blockConfigurations">An array of <see cref="RichTextBlockConfiguration"/> objects that define the configuration for each block type.</param>
    /// <returns>A <see cref="RichTextBlockModel"/> that represents the parsed and configured block content, or <see cref="RichTextBlockModel.Empty"/> if the value is invalid or empty.</returns>
    public RichTextBlockModel CreateBlockModel(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, RichTextBlockValue blockValue, bool preview, RichTextConfiguration.RichTextBlockConfiguration[] blockConfigurations)
    {
        RichTextBlockModel CreateEmptyModel() => RichTextBlockModel.Empty;

        RichTextBlockModel CreateModel(IList<RichTextBlockItem> items) => new RichTextBlockModel(items);

        RichTextBlockModel blockModel = CreateBlockModel(owner, referenceCacheLevel, blockValue, preview, blockConfigurations, CreateEmptyModel, CreateModel);

        return blockModel;
    }

    protected override BlockEditorDataConverter<RichTextBlockValue, RichTextBlockLayoutItem> CreateBlockEditorDataConverter() => new RichTextEditorBlockDataConverter(_jsonSerializer);

    protected override BlockItemActivator<RichTextBlockItem> CreateBlockItemActivator() => new RichTextBlockItemActivator(BlockEditorConverter, _constructorCache);

    private sealed class RichTextBlockItemActivator : BlockItemActivator<RichTextBlockItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RichTextBlockItemActivator"/> class.
        /// </summary>
        /// <param name="blockConverter">The <see cref="BlockEditorConverter"/> used to convert block editor values.</param>
        /// <param name="constructorCache">The <see cref="RichTextBlockPropertyValueConstructorCache"/> used to cache constructors for rich text block property values.</param>
        public RichTextBlockItemActivator(BlockEditorConverter blockConverter, RichTextBlockPropertyValueConstructorCache constructorCache)
            : base(blockConverter, constructorCache)
        {
        }

        protected override Type GenericItemType => typeof(RichTextBlockItem<,>);
    }
}
