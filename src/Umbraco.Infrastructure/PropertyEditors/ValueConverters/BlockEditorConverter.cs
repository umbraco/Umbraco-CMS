// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters
{
    /// <summary>
    /// Converts JSON block objects into <see cref="IPublishedElement" />.
    /// </summary>
    public sealed class BlockEditorConverter
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly IPublishedModelFactory _publishedModelFactory;

        public BlockEditorConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor, IPublishedModelFactory publishedModelFactory)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _publishedModelFactory = publishedModelFactory;
        }

        public IPublishedElement? ConvertToElement(BlockItemData data, PropertyCacheLevel referenceCacheLevel, bool preview)
        {
            var publishedContentCache = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot().Content;

            // Only convert element types - content types will cause an exception when PublishedModelFactory creates the model
            var publishedContentType = publishedContentCache?.GetContentType(data.ContentTypeKey);
            if (publishedContentType == null || publishedContentType.IsElement == false)
            {
                return null;
            }

            var propertyValues = data.RawPropertyValues;

            // Get the UDI from the deserialized object. If this is empty, we can fallback to checking the 'key' if there is one
            var key = (data.Udi is GuidUdi gudi) ? gudi.Guid : Guid.Empty;
            if (key == Guid.Empty && propertyValues.TryGetValue("key", out var keyo))
            {
                Guid.TryParse(keyo!.ToString(), out key);
            }

            IPublishedElement element = new PublishedElement(publishedContentType, key, propertyValues, preview, referenceCacheLevel, _publishedSnapshotAccessor);
            element = _publishedModelFactory.CreateModel(element);

            return element;
        }

        public Type GetModelType(Guid contentTypeKey)
        {
            var publishedContentCache = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot().Content;
            var publishedContentType = publishedContentCache?.GetContentType(contentTypeKey);
            if (publishedContentType is not null && publishedContentType.IsElement)
            {
                return _publishedModelFactory.GetModelType(publishedContentType.Alias);
            }

            return typeof(IPublishedElement);
        }
    }
}
