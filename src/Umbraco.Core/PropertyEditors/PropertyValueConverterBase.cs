using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Provides a default overridable implementation for <see cref="IPropertyValueConverter"/> that does nothing.
    /// </summary>
    public abstract class PropertyValueConverterBase : IPropertyValueConverter
    {
        public virtual bool IsConverter(PublishedPropertyType propertyType)
            => false;

        public bool IsValue(object value)
            => value != null && (!(value is string) || string.IsNullOrWhiteSpace((string) value) == false);

        public virtual Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (object);

        public virtual PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Snapshot;

        public virtual object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
            => source;

        public virtual object ConvertIntermediateToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            => inter;

        public virtual object ConvertIntermediateToXPath(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            => inter?.ToString() ?? string.Empty;
    }
}
