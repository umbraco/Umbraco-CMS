using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Provides a default overridable implementation for <see cref="IPropertyValueConverter"/> that does nothing.
    /// </summary>
    public abstract class PropertyValueConverterBase : IPropertyValueConverter
    {
        public virtual bool IsConverter(IPublishedPropertyType propertyType)
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

        public virtual Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof (object);

        public virtual PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Snapshot;

        public virtual object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
            => source;

        public virtual object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            => inter;

        public virtual object ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            => inter?.ToString() ?? string.Empty;
    }

    public abstract class PropertyValueConverterBase<TIntermediate, TObject> : PropertyValueConverterBase
    {
        public sealed override bool? IsValue(object value, PropertyValueLevel level)
        {
            switch (level)
            {
                case PropertyValueLevel.Source:
                    return IsSourceValue(value);
                case PropertyValueLevel.Inter:
                    return IsIntermediateValue((TIntermediate)value);
                case PropertyValueLevel.Object:
                    return IsObjectValue((TObject)value);
                default:
                    return base.IsValue(value, level);
            }
        }

        protected virtual bool? IsSourceValue(object value)
            => base.IsValue(value, PropertyValueLevel.Source);

        protected virtual bool? IsIntermediateValue(TIntermediate value)
            => base.IsValue(value, PropertyValueLevel.Inter);

        protected virtual bool? IsObjectValue(TObject value)
            => base.IsValue(value, PropertyValueLevel.Object);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(TObject);

        public sealed override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
            => this.ConvertSourceToIntermediate<object>(owner, propertyType, source, preview);

        protected abstract TIntermediate ConvertSourceToIntermediate<T>(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview);

        public sealed override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            => this.ConvertIntermediateToObject<TIntermediate>(owner, propertyType, referenceCacheLevel, (TIntermediate)inter, preview);

        protected abstract TObject ConvertIntermediateToObject<T>(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, TIntermediate inter, bool preview)
            where T : TIntermediate;

        public sealed override object ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            => this.ConvertIntermediateToXPath<TIntermediate>(owner, propertyType, referenceCacheLevel, (TIntermediate)inter, preview);

        protected virtual object ConvertIntermediateToXPath<T>(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, TIntermediate inter, bool preview)
            where T : TIntermediate
            => base.ConvertIntermediateToXPath(owner, propertyType, referenceCacheLevel, inter, preview);
    }

    public abstract class PropertyValueConverterBase<TValue> : PropertyValueConverterBase<TValue, TValue>
    {
        protected override TValue ConvertIntermediateToObject<T>(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, TValue inter, bool preview)
            => inter;

        protected override bool? IsObjectValue(TValue value)
            => IsIntermediateValue(value);
    }
}
