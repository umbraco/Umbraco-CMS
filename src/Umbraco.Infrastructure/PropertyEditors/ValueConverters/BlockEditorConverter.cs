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

        IPublishedElement element = new PublishedElement(publishedContentType, key, propertyValues, preview, referenceCacheLevel, _cacheManager);
        element = _publishedModelFactory.CreateModel(element);

        return element;
    }

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
