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

            ValidateCacheLevel(ReferenceCacheLevel, true);
            ValidateCacheLevel(PropertyType.CacheLevel, false);
        }

        private static void ValidateCacheLevel(PropertyCacheLevel cacheLevel, bool validateUnknown)
        {
            switch (cacheLevel)
            {
                case PropertyCacheLevel.Element:
                case PropertyCacheLevel.Elements:
                case PropertyCacheLevel.Snapshot:
                case PropertyCacheLevel.None:
                    break;
                case PropertyCacheLevel.Unknown:
                    if (!validateUnknown) goto default;
                    break;
                default:
                    throw new Exception($"Invalid cache level \"{cacheLevel}\".");
            }
        }

        public PublishedPropertyType PropertyType { get; }
        public string PropertyTypeAlias => PropertyType.PropertyTypeAlias;
        public PropertyCacheLevel ReferenceCacheLevel { get; }

        // these have to be provided by the actual implementation
        public abstract bool HasValue(int? languageId = null, string segment = null);
        public abstract object GetSourceValue(int? languageId = null, string segment = null);
        public abstract object GetValue(int? languageId = null, string segment = null);
        public abstract object GetXPathValue(int? languageId = null, string segment = null);
    }
}
