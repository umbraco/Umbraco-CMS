// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal abstract class BlockPropertyValueCreatorBase<TBlockModel, TBlockItemModel, TBlockLayoutItem, TBlockConfiguration, TBlockValue>
    where TBlockModel : BlockModelCollection<TBlockItemModel>
    where TBlockItemModel : class, IBlockReference<IPublishedElement, IPublishedElement>
    where TBlockLayoutItem : class, IBlockLayoutItem, new()
    where TBlockConfiguration : IBlockConfiguration
    where TBlockValue : BlockValue<TBlockLayoutItem>, new()
{
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly BlockEditorVarianceHandler _blockEditorVarianceHandler;

    /// <summary>
    /// Creates a specific data converter for the block property implementation.
    /// </summary>
    /// <returns></returns>
    protected abstract BlockEditorDataConverter<TBlockValue, TBlockLayoutItem> CreateBlockEditorDataConverter();

    /// <summary>
    /// Creates a specific block item activator for the block property implementation.
    /// </summary>
    /// <returns></returns>
    protected abstract BlockItemActivator<TBlockItemModel> CreateBlockItemActivator();

    /// <summary>
    /// Creates an empty block model, i.e. for uninitialized or invalid property values.
    /// </summary>
    /// <returns></returns>
    protected delegate TBlockModel CreateEmptyBlockModel();

    /// <summary>
    /// Creates a block model for a list of block items.
    /// </summary>
    /// <param name="blockItems">The block items to base the block model on.</param>
    /// <returns></returns>
    protected delegate TBlockModel CreateBlockModelFromItems(IList<TBlockItemModel> blockItems);

    /// <summary>
    /// Creates a block item from a block layout item.
    /// </summary>
    /// <param name="layoutItem">The block layout item to base the block item on.</param>
    /// <returns></returns>
    protected delegate TBlockItemModel? CreateBlockItemModelFromLayout(TBlockLayoutItem layoutItem);

    /// <summary>
    /// Enriches a block item after it has been created by the block item activator. Use this to set block item data based on concrete block layout and configuration.
    /// </summary>
    /// <param name="item">The block item to enrich.</param>
    /// <param name="layoutItem">The block layout item for the block item being enriched.</param>
    /// <param name="configuration">The configuration of the block.</param>
    /// <param name="blockItemModelCreator">Delegate for creating new block items from block layout items.</param>
    /// <returns></returns>
    protected delegate TBlockItemModel? EnrichBlockItemModelFromConfiguration(TBlockItemModel item, TBlockLayoutItem layoutItem, TBlockConfiguration configuration, CreateBlockItemModelFromLayout blockItemModelCreator);

    protected BlockPropertyValueCreatorBase(BlockEditorConverter blockEditorConverter, IVariationContextAccessor variationContextAccessor, BlockEditorVarianceHandler blockEditorVarianceHandler)
    {
        BlockEditorConverter = blockEditorConverter;
        _variationContextAccessor = variationContextAccessor;
        _blockEditorVarianceHandler = blockEditorVarianceHandler;
    }

    protected BlockEditorConverter BlockEditorConverter { get; }

    protected TBlockModel CreateBlockModel(
        IPublishedElement owner,
        PropertyCacheLevel referenceCacheLevel,
        string intermediateBlockModelValue,
        bool preview,
        IEnumerable<TBlockConfiguration> blockConfigurations,
        CreateEmptyBlockModel createEmptyModel,
        CreateBlockModelFromItems createModelFromItems,
        EnrichBlockItemModelFromConfiguration? enrichBlockItem = null)
    {
        // Short-circuit on empty values
        if (intermediateBlockModelValue.IsNullOrWhiteSpace())
        {
            return createEmptyModel();
        }

        BlockEditorDataConverter<TBlockValue, TBlockLayoutItem> blockEditorDataConverter = CreateBlockEditorDataConverter();
        BlockEditorData<TBlockValue, TBlockLayoutItem> converted = blockEditorDataConverter.Deserialize(intermediateBlockModelValue);
        return CreateBlockModel(owner, referenceCacheLevel, converted, preview, blockConfigurations, createEmptyModel, createModelFromItems, enrichBlockItem);
    }

    protected TBlockModel CreateBlockModel(
        IPublishedElement owner,
        PropertyCacheLevel referenceCacheLevel,
        TBlockValue blockValue,
        bool preview,
        IEnumerable<TBlockConfiguration> blockConfigurations,
        CreateEmptyBlockModel createEmptyModel,
        CreateBlockModelFromItems createModelFromItems,
        EnrichBlockItemModelFromConfiguration? enrichBlockItem = null)
    {
        BlockEditorDataConverter<TBlockValue, TBlockLayoutItem> blockEditorDataConverter = CreateBlockEditorDataConverter();
        BlockEditorData<TBlockValue, TBlockLayoutItem> converted = blockEditorDataConverter.Convert(blockValue);
        return CreateBlockModel(owner, referenceCacheLevel, converted, preview, blockConfigurations, createEmptyModel, createModelFromItems, enrichBlockItem);
    }

    private TBlockModel CreateBlockModel(
        IPublishedElement owner,
        PropertyCacheLevel referenceCacheLevel,
        BlockEditorData<TBlockValue, TBlockLayoutItem> converted,
        bool preview,
        IEnumerable<TBlockConfiguration> blockConfigurations,
        CreateEmptyBlockModel createEmptyModel,
        CreateBlockModelFromItems createModelFromItems,
        EnrichBlockItemModelFromConfiguration? enrichBlockItem = null)
    {
        if (converted.BlockValue.ContentData.Count == 0)
        {
            return createEmptyModel();
        }

        if (converted.Layout is null)
        {
            return createEmptyModel();
        }

        var blockConfigMap = blockConfigurations.ToDictionary(bc => bc.ContentElementTypeKey);
        VariationContext variationContext = _variationContextAccessor.VariationContext ?? new VariationContext();

        // Convert the content data
        var contentPublishedElements = new Dictionary<Guid, IPublishedElement>();
        foreach (BlockItemData data in converted.BlockValue.ContentData)
        {
            if (!blockConfigMap.ContainsKey(data.ContentTypeKey))
            {
                continue;
            }

            IPublishedElement? element = BlockEditorConverter.ConvertToElement(owner, data, referenceCacheLevel, preview);
            if (element == null)
            {
                continue;
            }

            // if case changes have been made to the content or element type variation since the content was published,
            // we need to align those changes for the exposed blocks.
            IEnumerable<BlockItemVariation> expose = _blockEditorVarianceHandler.AlignedExposeVarianceAsync(converted.BlockValue, owner, element).GetAwaiter().GetResult();
            var expectedBlockVariationCulture = owner.ContentType.VariesByCulture() && element.ContentType.VariesByCulture()
                ? variationContext.Culture.NullOrWhiteSpaceAsNull()
                : null;
            var expectedBlockVariationSegment = owner.ContentType.VariesBySegment() && element.ContentType.VariesBySegment()
                ? variationContext.Segment.NullOrWhiteSpaceAsNull()
                : null;
            if (expose.Any(v =>
                    v.ContentKey == element.Key && v.Culture == expectedBlockVariationCulture &&
                    v.Segment == expectedBlockVariationSegment) is false)
            {
                continue;
            }

            contentPublishedElements[element.Key] = element;
        }

        // If there are no content elements, it doesn't matter what is stored in layout
        if (contentPublishedElements.Count == 0)
        {
            return createEmptyModel();
        }

        // Convert the settings data
        var settingsPublishedElements = new Dictionary<Guid, IPublishedElement>();
        var validSettingsElementTypes = blockConfigMap.Values.Select(x => x.SettingsElementTypeKey)
            .Where(x => x.HasValue).Distinct().ToList();
        foreach (BlockItemData data in converted.BlockValue.SettingsData)
        {
            if (!validSettingsElementTypes.Contains(data.ContentTypeKey))
            {
                continue;
            }

            IPublishedElement? element = BlockEditorConverter.ConvertToElement(owner, data, referenceCacheLevel, preview);
            if (element is null)
            {
                continue;
            }

            settingsPublishedElements[element.Key] = element;
        }

        BlockItemActivator<TBlockItemModel> blockItemActivator = CreateBlockItemActivator();

        TBlockItemModel? CreateBlockItem(TBlockLayoutItem layoutItem)
        {
            // Get the content reference
            if (!contentPublishedElements.TryGetValue(layoutItem.ContentKey, out IPublishedElement? contentData))
            {
                return null;
            }

            if (!blockConfigMap.TryGetValue(
                    contentData.ContentType.Key,
                    out TBlockConfiguration? blockConfig))
            {
                return null;
            }

            // Get the setting reference
            IPublishedElement? settingsData = null;
            if (layoutItem.SettingsKey.HasValue)
            {
                settingsPublishedElements.TryGetValue(layoutItem.SettingsKey.Value, out settingsData);
            }

            // This can happen if they have a settings type, save content, remove the settings type, and display the front-end page before saving the content again
            // We also ensure that the content types match, since maybe the settings type has been changed after this has been persisted
            if (settingsData is not null && (!blockConfig.SettingsElementTypeKey.HasValue ||
                                             settingsData.ContentType.Key != blockConfig.SettingsElementTypeKey))
            {
                settingsData = null;
            }

            // Create instance (use content/settings type from configuration)
            var blockItem = blockItemActivator.CreateInstance(blockConfig.ContentElementTypeKey, blockConfig.SettingsElementTypeKey, layoutItem.ContentKey, contentData, layoutItem.SettingsKey, settingsData);
            if (blockItem == null)
            {
                return null;
            }

            if (enrichBlockItem != null)
            {
                blockItem = enrichBlockItem(blockItem, layoutItem, blockConfig, CreateBlockItem);
            }

            return blockItem;
        }

        var blockItems = converted.Layout.Select(CreateBlockItem).WhereNotNull().ToList();
        return createModelFromItems(blockItems);
    }

    // Cache constructors locally (it's tied to the current IPublishedSnapshot and IPublishedModelFactory)
    protected abstract class BlockItemActivator<T>
        where T : IBlockReference<IPublishedElement, IPublishedElement>
    {
        protected abstract Type GenericItemType { get; }

        private readonly BlockEditorConverter _blockConverter;

        private readonly BlockEditorPropertyValueConstructorCacheBase<T> _constructorCache;

        public BlockItemActivator(BlockEditorConverter blockConverter, BlockEditorPropertyValueConstructorCacheBase<T> constructorCache)
        {
            _blockConverter = blockConverter;
            _constructorCache = constructorCache;
        }

        public T CreateInstance(Guid contentTypeKey, Guid? settingsTypeKey, Guid contentKey, IPublishedElement contentData, Guid? settingsKey, IPublishedElement? settingsData)
        {
            if (!_constructorCache.TryGetValue(
                (contentTypeKey, settingsTypeKey),
                out Func<Guid, IPublishedElement, Guid?, IPublishedElement?, T>? constructor))
            {
                constructor = EmitConstructor(contentTypeKey, settingsTypeKey);
                _constructorCache.SetValue((contentTypeKey, settingsTypeKey), constructor);
            }

            return constructor(contentKey, contentData, settingsKey, settingsData);
        }

        private Func<Guid, IPublishedElement, Guid?, IPublishedElement?, T> EmitConstructor(
            Guid contentTypeKey, Guid? settingsTypeKey)
        {
            Type contentType = _blockConverter.GetModelType(contentTypeKey);
            Type settingsType = settingsTypeKey.HasValue
                ? _blockConverter.GetModelType(settingsTypeKey.Value)
                : typeof(IPublishedElement);
            Type type = GenericItemType.MakeGenericType(contentType, settingsType);

            ConstructorInfo? constructor =
                type.GetConstructor(new[] { typeof(Guid), contentType, typeof(Guid?), settingsType });
            if (constructor == null)
            {
                throw new InvalidOperationException($"Could not find the required public constructor on {type}.");
            }

            // We use unsafe here, because we know the constructor parameter count and types match
            return ReflectionUtilities
                .EmitConstructorUnsafe<Func<Guid, IPublishedElement, Guid?, IPublishedElement?, T>>(
                    constructor);
        }
    }
}
