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
                _values = value;

                _pvalue = value.FirstOrDefault(x => !x.LanguageId.HasValue && x.Segment == null);

                _vvalues = value.ToDictionary(x => (x.LanguageId, x.Segment), x => x);
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
        /// Gets the neutral value.
        /// </summary>
        public object GetValue(bool published = false)
        {
            return _pvalue == null ? null : GetPropertyValue(_pvalue, published);
        }

        /// <summary>
        /// Gets the culture value.
        /// </summary>
        public object GetValue(int? languageId, bool published = false)
        {
            if (_vvalues == null) return null;
            return _vvalues.TryGetValue((languageId, null), out var pvalue)
                ? GetPropertyValue(pvalue, published)
                : null;
        }

        /// <summary>
        /// Gets the segment value.
        /// </summary>
        public object GetValue(string segment, bool published = false)
        {
            if (_vvalues == null) return null;
            return _vvalues.TryGetValue((null, segment), out var pvalue)
                ? GetPropertyValue(pvalue, published)
                : null;
        }

        /// <summary>
        /// Gets the culture+segment value.
        /// </summary>
        public object GetValue(int? languageId, string segment, bool published = false)
        {
            if (_vvalues == null) return null;
            return _vvalues.TryGetValue((languageId, segment), out var pvalue)
                ? GetPropertyValue(pvalue, published)
                : null;
        }

        private object GetPropertyValue(PropertyValue pvalue, bool published)
        {
            return _propertyType.IsPublishing
                ? (published ? pvalue.PublishedValue : pvalue.EditedValue)
                : pvalue.EditedValue;
        }

        internal void PublishValues(int? languageId, string segment)
        {
            (var pvalue, _) = GetPValue(languageId, segment, false);
            if (pvalue == null) return;
            PublishPropertyValue(pvalue);
        }

        internal void PublishAllValues()
        {
            foreach (var pvalue in Values)
                PublishPropertyValue(pvalue);
        }

        private void PublishPropertyValue(PropertyValue pvalue)
        {
            if (!_propertyType.IsPublishing)
                throw new NotSupportedException("Property type does not support publishing.");
            var origValue = pvalue.PublishedValue;
            pvalue.PublishedValue = ConvertSetValue(pvalue.EditedValue);
            DetectChanges(pvalue.EditedValue, origValue, Ps.Value.ValuesSelector, Ps.Value.PropertyValueComparer, false);
        }

        /// <summary>
        /// Sets a (edited) neutral value.
        /// </summary>
        public void SetValue(object value)
        {
            (var pvalue, var change) = GetPValue(true);
            SetPropertyValue(pvalue, value, change);
        }

        /// <summary>
        /// Sets a (edited) culture value.
        /// </summary>
        public void SetValue(int? languageId, object value)
        {
            (var pvalue, var change) = GetPValue(languageId, null, true);
            SetPropertyValue(pvalue, value, change);
        }

        /// <summary>
        /// Sets a (edited) culture value.
        /// </summary>
        public void SetValue(string segment, object value)
        {
            (var pvalue, var change) = GetPValue(null, segment, true);
            SetPropertyValue(pvalue, value, change);
        }

        /// <summary>
        /// Sets a (edited) culture+segment value.
        /// </summary>
        public void SetValue(int? languageId, string segment, object value)
        {
            (var pvalue, var change) = GetPValue(languageId, segment, true);
            SetPropertyValue(pvalue, value, change);
        }

        private void SetPropertyValue(PropertyValue pvalue, object value, bool change)
        {
            var origValue = pvalue.EditedValue;
            var setValue = ConvertSetValue(value);

            pvalue.EditedValue = setValue;

            DetectChanges(setValue, origValue, Ps.Value.ValuesSelector, Ps.Value.PropertyValueComparer, change);
        }

        // bypasses all changes detection and is the *only* to set the published value
        internal void FactorySetValue(int? languageId, string segment, bool published, object value)
        {
            (var pvalue, _) = GetPValue(languageId, segment, true);
            FactorySetPropertyValue(pvalue, published, value);
        }

        private void FactorySetPropertyValue(PropertyValue pvalue, bool published, object value)
        {
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
            if (_vvalues.TryGetValue((languageId, segment), out var pvalue))
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
        /// Gets a value indicating whether the (edit) neutral value is valid.
        /// </summary>
        /// <remarks>An invalid value can be saved, but only valid values can be published.</remarks>
        public bool IsValid()
        {
            return IsValid(GetValue());
        }

        /// <summary>
        /// Gets a value indicating whether the (edit) culture value is valid.
        /// </summary>
        /// <remarks>An invalid value can be saved, but only valid values can be published.</remarks>
        public bool IsValid(int languageId)
        {
            return IsValid(GetValue(languageId));
        }

        /// <summary>
        /// Gets a value indicating whether the (edit) segment value is valid.
        /// </summary>
        /// <remarks>An invalid value can be saved, but only valid values can be published.</remarks>
        public bool IsValue(int languageId, string segment)
        {
            return IsValid(GetValue(languageId, segment));
        }

        /// <summary>
        /// Boolean indicating whether the passed in value is valid
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True is property value is valid, otherwise false</returns>
        private bool IsValid(object value)
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
