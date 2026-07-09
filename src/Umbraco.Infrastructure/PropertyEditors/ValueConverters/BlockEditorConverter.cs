// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Converts JSON block objects into <see cref="IPublishedElement" />.
/// </summary>
public sealed class BlockEditorConverter
{
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;
    private readonly ICacheManager _cacheManager;
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly BlockEditorVarianceHandler _blockEditorVarianceHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.PropertyEditors.ValueConverters.BlockEditorConverter"/> class.
    /// </summary>
    /// <param name="publishedContentTypeCache">Provides access to cached published content types for efficient lookup.</param>
    /// <param name="cacheManager">Manages caching for property editor values and related data.</param>
    /// <param name="publishedModelFactory">Factory for creating strongly typed published content models.</param>
    /// <param name="variationContextAccessor">Accessor for the current variation context, used for culture and segment variations.</param>
    /// <param name="blockEditorVarianceHandler">Handles variance logic specific to block editor properties.</param>
    public BlockEditorConverter(
        IPublishedContentTypeCache publishedContentTypeCache,
        ICacheManager cacheManager,
        IPublishedModelFactory publishedModelFactory,
        IVariationContextAccessor variationContextAccessor,
        BlockEditorVarianceHandler blockEditorVarianceHandler)
    {
        _publishedContentTypeCache = publishedContentTypeCache;
        _cacheManager = cacheManager;
        _publishedModelFactory = publishedModelFactory;
        _variationContextAccessor = variationContextAccessor;
        _blockEditorVarianceHandler = blockEditorVarianceHandler;
    }

    /// <summary>
    /// Converts a <see cref="BlockItemData"/> instance into an <see cref="IPublishedElement"/> for use in the block editor.
    /// </summary>
    /// <param name="owner">The parent <see cref="IPublishedElement"/> that owns the block element, used for context such as culture and segment variations.</param>
    /// <param name="data">The <see cref="BlockItemData"/> representing the block to convert.</param>
    /// <param name="referenceCacheLevel">The <see cref="PropertyCacheLevel"/> to use for resolving references during conversion.</param>
    /// <param name="preview">If <c>true</c>, conversion is performed in preview mode; otherwise, in published mode.</param>
    /// <returns>
    /// An <see cref="IPublishedElement"/> representing the converted block if the conversion is successful and the data is valid; otherwise, <c>null</c> if the content type is not found, is not an element type, or the key is missing or invalid.
    /// </returns>
    public IPublishedElement? ConvertToElement(IPublishedElement owner, BlockItemData data, PropertyCacheLevel referenceCacheLevel, bool preview)
    {
        // Only convert element types - content types will cause an exception when PublishedModelFactory creates the model
        IPublishedContentType? publishedContentType = _publishedContentTypeCache.Get(PublishedItemType.Element, data.ContentTypeKey);
        if (publishedContentType == null || publishedContentType.IsElement == false)
        {
            return null;
        }

        VariationContext variationContext = _variationContextAccessor.VariationContext ?? new VariationContext();

        var propertyTypesByAlias = publishedContentType
            .PropertyTypes
            .ToDictionary(propertyType => propertyType.Alias);

        var propertyValues = new Dictionary<string, object?>();
        foreach (BlockPropertyValue property in data.Values)
        {
            if (!propertyTypesByAlias.TryGetValue(property.Alias, out IPublishedPropertyType? propertyType))
            {
                continue;
            }

            // if case changes have been made to the content or element type variation since the parent content was published,
            // we need to align those changes for the block properties - unlike for root level properties, where these
            // things are handled when a content type is saved.
            BlockPropertyValue? alignedProperty = _blockEditorVarianceHandler.AlignedPropertyVarianceAsync(property, propertyType, owner).GetAwaiter().GetResult();
            if (alignedProperty is null)
            {
                continue;
            }

            var expectedCulture = owner.ContentType.VariesByCulture() && publishedContentType.VariesByCulture() && propertyType.VariesByCulture()
                ? variationContext.Culture
                : null;
            var expectedSegment = owner.ContentType.VariesBySegment() && publishedContentType.VariesBySegment() && propertyType.VariesBySegment()
                ? variationContext.Segment
                : null;

            if (alignedProperty.Culture.NullOrWhiteSpaceAsNull().InvariantEquals(expectedCulture.NullOrWhiteSpaceAsNull())
                && alignedProperty.Segment.NullOrWhiteSpaceAsNull().InvariantEquals(expectedSegment.NullOrWhiteSpaceAsNull()))
            {
                propertyValues[alignedProperty.Alias] = alignedProperty.Value;
            }
        }

        // Get the key from the deserialized object. If this is empty, we can fallback to checking the 'key' if there is one
        Guid key = data.Key;
        if (key == Guid.Empty && propertyValues.TryGetValue("key", out var keyo))
        {
            Guid.TryParse(keyo!.ToString(), out key);
        }

        if (key == Guid.Empty)
        {
            return null;
        }

        IPublishedElement element = new PublishedElement(publishedContentType, key, propertyValues, preview, referenceCacheLevel, variationContext, _cacheManager);
        element = _publishedModelFactory.CreateModel(element);

        return element;
    }

    /// <summary>
    /// Returns the model <see cref="Type"/> associated with the specified content type key.
    /// If the content type key does not correspond to an element type, returns <see cref="IPublishedElement"/>.
    /// </summary>
    /// <param name="contentTypeKey">The unique key identifying the content type.</param>
    /// <returns>The model <see cref="Type"/> for the content type key, or <see cref="IPublishedElement"/> if not found or not an element.</returns>
    public Type GetModelType(Guid contentTypeKey)
    {
        IPublishedContentType? publishedContentType = _publishedContentTypeCache.Get(PublishedItemType.Content, contentTypeKey);
        if (publishedContentType is not null && publishedContentType.IsElement)
        {
            return _publishedModelFactory.GetModelType(publishedContentType.Alias);
        }

        return typeof(IPublishedElement);
    }
}
