using System;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a base class for <c>IPublishedProperty</c> implementations which converts and caches
    /// the value source to the actual value to use when rendering content.
    /// </summary>
    internal abstract class PublishedPropertyBase : IPublishedProperty
    {
        protected PublishedPropertyBase(PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel)
        {
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            ReferenceCacheLevel = referenceCacheLevel;

            ValidateCacheLevel(ReferenceCacheLevel);
            ValidateCacheLevel(PropertyType.CacheLevel);
        }

        private static void ValidateCacheLevel(PropertyCacheLevel cacheLevel)
        {
            switch (cacheLevel)
            {
                case PropertyCacheLevel.Content:
                case PropertyCacheLevel.Snapshot:
                case PropertyCacheLevel.Facade:
                case PropertyCacheLevel.None:
                    break;
                default:
                    throw new Exception("Invalid cache level.");
            }
        }

        public PublishedPropertyType PropertyType { get; }
        public string PropertyTypeAlias => PropertyType.PropertyTypeAlias;
        public PropertyCacheLevel ReferenceCacheLevel { get; }

        // these have to be provided by the actual implementation
        public abstract bool HasValue { get; }
        public abstract object SourceValue { get; }
        public abstract object Value { get; }
        public abstract object XPathValue { get; }
    }
}
