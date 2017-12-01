using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a property.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Property : Entity
    {
        private PropertyType _propertyType;
        private List<PropertyTagChange> _tagChanges;

        private List<PropertyValue> _values = new List<PropertyValue>();
        private PropertyValue _pvalue;
        private Dictionary<(int?, string), PropertyValue> _vvalues;

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        protected Property()
        { }

        public Property(PropertyType propertyType)
        {
            _propertyType = propertyType;
        }

        public Property(int id, PropertyType propertyType)
        {
            Id = id;
            _propertyType = propertyType;
        }

        public class PropertyValue
        {
            public int? LanguageId { get; internal set; }
            public string Segment { get; internal set; }
            public object EditedValue { get; internal set; }
            public object PublishedValue { get; internal set; }

            public PropertyValue Clone()
                => new PropertyValue { LanguageId = LanguageId, Segment = Segment, PublishedValue = PublishedValue, EditedValue = EditedValue };
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
        public PropertyType PropertyType => _propertyType;

        /// <summary>
        /// Gets the list of values.
        /// </summary>
        [DataMember]
        public List<PropertyValue> Values
        {
            get => _values;
            set
            {
                // make sure we filter out invalid variations
                // make sure we leave _vvalues null if possible
                _values = value.Where(x => _propertyType.ValidateVariation(x.LanguageId, x.Segment, false)).ToList();
                _pvalue = _values.FirstOrDefault(x => !x.LanguageId.HasValue && x.Segment == null);
                _vvalues = _values.Count > (_pvalue == null ? 0 : 1)
                    ? _values.Where(x => x != _pvalue).ToDictionary(x => (x.LanguageId, x.Segment), x => x)
                    : null;
            }
        }

        /// <summary>
        /// Gets the tag changes.
        /// </summary>
        internal List<PropertyTagChange> TagChanges => _tagChanges ?? (_tagChanges = new List<PropertyTagChange>());

        /// <summary>
        /// Gets a value indicating whether the property has tag changes.
        /// </summary>
        internal bool HasTagChanges => _tagChanges != null;

        /// <summary>
        /// Returns the Alias of the PropertyType, which this Property is based on
        /// </summary>
        [DataMember]
        public string Alias => _propertyType.Alias;

        /// <summary>
        /// Returns the Id of the PropertyType, which this Property is based on
        /// </summary>
        [IgnoreDataMember]
        internal int PropertyTypeId => _propertyType.Id;

        /// <summary>
        /// Returns the DatabaseType that the underlaying DataType is using to store its values
        /// </summary>
        /// <remarks>
        /// Only used internally when saving the property value.
        /// </remarks>
        [IgnoreDataMember]
        internal DataTypeDatabaseType DataTypeDatabaseType => _propertyType.DataTypeDatabaseType;

        /// <summary>
        /// Gets the value.
        /// </summary>
        public object GetValue(int? languageId = null, string segment = null, bool published = false)
        {
            if (!_propertyType.ValidateVariation(languageId, segment, false)) return null;
            if (!languageId.HasValue && segment == null) return GetPropertyValue(_pvalue, published);
            if (_vvalues == null) return null;
            return _vvalues.TryGetValue((languageId, segment), out var pvalue)
                ? GetPropertyValue(pvalue, published)
                : null;
        }

        private object GetPropertyValue(PropertyValue pvalue, bool published)
        {
            if (pvalue == null) return null;

            return _propertyType.IsPublishing
                ? (published ? pvalue.PublishedValue : pvalue.EditedValue)
                : pvalue.EditedValue;
        }

        // internal - must be invoked by the content item
        internal void PublishAllValues()
        {
            // throw if some values are not valid
            if (!IsAllValid()) throw new InvalidOperationException("Some values are not valid.");

            // if invariant-neutral is supported, publish invariant-neutral
            if (_propertyType.ValidateVariation(null, null, false))
                PublishPropertyValue(_pvalue);

            // publish everything not invariant-neutral that is supported
            if (_vvalues != null)
            {
                var pvalues = _vvalues
                    .Where(x => _propertyType.ValidateVariation(x.Value.LanguageId, x.Value.Segment, false))
                    .Select(x => x.Value);
                foreach (var pvalue in pvalues)
                    PublishPropertyValue(pvalue);
            }
        }

        // internal - must be invoked by the content item
        internal void PublishValue(int? languageId = null, string segment = null)
        {
            // throw if value is not valid, or if variation is not supported
            if (!IsValid(languageId, segment)) throw new InvalidOperationException("Value is not valid.");
            _propertyType.ValidateVariation(languageId, segment, true);

            (var pvalue, _) = GetPValue(languageId, segment, false);
            if (pvalue == null) return;
            PublishPropertyValue(pvalue);
        }

        // internal - must be invoked by the content item
        internal void PublishCultureValues(int? languageId = null)
        {
            // throw if some values are not valid
            if (!IsCultureValid(languageId)) throw new InvalidOperationException("Some values are not valid.");

            // if invariant and invariant-neutral is supported, publish invariant-neutral
            if (!languageId.HasValue && _propertyType.ValidateVariation(null, null, false))
                PublishPropertyValue(_pvalue);

            // publish everything not invariant-neutral that matches the culture and is supported
            if (_vvalues != null)
            {
                var pvalues = _vvalues
                    .Where(x => x.Value.LanguageId == languageId)
                    .Where(x => _propertyType.ValidateVariation(languageId, x.Value.Segment, false))
                    .Select(x => x.Value);
                foreach (var pvalue in pvalues)
                    PublishPropertyValue(pvalue);
            }
        }

        // internal - must be invoked by the content item
        internal void ClearPublishedAllValues()
        {
            if (_propertyType.ValidateVariation(null, null, false))
                ClearPublishedPropertyValue(_pvalue);

            if (_vvalues != null)
            {
                var pvalues = _vvalues
                    .Where(x => _propertyType.ValidateVariation(x.Value.LanguageId, x.Value.Segment, false))
                    .Select(x => x.Value);
                foreach (var pvalue in pvalues)
                    ClearPublishedPropertyValue(pvalue);
            }
        }

        // internal - must be invoked by the content item
        internal void ClearPublishedValue(int? languageId = null, string segment = null)
        {
            _propertyType.ValidateVariation(languageId, segment, true);
            (var pvalue, _) = GetPValue(languageId, segment, false);
            if (pvalue == null) return;
            ClearPublishedPropertyValue(pvalue);
        }

        // internal - must be invoked by the content item
        internal void ClearPublishedCultureValues(int? languageId = null)
        {
            if (!languageId.HasValue && _propertyType.ValidateVariation(null, null, false))
                ClearPublishedPropertyValue(_pvalue);

            if (_vvalues != null)
            {
                var pvalues = _vvalues
                    .Where(x => x.Value.LanguageId == languageId)
                    .Where(x => _propertyType.ValidateVariation(languageId, x.Value.Segment, false))
                    .Select(x => x.Value);
                foreach (var pvalue in pvalues)
                    ClearPublishedPropertyValue(pvalue);
            }
        }

        private void PublishPropertyValue(PropertyValue pvalue)
        {
            if (pvalue == null) return;

            if (!_propertyType.IsPublishing)
                throw new NotSupportedException("Property type does not support publishing.");
            var origValue = pvalue.PublishedValue;
            pvalue.PublishedValue = ConvertSetValue(pvalue.EditedValue);
            DetectChanges(pvalue.EditedValue, origValue, Ps.Value.ValuesSelector, Ps.Value.PropertyValueComparer, false);
        }

        private void ClearPublishedPropertyValue(PropertyValue pvalue)
        {
            if (pvalue == null) return;

            if (!_propertyType.IsPublishing)
                throw new NotSupportedException("Property type does not support publishing.");
            var origValue = pvalue.PublishedValue;
            pvalue.PublishedValue = ConvertSetValue(null);
            DetectChanges(pvalue.EditedValue, origValue, Ps.Value.ValuesSelector, Ps.Value.PropertyValueComparer, false);
        }

        /// <summary>
        /// Sets a value.
        /// </summary>
        public void SetValue(object value, int? languageId = null, string segment = null)
        {
            _propertyType.ValidateVariation(languageId, segment, true);
            (var pvalue, var change) = GetPValue(languageId, segment, true);

            var origValue = pvalue.EditedValue;
            var setValue = ConvertSetValue(value);

            pvalue.EditedValue = setValue;

            DetectChanges(setValue, origValue, Ps.Value.ValuesSelector, Ps.Value.PropertyValueComparer, change);
        }

        // bypasses all changes detection and is the *only* way to set the published value
        internal void FactorySetValue(int? languageId, string segment, bool published, object value)
        {
            (var pvalue, _) = GetPValue(languageId, segment, true);

            if (published && _propertyType.IsPublishing)
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
                _vvalues = new Dictionary<(int?, string), PropertyValue>();
                change = true;
            }
            if (!_vvalues.TryGetValue((languageId, segment), out var pvalue))
            {
                if (!create) return (null, false);
                pvalue = _vvalues[(languageId, segment)] = new PropertyValue();
                pvalue.LanguageId = languageId;
                pvalue.Segment = segment;
                _values.Add(pvalue);
                change = true;
            }
            return (pvalue, change);
        }

        private object ConvertSetValue(object value)
        {
            var isOfExpectedType = _propertyType.IsPropertyTypeValid(value);

            if (isOfExpectedType)
                return value;

            // isOfExpectedType is true if value is null - so if false, value is *not* null
            // "garbage-in", accept what we can & convert
            // throw only if conversion is not possible

            var s = value.ToString();

            switch (_propertyType.DataTypeDatabaseType)
            {
                case DataTypeDatabaseType.Nvarchar:
                case DataTypeDatabaseType.Ntext:
                    return s;

                case DataTypeDatabaseType.Integer:
                    if (s.IsNullOrWhiteSpace())
                        return null; // assume empty means null
                    var convInt = value.TryConvertTo<int>();
                    if (convInt == false) ThrowTypeException(value, typeof(int), _propertyType.Alias);
                    return convInt.Result;

                case DataTypeDatabaseType.Decimal:
                    if (s.IsNullOrWhiteSpace())
                        return null; // assume empty means null
                    var convDecimal = value.TryConvertTo<decimal>();
                    if (convDecimal == false) ThrowTypeException(value, typeof(decimal), _propertyType.Alias);
                    // need to normalize the value (change the scaling factor and remove trailing zeroes)
                    // because the underlying database is going to mess with the scaling factor anyways.
                    return convDecimal.Result.Normalize();

                case DataTypeDatabaseType.Date:
                    if (s.IsNullOrWhiteSpace())
                        return null; // assume empty means null
                    var convDateTime = value.TryConvertTo<DateTime>();
                    if (convDateTime == false) ThrowTypeException(value, typeof(DateTime), _propertyType.Alias);
                    return convDateTime.Result;
            }

            return value;
        }

        private static void ThrowTypeException(object value, Type expected, string alias)
        {
            throw new InvalidOperationException($"Cannot assign value \"{value}\" of type \"{value.GetType()}\" to property \"{alias}\" expecting type \"{expected}\".");
        }

        /// <summary>
        /// Gets a value indicating whether everything is valid.
        /// </summary>
        /// <returns></returns>
        public bool IsAllValid()
        {
            // if invariant-neutral is supported, validate invariant-neutral
            if (_propertyType.ValidateVariation(null, null, false))
                if (!IsValidValue(_pvalue)) return false;

            if (_vvalues == null) return IsValidValue(null);

            // validate everything not invariant-neutral that is supported
            // fixme - broken - how can we figure out what is mandatory here?

            var pvalues = _vvalues
                .Where(x => _propertyType.ValidateVariation(x.Value.LanguageId, x.Value.Segment, false))
                .Select(x => x.Value)
                .ToArray();
            return pvalues.Length == 0
                ? IsValidValue(null)
                : pvalues.All(x => IsValidValue(x.EditedValue));
        }

        /// <summary>
        /// Gets a value indicating whether the culture/any values are valid.
        /// </summary>
        /// <remarks>An invalid value can be saved, but only valid values can be published.</remarks>
        public bool IsCultureValid(int? languageId)
        {
            // if invariant and invariant-neutral is supported, validate invariant-neutral
            if (!languageId.HasValue && _propertyType.ValidateVariation(null, null, false))
                if (!IsValidValue(_pvalue)) return false;

            // validate everything not invariant-neutral that matches the culture and is supported
            // fixme - broken - how can we figure out what is mandatory here?

            if (_vvalues == null) return IsValidValue(null);

            var pvalues = _vvalues
                .Where(x => x.Value.LanguageId == languageId)
                .Where(x => _propertyType.ValidateVariation(languageId, x.Value.Segment, false))
                .Select(x => x.Value)
                .ToArray();
            return pvalues.Length == 0
                ? IsValidValue(null)
                : pvalues.All(x => IsValidValue(x.EditedValue));
        }

        /// <summary>
        /// Gets a value indicating whether the value is valid.
        /// </summary>
        /// <remarks>An invalid value can be saved, but only valid values can be published.</remarks>
        public bool IsValid(int? languageId = null, string segment = null)
        {
            return IsValidValue(GetValue(languageId, segment));
        }

        /// <summary>
        /// Boolean indicating whether the passed in value is valid
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True is property value is valid, otherwise false</returns>
        private bool IsValidValue(object value)
        {
            return _propertyType.IsValidPropertyValue(value);
        }

        public override object DeepClone()
        {
            var clone = (Property) base.DeepClone();

            //turn off change tracking
            clone.DisableChangeTracking();

            //need to manually assign since this is a readonly property
            clone._propertyType = (PropertyType) PropertyType.DeepClone();

            //re-enable tracking
            clone.ResetDirtyProperties(false); // not needed really, since we're not tracking
            clone.EnableChangeTracking();

            return clone;
        }
    }
}
