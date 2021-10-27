using System;
using Umbraco.Core;
using Umbraco.Core.Models.Blocks;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.PropertyEditors.ValueConverters
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

        public IPublishedElement ConvertToElement(BlockItemData data, PropertyCacheLevel referenceCacheLevel, bool preview)
        {
            var publishedContentType = GetContentType(data.ContentTypeKey);

            // Only convert element types
            if (publishedContentType == null || publishedContentType.IsElement == false)
            {
                return null;
            }

            var propertyValues = data.RawPropertyValues;

            // Get the UDI from the deserialized object. If this is empty, we can fallback to checking the 'key' if there is one
            var key = (data.Udi is GuidUdi gudi) ? gudi.Guid : Guid.Empty;
            if (key == Guid.Empty && propertyValues.TryGetValue("key", out var keyo))
            {
                Guid.TryParse(keyo.ToString(), out key);
            }

            IPublishedElement element = new PublishedElement(publishedContentType, key, propertyValues, preview, referenceCacheLevel, _publishedSnapshotAccessor);
            element = _publishedModelFactory.CreateModel(element);

            return element;
        }

        public Type GetModelType(Guid contentTypeKey)
        {
            var publishedContentType = GetContentType(contentTypeKey);
            if (publishedContentType != null)
            {
                var modelType = ModelType.For(publishedContentType.Alias);

                return _publishedModelFactory.MapModelType(modelType);
            }

            return typeof(IPublishedElement);
        }

        private IPublishedContentType GetContentType(Guid contentTypeKey)
        {
            // HACK! We need to cast, we have no choice because we can't make breaking changes (and we need the GUID overload)
            var publishedContentCache = _publishedSnapshotAccessor.PublishedSnapshot.Content as IPublishedContentCache2;
            if (publishedContentCache == null)
                throw new InvalidOperationException("The published content cache is not " + typeof(IPublishedContentCache2));

            return publishedContentCache.GetContentType(contentTypeKey);
        }
    }
}
