﻿using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Provides a default overridable implementation for <see cref="IPropertyValueConverter"/> that does nothing.
    /// </summary>
    public abstract class PropertyValueConverterBase : IPropertyValueConverter
    {
        public virtual bool IsConverter(PublishedPropertyType propertyType)
        {
            return false;
        }

        public virtual Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof (object);
        }

        public virtual PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
        {
            return PropertyCacheLevel.Snapshot;
        }

        public virtual object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            return source;
        }

        public virtual object ConvertIntermediateToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            return inter;
        }

        public virtual object ConvertIntermediateToXPath(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            return inter?.ToString() ?? string.Empty;
        }
    }
}
