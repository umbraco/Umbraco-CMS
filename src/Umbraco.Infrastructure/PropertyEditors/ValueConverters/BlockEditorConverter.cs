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
    private readonly ICacheManager _cacheManager;
    private readonly IPublishedModelFactory _publishedModelFactory;

    public BlockEditorConverter(
        IPublishedContentTypeCache publishedContentTypeCache,
        ICacheManager cacheManager,
        IPublishedModelFactory publishedModelFactory)
    {
        _publishedContentTypeCache = publishedContentTypeCache;
        _cacheManager = cacheManager;
        _publishedModelFactory = publishedModelFactory;
    }

    public IPublishedElement? ConvertToElement(BlockItemData data, PropertyCacheLevel referenceCacheLevel, bool preview)
    {
        // Only convert element types - content types will cause an exception when PublishedModelFactory creates the model
        IPublishedContentType? publishedContentType = _publishedContentTypeCache.Get(PublishedItemType.Element, data.ContentTypeKey);
        if (publishedContentType == null || publishedContentType.IsElement == false)
        {
            return null;
        }

        Dictionary<string, object?> propertyValues = data.RawPropertyValues;

        // Get the UDI from the deserialized object. If this is empty, we can fallback to checking the 'key' if there is one
        Guid key = data.Udi is GuidUdi gudi ? gudi.Guid : Guid.Empty;
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
        IPublishedContentType? publishedContentType = _publishedContentTypeCache.Get(PublishedItemType.Element, contentTypeKey);
        if (publishedContentType is not null && publishedContentType.IsElement)
        {
            return _publishedModelFactory.GetModelType(publishedContentType.Alias);
        }

        return typeof(IPublishedElement);
    }
}
