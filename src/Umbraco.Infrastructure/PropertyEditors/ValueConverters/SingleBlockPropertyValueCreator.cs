using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal sealed class SingleBlockPropertyValueCreator : BlockPropertyValueCreatorBase<BlockListModel, BlockListItem, SingleBlockLayoutItem, BlockListConfiguration.BlockConfiguration, SingleBlockValue>
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly BlockListPropertyValueConstructorCache _constructorCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.PropertyEditors.ValueConverters.SingleBlockPropertyValueCreator"/> class,
    /// used to create property values for single block editors in Umbraco.
    /// </summary>
    /// <param name="blockEditorConverter">The service used to convert block editor data.</param>
    /// <param name="variationContextAccessor">Provides access to the current variation context.</param>
    /// <param name="blockEditorVarianceHandler">Handles variance logic for block editors.</param>
    /// <param name="jsonSerializer">The serializer used for JSON serialization and deserialization.</param>
    /// <param name="constructorCache">A cache for constructors used in block list property value creation.</param>
    /// <param name="languageService">Service used to retrieve language information for fallback resolution.</param>
    public SingleBlockPropertyValueCreator(
        BlockEditorConverter blockEditorConverter,
        IVariationContextAccessor variationContextAccessor,
        IPropertyRenderingContextAccessor propertyRenderingContextAccessor,
        BlockEditorVarianceHandler blockEditorVarianceHandler,
        IJsonSerializer jsonSerializer,
        BlockListPropertyValueConstructorCache constructorCache,
        ILanguageService languageService)
        : base(blockEditorConverter, variationContextAccessor, propertyRenderingContextAccessor, blockEditorVarianceHandler, languageService)
    {
        _jsonSerializer = jsonSerializer;
        _constructorCache = constructorCache;
    }

    /// <summary>
    /// Creates a single <see cref="BlockListItem"/> model from the provided intermediate block model value.
    /// </summary>
    /// <remarks>The underlying Value is still stored as an array to allow for code reuse and easier migration</remarks>
    /// <param name="owner">The <see cref="IPublishedElement"/> that owns the property.</param>
    /// <param name="referenceCacheLevel">The cache level to use for property references.</param>
    /// <param name="intermediateBlockModelValue">The intermediate block model value as a string, typically JSON.</param>
    /// <param name="preview">True if the model is being created for preview purposes; otherwise, false.</param>
    /// <param name="blockConfigurations">An array of <see cref="BlockListConfiguration.BlockConfiguration"/> objects to use when creating the model.</param>
    /// <returns>A single <see cref="BlockListItem"/> representing the block model, or <c>null</c> if none could be created.</returns>
    public async Task<BlockListItem?> CreateBlockModelAsync(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, string intermediateBlockModelValue, bool preview, BlockListConfiguration.BlockConfiguration[] blockConfigurations)
    {
        BlockListModel CreateEmptyModel() => BlockListModel.Empty;

        BlockListModel CreateModel(IList<BlockListItem> items) => new BlockListModel(items);

        BlockListItem? blockModel = (await CreateBlockModelAsync(owner, referenceCacheLevel, intermediateBlockModelValue, preview, blockConfigurations, CreateEmptyModel, CreateModel)).SingleOrDefault();

        return blockModel;
    }

    protected override BlockEditorDataConverter<SingleBlockValue, SingleBlockLayoutItem> CreateBlockEditorDataConverter() => new SingleBlockEditorDataConverter(_jsonSerializer);

    protected override BlockItemActivator<BlockListItem> CreateBlockItemActivator() => new BlockListItemActivator(BlockEditorConverter, _constructorCache);

    private sealed class BlockListItemActivator : BlockItemActivator<BlockListItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockListItemActivator"/> class.
        /// </summary>
        /// <param name="blockConverter">The <see cref="BlockEditorConverter"/> used to convert block editor data.</param>
        /// <param name="constructorCache">The <see cref="BlockListPropertyValueConstructorCache"/> providing cached constructors for block list property values.</param>

        public BlockListItemActivator(BlockEditorConverter blockConverter, BlockListPropertyValueConstructorCache constructorCache)
            : base(blockConverter, constructorCache)
        {
        }

        protected override Type GenericItemType => typeof(BlockListItem<,>);
    }
}
