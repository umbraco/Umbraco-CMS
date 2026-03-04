// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Converts JSON block objects into <see cref="IPublishedElement" />.
/// </summary>
public sealed class BlockEditorConverter
{
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly BlockEditorVarianceHandler _blockEditorVarianceHandler;
    private readonly IBlockElementService _blockElementService;

    public BlockEditorConverter(
        IPublishedContentTypeCache publishedContentTypeCache,
        IPublishedModelFactory publishedModelFactory,
        IVariationContextAccessor variationContextAccessor,
        BlockEditorVarianceHandler blockEditorVarianceHandler,
        IBlockElementService blockElementService)
    {
        _publishedContentTypeCache = publishedContentTypeCache;
        _publishedModelFactory = publishedModelFactory;
        _variationContextAccessor = variationContextAccessor;
        _blockEditorVarianceHandler = blockEditorVarianceHandler;
        _blockElementService = blockElementService;
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

        var alignedProperties = new List<BlockPropertyValue>();

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

            alignedProperties.Add(alignedProperty);
        }

        var alignedData = new BlockItemData
        {
            Values = alignedProperties,
            ContentTypeAlias = data.ContentTypeAlias,
            ContentTypeKey = data.ContentTypeKey,
            Key = data.Key,
        };

        return _blockElementService.BuildElementAsync(alignedData, preview).GetAwaiter().GetResult();
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
