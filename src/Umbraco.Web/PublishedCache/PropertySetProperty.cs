using System;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache
{
    internal class PropertySetProperty : PropertySetPropertyBase
    {
        public PropertySetProperty(PublishedPropertyType propertyType, IPropertySet set, bool previewing, PropertyCacheLevel cacheLevel, object sourceValue = null)
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
