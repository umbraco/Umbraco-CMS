using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Collections;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a property.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Property : EntityBase
    {
        private List<PropertyValue> _values = new List<PropertyValue>();
        private PropertyValue _pvalue;
        private Dictionary<CompositeIntStringKey, PropertyValue> _vvalues;

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        protected Property()
        { }

        public Property(PropertyType propertyType)
        {
            PropertyType = propertyType;
        }

        public Property(int id, PropertyType propertyType)
        {
            Id = id;
            PropertyType = propertyType;
        }

        public class PropertyValue
        {
            private string _segment;

            public int? LanguageId { get; internal set; }
            public string Segment
            {
                get => _segment;
                internal set => _segment = value?.ToLowerInvariant();
            }
            public object EditedValue { get; internal set; }
            public object PublishedValue { get; internal set; }

            public PropertyValue Clone()
                => new PropertyValue { LanguageId = LanguageId, _segment = _segment, PublishedValue = PublishedValue, EditedValue = EditedValue };
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo ValuesSelector = ExpressionHelper.GetPropertyInfo<Property, object>(x => x.Values);

            public readonly DelegateEqualityComparer<object> PropertyValueComparer = new DelegateEqualityComparer<object>(
                (o, o1) =>
                {
                    if (o == null && o1 == null) return true;

                    // custom comparer for strings.
                    // if one is null and another is empty then they are the same
                    if (o is string || o1 is string)
                        return ((o as string).IsNullOrWhiteSpace() && (o1 as string).IsNullOrWhiteSpace()) || (o != null && o1 != null && o.Equals(o1));

                    if (o == null || o1 == null) return false;

                    // custom comparer for enumerable
                    // ReSharper disable once MergeCastWithTypeCheck
                    if (o is IEnumerable && o1 is IEnumerable)
                        return ((IEnumerable) o).Cast<object>().UnsortedSequenceEqual(((IEnumerable) o1).Cast<object>());

                    return o.Equals(o1);
                }, o => o.GetHashCode());
        }

        /// <summary>
        /// Returns the PropertyType, which this Property is based on
        /// </summary>
        [IgnoreDataMember]
        public PropertyType PropertyType { get; private set; }

        /// <summary>
        /// Gets the list of values.
        /// </summary>
        [DataMember]
        public IReadOnlyCollection<PropertyValue> Values
        {
            get => _values;
            set
            {
                // make sure we filter out invalid variations
                // make sure we leave _vvalues null if possible
                _values = value.Where(x => PropertyType.ValidateVariation(x.LanguageId, x.Segment, false)).ToList();
                _pvalue = _values.FirstOrDefault(x => !x.LanguageId.HasValue && x.Segment == null);
                _vvalues = _values.Count > (_pvalue == null ? 0 : 1)
                    ? _values.Where(x => x != _pvalue).ToDictionary(x => new CompositeIntStringKey(x.LanguageId, x.Segment), x => x)
                    : null;
            }
        }

        /// <summary>
        /// Returns the Alias of the PropertyType, which this Property is based on
        /// </summary>
        [DataMember]
        public string Alias => PropertyType.Alias;

        /// <summary>
        /// Returns the Id of the PropertyType, which this Property is based on
        /// </summary>
        [IgnoreDataMember]
        internal int PropertyTypeId => PropertyType.Id;

        /// <summary>
        /// Returns the DatabaseType that the underlaying DataType is using to store its values
        /// </summary>
        /// <remarks>
        /// Only used internally when saving the property value.
        /// </remarks>
        [IgnoreDataMember]
        internal ValueStorageType ValueStorageType => PropertyType.ValueStorageType;

        /// <summary>
        /// Gets the value.
        /// </summary>
        public object GetValue(int? languageId = null, string segment = null, bool published = false)
        {
            if (!PropertyType.ValidateVariation(languageId, segment, false)) return null;
            if (!languageId.HasValue && segment == null) return GetPropertyValue(_pvalue, published);
            if (_vvalues == null) return null;
            return _vvalues.TryGetValue(new CompositeIntStringKey(languageId, segment), out var pvalue)
                ? GetPropertyValue(pvalue, published)
                : null;
        }

        private object GetPropertyValue(PropertyValue pvalue, bool published)
        {
            if (pvalue == null) return null;

            return PropertyType.IsPublishing
                ? (published ? pvalue.PublishedValue : pvalue.EditedValue)
                : pvalue.EditedValue;
        }

        // internal - must be invoked by the content item
        // does *not* validate the value - content item must validate first
        internal void PublishAllValues()
        {
            // if invariant-neutral is supported, publish invariant-neutral
            if (PropertyType.ValidateVariation(null, null, false))
                PublishPropertyValue(_pvalue);

            // publish everything not invariant-neutral that is supported
            if (_vvalues != null)
            {
                var pvalues = _vvalues
                    .Where(x => PropertyType.ValidateVariation(x.Value.LanguageId, x.Value.Segment, false))
                    .Select(x => x.Value);
                foreach (var pvalue in pvalues)
                    PublishPropertyValue(pvalue);
            }
        }

        // internal - must be invoked by the content item
        // does *not* validate the value - content item must validate first
        internal void PublishValue(int? languageId = null, string segment = null)
        {
            PropertyType.ValidateVariation(languageId, segment, true);

            (var pvalue, _) = GetPValue(languageId, segment, false);
            if (pvalue == null) return;
            PublishPropertyValue(pvalue);
        }

        // internal - must be invoked by the content item
        // does *not* validate the value - content item must validate first
        internal void PublishCultureValues(int? languageId = null)
        {
            // if invariant and invariant-neutral is supported, publish invariant-neutral
            if (!languageId.HasValue && PropertyType.ValidateVariation(null, null, false))
                PublishPropertyValue(_pvalue);

            // publish everything not invariant-neutral that matches the culture and is supported
            if (_vvalues != null)
            {
                var pvalues = _vvalues
                    .Where(x => x.Value.LanguageId == languageId)
                    .Where(x => PropertyType.ValidateVariation(languageId, x.Value.Segment, false))
                    .Select(x => x.Value);
                foreach (var pvalue in pvalues)
                    PublishPropertyValue(pvalue);
            }
        }

        // internal - must be invoked by the content item
        internal void ClearPublishedAllValues()
        {
            if (PropertyType.ValidateVariation(null, null, false))
                ClearPublishedPropertyValue(_pvalue);

            if (_vvalues != null)
            {
                var pvalues = _vvalues
                    .Where(x => PropertyType.ValidateVariation(x.Value.LanguageId, x.Value.Segment, false))
                    .Select(x => x.Value);
                foreach (var pvalue in pvalues)
                    ClearPublishedPropertyValue(pvalue);
            }
        }

        // internal - must be invoked by the content item
        internal void ClearPublishedValue(int? languageId = null, string segment = null)
        {
            PropertyType.ValidateVariation(languageId, segment, true);
            (var pvalue, _) = GetPValue(languageId, segment, false);
            if (pvalue == null) return;
            ClearPublishedPropertyValue(pvalue);
        }

        // internal - must be invoked by the content item
        internal void ClearPublishedCultureValues(int? languageId = null)
        {
            if (!languageId.HasValue && PropertyType.ValidateVariation(null, null, false))
                ClearPublishedPropertyValue(_pvalue);

            if (_vvalues != null)
            {
                var pvalues = _vvalues
                    .Where(x => x.Value.LanguageId == languageId)
                    .Where(x => PropertyType.ValidateVariation(languageId, x.Value.Segment, false))
                    .Select(x => x.Value);
                foreach (var pvalue in pvalues)
                    ClearPublishedPropertyValue(pvalue);
            }
        }

        private void PublishPropertyValue(PropertyValue pvalue)
        {
            if (pvalue == null) return;

            if (!PropertyType.IsPublishing)
                throw new NotSupportedException("Property type does not support publishing.");
            var origValue = pvalue.PublishedValue;
            pvalue.PublishedValue = PropertyType.ConvertAssignedValue(pvalue.EditedValue);
            DetectChanges(pvalue.EditedValue, origValue, Ps.Value.ValuesSelector, Ps.Value.PropertyValueComparer, false);
        }

        private void ClearPublishedPropertyValue(PropertyValue pvalue)
        {
            if (pvalue == null) return;

            if (!PropertyType.IsPublishing)
                throw new NotSupportedException("Property type does not support publishing.");
            var origValue = pvalue.PublishedValue;
            pvalue.PublishedValue = PropertyType.ConvertAssignedValue(null);
            DetectChanges(pvalue.EditedValue, origValue, Ps.Value.ValuesSelector, Ps.Value.PropertyValueComparer, false);
        }

        /// <summary>
        /// Sets a value.
        /// </summary>
        public void SetValue(object value, int? languageId = null, string segment = null)
        {
            PropertyType.ValidateVariation(languageId, segment, true);
            (var pvalue, var change) = GetPValue(languageId, segment, true);

            var origValue = pvalue.EditedValue;
            var setValue = PropertyType.ConvertAssignedValue(value);

            pvalue.EditedValue = setValue;

            DetectChanges(setValue, origValue, Ps.Value.ValuesSelector, Ps.Value.PropertyValueComparer, change);
        }

        // bypasses all changes detection and is the *only* way to set the published value
        internal void FactorySetValue(int? languageId, string segment, bool published, object value)
        {
            (var pvalue, _) = GetPValue(languageId, segment, true);

            if (published && PropertyType.IsPublishing)
                pvalue.PublishedValue = value;
            else
                pvalue.EditedValue = value;
        }

        private (PropertyValue, bool) GetPValue(bool create)
        {
            var change = false;
            if (_pvalue == null)
            {
                if (!create) return (null, false);
                _pvalue = new PropertyValue();
                _values.Add(_pvalue);
                change = true;
            }
            return (_pvalue, change);
        }

        private (PropertyValue, bool) GetPValue(int? languageId, string segment, bool create)
        {
            if (!languageId.HasValue && segment == null)
                return GetPValue(create);

            var change = false;
            if (_vvalues == null)
            {
                if (!create) return (null, false);
                _vvalues = new Dictionary<CompositeIntStringKey, PropertyValue>();
                change = true;
            }
            var k = new CompositeIntStringKey(languageId, segment);
            if (!_vvalues.TryGetValue(k, out var pvalue))
            {
                if (!create) return (null, false);
                pvalue = _vvalues[k] = new PropertyValue();
                pvalue.LanguageId = languageId;
                pvalue.Segment = segment;
                _values.Add(pvalue);
                change = true;
            }
            return (pvalue, change);
        }

        /// <summary>
        /// Gets a value indicating whether everything is valid.
        /// </summary>
        /// <returns></returns>
        public bool IsAllValid()
        {
            // invariant-neutral is supported, validate invariant-neutral
            // includes mandatory validation
            if (PropertyType.ValidateVariation(null, null, false) && !IsValidValue(_pvalue)) return false;

            // either invariant-neutral is not supported, or it is valid
            // for anything else, validate the existing values (including mandatory),
            // but we cannot validate mandatory globally (we don't know the possible cultures and segments)

            if (_vvalues == null) return true;

            var pvalues = _vvalues
                .Where(x => PropertyType.ValidateVariation(x.Value.LanguageId, x.Value.Segment, false))
                .Select(x => x.Value)
                .ToArray();

            return pvalues.Length == 0 || pvalues.All(x => IsValidValue(x.EditedValue));
        }

        /// <summary>
        /// Gets a value indicating whether the culture/any values are valid.
        /// </summary>
        /// <remarks>An invalid value can be saved, but only valid values can be published.</remarks>
        public bool IsCultureValid(int? languageId)
        {
            // culture-neutral is supported, validate culture-neutral
            // includes mandatory validation
            if (PropertyType.ValidateVariation(languageId, null, false) && !IsValidValue(GetValue(languageId)))
                return false;

            // either culture-neutral is not supported, or it is valid
            // for anything non-neutral, validate the existing values (including mandatory),
            // but we cannot validate mandatory globally (we don't know the possible segments)

            if (_vvalues == null) return true;

            var pvalues = _vvalues
                .Where(x => x.Value.LanguageId == languageId)
                .Where(x => PropertyType.ValidateVariation(languageId, x.Value.Segment, false))
                .Select(x => x.Value)
                .ToArray();

            return pvalues.Length == 0 || pvalues.All(x => IsValidValue(x.EditedValue));
        }

        /// <summary>
        /// Gets a value indicating whether the value is valid.
        /// </summary>
        /// <remarks>An invalid value can be saved, but only valid values can be published.</remarks>
        public bool IsValid(int? languageId = null, string segment = null)
        {
            // single value -> validates mandatory
            return IsValidValue(GetValue(languageId, segment));
        }

        /// <summary>
        /// Boolean indicating whether the passed in value is valid
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True is property value is valid, otherwise false</returns>
        private bool IsValidValue(object value)
        {
            return PropertyType.IsPropertyValueValid(value);
        }

        public override object DeepClone()
        {
            var clone = (Property) base.DeepClone();

            //turn off change tracking
            clone.DisableChangeTracking();

            //need to manually assign since this is a readonly property
            clone.PropertyType = (PropertyType) PropertyType.DeepClone();

            //re-enable tracking
            clone.ResetDirtyProperties(false); // not needed really, since we're not tracking
            clone.EnableChangeTracking();

            return clone;
        }
    }
}
