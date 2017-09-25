using System;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache
{
    internal class PublishedElementProperty : PublishedElementPropertyBase
    {
        public PublishedElementProperty(PublishedPropertyType propertyType, IPublishedElement set, bool previewing, PropertyCacheLevel cacheLevel, object sourceValue = null)
            : base(propertyType, set, previewing, cacheLevel, sourceValue)
        { }

        protected override CacheValues GetSnapshotCacheValues()
        {
            throw new NotSupportedException();
        }

        protected override CacheValues GetFacadeCacheValues()
        {
            throw new NotSupportedException();
        }
    }
}
