using System;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache
{
    class PropertySetProperty : PropertySetPropertyBase
    {
        public PropertySetProperty(PublishedPropertyType propertyType, Guid fragmentKey, bool previewing, PropertyCacheLevel cacheLevel, object sourceValue = null)
            : base(propertyType, fragmentKey, previewing, cacheLevel, sourceValue)
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
