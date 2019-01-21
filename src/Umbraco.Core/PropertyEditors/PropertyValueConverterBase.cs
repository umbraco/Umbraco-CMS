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

        public virtual bool? IsValue(object value, PropertyValueLevel level)
        {
            switch (level)
            {
                case PropertyValueLevel.Source:
                    return value != null && (!(value is string) || string.IsNullOrWhiteSpace((string) value) == false);
                default:
                    throw new NotSupportedException($"Invalid level: {level}.");
            }
        }

        public virtual bool HasValue(IPublishedProperty property, string culture, string segment)
        {
            // the default implementation uses the old magic null & string comparisons,
            // other implementations may be more clever, and/or test the final converted object values
            var value = property.GetSourceValue(culture, segment);
            return value != null && (!(value is string) || string.IsNullOrWhiteSpace((string) value) == false);
        }

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
