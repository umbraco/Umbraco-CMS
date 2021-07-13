using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Provides a default implementation for <see cref="IPropertyValueConverter" />.
    /// </summary>
    /// <seealso cref="Umbraco.Core.PropertyEditors.IPropertyValueConverter" />
    public abstract class PropertyValueConverterBase : IPropertyValueConverter
    {
        /// <inheritdoc />
        public virtual bool IsConverter(IPublishedPropertyType propertyType)
            => false;

        /// <inheritdoc />
        public virtual bool? IsValue(object value, PropertyValueLevel level)
        {
            switch (level)
            {
                case PropertyValueLevel.Source:
                    // the default implementation uses the old magic null & string comparisons,
                    // other implementations may be more clever, and/or test the final converted object values
                    return value != null && (!(value is string stringValue) || !string.IsNullOrWhiteSpace(stringValue));
                case PropertyValueLevel.Inter:
                    return null;
                case PropertyValueLevel.Object:
                    return null;
                default:
                    throw new NotSupportedException($"Invalid level: {level}.");
            }
        }

        [Obsolete("This method is not part of the IPropertyValueConverter contract, therefore not used and will be removed in future versions; use IsValue instead.")]
        public virtual bool HasValue(IPublishedProperty property, string culture, string segment)
        {
            var value = property.GetSourceValue(culture, segment);
            return value != null && (!(value is string stringValue) || !string.IsNullOrWhiteSpace(stringValue));
        }

        /// <inheritdoc />
        public virtual Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(object);

        /// <inheritdoc />
        public virtual PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Snapshot;

        /// <inheritdoc />
        public virtual object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
            => source;

        /// <inheritdoc />
        public virtual object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            => inter;

        /// <inheritdoc />
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
