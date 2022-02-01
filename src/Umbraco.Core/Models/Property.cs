using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        // _values contains all property values, including the invariant-neutral value
        private List<PropertyValue> _values = new List<PropertyValue>();

        // _pvalue contains the invariant-neutral property value
        private PropertyValue _pvalue;

        // _vvalues contains the (indexed) variant property values
        private Dictionary<CompositeNStringNStringKey, PropertyValue> _vvalues;

        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> class.
        /// </summary>
        protected Property()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> class.
        /// </summary>
        public Property(PropertyType propertyType)
        {
            PropertyType = propertyType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> class.
        /// </summary>
        public Property(int id, PropertyType propertyType)
        {
            Id = id;
            PropertyType = propertyType;
        }

        /// <summary>
        /// Represents a property value.
        /// </summary>
        public class PropertyValue : IDeepCloneable, IEquatable<PropertyValue>
        {
            // TODO: Either we allow change tracking at this class level, or we add some special change tracking collections to the Property
            // class to deal with change tracking which variants have changed

            private string _culture;
            private string _segment;

            /// <summary>
            /// Gets or sets the culture of the property.
            /// </summary>
            /// <remarks>The culture is either null (invariant) or a non-empty string. If the property is
            /// set with an empty or whitespace value, its value is converted to null.</remarks>
            public string Culture
            {
                get => _culture;
                internal set => _culture = value.IsNullOrWhiteSpace() ? null : value.ToLowerInvariant();
            }

            /// <summary>
            /// Gets or sets the segment of the property.
            /// </summary>
            /// <remarks>The segment is either null (neutral) or a non-empty string. If the property is
            /// set with an empty or whitespace value, its value is converted to null.</remarks>
            public string Segment
            {
                get => _segment;
                internal set => _segment = value?.ToLowerInvariant();
            }

            /// <summary>
            /// Gets or sets the edited value of the property.
            /// </summary>
            public object EditedValue { get; internal set; }

            /// <summary>
            /// Gets or sets the published value of the property.
            /// </summary>
            public object PublishedValue { get; internal set; }

            /// <summary>
            /// Clones the property value.
            /// </summary>
            public PropertyValue Clone()
                => new PropertyValue { _culture = _culture, _segment = _segment, PublishedValue = PublishedValue, EditedValue = EditedValue };

            public object DeepClone() => Clone();

            public override bool Equals(object obj)
            {
                return Equals(obj as PropertyValue);
            }

            public bool Equals(PropertyValue other)
            {
                return other != null &&
                       _culture == other._culture &&
                       _segment == other._segment &&
                       EqualityComparer<object>.Default.Equals(EditedValue, other.EditedValue) &&
                       EqualityComparer<object>.Default.Equals(PublishedValue, other.PublishedValue);
            }

            public override int GetHashCode()
            {
                var hashCode = 1885328050;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_culture);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_segment);
                hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(EditedValue);
                hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(PublishedValue);
                return hashCode;
            }
        }

        private static readonly DelegateEqualityComparer<object> PropertyValueComparer = new DelegateEqualityComparer<object>(
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
                if (o is IEnumerable && o1 is IEnumerable enumerable)
                    return ((IEnumerable)o).Cast<object>().UnsortedSequenceEqual(enumerable.Cast<object>());

                return o.Equals(o1);
            }, o => o.GetHashCode());

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
                _values = value.Where(x => PropertyType.SupportsVariation(x.Culture, x.Segment)).ToList();
                _pvalue = _values.FirstOrDefault(x => x.Culture == null && x.Segment == null);
                _vvalues = _values.Count > (_pvalue == null ? 0 : 1)
                    ? _values.Where(x => x != _pvalue).ToDictionary(x => new CompositeNStringNStringKey(x.Culture, x.Segment), x => x)
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
        public object GetValue(string culture = null, string segment = null, bool published = false)
        {
            // ensure null or whitespace are nulls
            culture = culture.NullOrWhiteSpaceAsNull();
            segment = segment.NullOrWhiteSpaceAsNull();

            if (!PropertyType.SupportsVariation(culture, segment)) return null;
            if (culture == null && segment == null) return GetPropertyValue(_pvalue, published);
            if (_vvalues == null) return null;
            return _vvalues.TryGetValue(new CompositeNStringNStringKey(culture, segment), out var pvalue)
                ? GetPropertyValue(pvalue, published)
                : null;
        }

        private object GetPropertyValue(PropertyValue pvalue, bool published)
        {
            if (pvalue == null) return null;

            return PropertyType.SupportsPublishing
                ? (published ? pvalue.PublishedValue : pvalue.EditedValue)
                : pvalue.EditedValue;
        }

        // internal - must be invoked by the content item
        // does *not* validate the value - content item must validate first
        internal void PublishValues(string culture = "*", string segment = "*")
        {
            culture = culture.NullOrWhiteSpaceAsNull();
            segment = segment.NullOrWhiteSpaceAsNull();

            // if invariant or all, and invariant-neutral is supported, publish invariant-neutral
            if ((culture == null || culture == "*") && (segment == null || segment == "*") && PropertyType.SupportsVariation(null, null))
                PublishValue(_pvalue);

            // then deal with everything that varies
            if (_vvalues == null) return;

            // get the property values that are still relevant (wrt the property type variation),
            // and match the specified culture and segment (or anything when '*').
            var pvalues = _vvalues.Where(x =>
                    PropertyType.SupportsVariation(x.Value.Culture, x.Value.Segment, true) && // the value variation is ok
                    (culture == "*" || x.Value.Culture.InvariantEquals(culture)) && // the culture matches
                    (segment == "*" || x.Value.Segment.InvariantEquals(segment))) // the segment matches
                .Select(x => x.Value);

            foreach (var pvalue in pvalues)
                PublishValue(pvalue);
        }

        // internal - must be invoked by the content item
        internal void UnpublishValues(string culture = "*", string segment = "*")
        {
            culture = culture.NullOrWhiteSpaceAsNull();
            segment = segment.NullOrWhiteSpaceAsNull();

            // if invariant or all, and invariant-neutral is supported, publish invariant-neutral
            if ((culture == null || culture == "*") && (segment == null || segment == "*") && PropertyType.SupportsVariation(null, null))
                UnpublishValue(_pvalue);

            // then deal with everything that varies
            if (_vvalues == null) return;

            // get the property values that are still relevant (wrt the property type variation),
            // and match the specified culture and segment (or anything when '*').
            var pvalues = _vvalues.Where(x =>
                    PropertyType.SupportsVariation(x.Value.Culture, x.Value.Segment, true) && // the value variation is ok
                    (culture == "*" || x.Value.Culture.InvariantEquals(culture)) && // the culture matches
                    (segment == "*" || x.Value.Segment.InvariantEquals(segment))) // the segment matches
                .Select(x => x.Value);

            foreach (var pvalue in pvalues)
                UnpublishValue(pvalue);
        }

        private void PublishValue(PropertyValue pvalue)
        {
            if (pvalue == null) return;

            if (!PropertyType.SupportsPublishing)
                throw new NotSupportedException("Property type does not support publishing.");
            var origValue = pvalue.PublishedValue;
            pvalue.PublishedValue = PropertyType.ConvertAssignedValue(pvalue.EditedValue);
            DetectChanges(pvalue.EditedValue, origValue, nameof(Values), PropertyValueComparer, false);
        }

        private void UnpublishValue(PropertyValue pvalue)
        {
            if (pvalue == null) return;

            if (!PropertyType.SupportsPublishing)
                throw new NotSupportedException("Property type does not support publishing.");
            var origValue = pvalue.PublishedValue;
            pvalue.PublishedValue = PropertyType.ConvertAssignedValue(null);
            DetectChanges(pvalue.EditedValue, origValue, nameof(Values), PropertyValueComparer, false);
        }

        /// <summary>
        /// Sets a value.
        /// </summary>
        public void SetValue(object value, string culture = null, string segment = null)
        {
            culture = culture.NullOrWhiteSpaceAsNull();
            segment = segment.NullOrWhiteSpaceAsNull();

            if (!PropertyType.SupportsVariation(culture, segment))
                throw new NotSupportedException($"Variation \"{culture??"<null>"},{segment??"<null>"}\" is not supported by the property type.");

            var (pvalue, change) = GetPValue(culture, segment, true);

            var origValue = pvalue.EditedValue;
            var setValue = PropertyType.ConvertAssignedValue(value);

            pvalue.EditedValue = setValue;

            DetectChanges(setValue, origValue, nameof(Values), PropertyValueComparer, change);
        }

        // bypasses all changes detection and is the *only* way to set the published value
        internal void FactorySetValue(string culture, string segment, bool published, object value)
        {
            var (pvalue, _) = GetPValue(culture, segment, true);

            if (published && PropertyType.SupportsPublishing)
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

        private (PropertyValue, bool) GetPValue(string culture, string segment, bool create)
        {
            if (culture == null && segment == null)
                return GetPValue(create);

            var change = false;
            if (_vvalues == null)
            {
                if (!create) return (null, false);
                _vvalues = new Dictionary<CompositeNStringNStringKey, PropertyValue>();
                change = true;
            }
            var k = new CompositeNStringNStringKey(culture, segment);
            if (!_vvalues.TryGetValue(k, out var pvalue))
            {
                if (!create) return (null, false);
                pvalue = _vvalues[k] = new PropertyValue();
                pvalue.Culture = culture;
                pvalue.Segment = segment;
                _values.Add(pvalue);
                change = true;
            }
            return (pvalue, change);
        }

        protected override void PerformDeepClone(object clone)
        {
            base.PerformDeepClone(clone);

            var clonedEntity = (Property)clone;

            //need to manually assign since this is a readonly property
            clonedEntity.PropertyType = (PropertyType) PropertyType.DeepClone();
        }
    }
}
