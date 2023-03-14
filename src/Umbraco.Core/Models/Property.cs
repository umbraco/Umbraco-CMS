using System.Collections;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a property.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class Property : EntityBase, IProperty
{
    private static readonly DelegateEqualityComparer<object?> PropertyValueComparer = new(
        (o, o1) =>
        {
            if (o == null && o1 == null)
            {
                return true;
            }

            // custom comparer for strings.
            // if one is null and another is empty then they are the same
            if (o is string || o1 is string)
            {
                return ((o as string).IsNullOrWhiteSpace() && (o1 as string).IsNullOrWhiteSpace()) ||
                       (o != null && o1 != null && o.Equals(o1));
            }

            if (o == null || o1 == null)
            {
                return false;
            }

            // custom comparer for enumerable
            // ReSharper disable once MergeCastWithTypeCheck
            if (o is IEnumerable && o1 is IEnumerable enumerable)
            {
                return ((IEnumerable)o).Cast<object>().UnsortedSequenceEqual(enumerable.Cast<object>());
            }

            return o.Equals(o1);
        },
        o => o!.GetHashCode());

    // _pvalue contains the invariant-neutral property value
    private IPropertyValue? _pvalue;

    // _values contains all property values, including the invariant-neutral value
    private List<IPropertyValue> _values = new();

    // _vvalues contains the (indexed) variant property values
    private Dictionary<CompositeNStringNStringKey, IPropertyValue>? _vvalues;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Property" /> class.
    /// </summary>
    public Property(IPropertyType propertyType) => PropertyType = propertyType;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Property" /> class.
    /// </summary>
    public Property(int id, IPropertyType propertyType)
    {
        Id = id;
        PropertyType = propertyType;
    }

    /// <summary>
    ///     Returns the PropertyType, which this Property is based on
    /// </summary>
    [IgnoreDataMember]
    public IPropertyType PropertyType { get; private set; }

    /// <summary>
    ///     Gets the list of values.
    /// </summary>
    [DataMember]
    public IReadOnlyCollection<IPropertyValue> Values
    {
        get => _values;
        set
        {
            // make sure we filter out invalid variations
            // make sure we leave _vvalues null if possible
            _values = value.Where(x => PropertyType?.SupportsVariation(x.Culture, x.Segment) ?? false).ToList();
            _pvalue = _values.FirstOrDefault(x => x.Culture == null && x.Segment == null);
            _vvalues = _values.Count > (_pvalue == null ? 0 : 1)
                ? _values.Where(x => x != _pvalue)
                    .ToDictionary(x => new CompositeNStringNStringKey(x.Culture, x.Segment), x => x)
                : null;
        }
    }

    /// <summary>
    ///     Returns the Alias of the PropertyType, which this Property is based on
    /// </summary>
    [DataMember]
    public string Alias => PropertyType.Alias;

    /// <summary>
    ///     Returns the Id of the PropertyType, which this Property is based on
    /// </summary>
    [IgnoreDataMember]
    public int PropertyTypeId => PropertyType.Id;

    /// <summary>
    ///     Returns the DatabaseType that the underlaying DataType is using to store its values
    /// </summary>
    /// <remarks>
    ///     Only used internally when saving the property value.
    /// </remarks>
    [IgnoreDataMember]
    public ValueStorageType ValueStorageType => PropertyType.ValueStorageType;

    /// <summary>
    ///     Creates a new <see cref="Property" /> instance for existing <see cref="IProperty" />
    /// </summary>
    /// <param name="id"></param>
    /// <param name="propertyType"></param>
    /// <param name="values">
    ///     Generally will contain a published and an unpublished property values
    /// </param>
    /// <returns></returns>
    public static Property CreateWithValues(int id, IPropertyType propertyType, params InitialPropertyValue[] values)
    {
        var property = new Property(propertyType);
        try
        {
            property.DisableChangeTracking();
            property.Id = id;
            foreach (InitialPropertyValue value in values)
            {
                property.FactorySetValue(value.Culture, value.Segment, value.Published, value.Value);
            }

            property.ResetDirtyProperties(false);
            return property;
        }
        finally
        {
            property.EnableChangeTracking();
        }
    }

    /// <summary>
    ///     Gets the value.
    /// </summary>
    public object? GetValue(string? culture = null, string? segment = null, bool published = false)
    {
        // ensure null or whitespace are nulls
        culture = culture?.NullOrWhiteSpaceAsNull();
        segment = segment?.NullOrWhiteSpaceAsNull();

        if (!PropertyType.SupportsVariation(culture, segment))
        {
            return null;
        }

        if (culture == null && segment == null)
        {
            return GetPropertyValue(_pvalue, published);
        }

        if (_vvalues == null)
        {
            return null;
        }

        return _vvalues.TryGetValue(new CompositeNStringNStringKey(culture, segment), out IPropertyValue? pvalue)
            ? GetPropertyValue(pvalue, published)
            : null;
    }

    // internal - must be invoked by the content item
    // does *not* validate the value - content item must validate first
    public void PublishValues(string? culture = "*", string? segment = "*")
    {
        culture = culture?.NullOrWhiteSpaceAsNull();
        segment = segment?.NullOrWhiteSpaceAsNull();

        // if invariant or all, and invariant-neutral is supported, publish invariant-neutral
        if ((culture == null || culture == "*") && (segment == null || segment == "*") &&
            PropertyType.SupportsVariation(null, null))
        {
            PublishValue(_pvalue);
        }

        // then deal with everything that varies
        if (_vvalues == null)
        {
            return;
        }

        // get the property values that are still relevant (wrt the property type variation),
        // and match the specified culture and segment (or anything when '*').
        IEnumerable<IPropertyValue> pvalues = _vvalues.Where(x =>
                PropertyType.SupportsVariation(x.Value.Culture, x.Value.Segment, true) && // the value variation is ok
                    (culture == "*" || x.Value.Culture.InvariantEquals(culture)) && // the culture matches
                    (segment == "*" || x.Value.Segment.InvariantEquals(segment))) // the segment matches
            .Select(x => x.Value);

        foreach (IPropertyValue pvalue in pvalues)
        {
            PublishValue(pvalue);
        }
    }

    // internal - must be invoked by the content item
    public void UnpublishValues(string? culture = "*", string? segment = "*")
    {
        culture = culture?.NullOrWhiteSpaceAsNull();
        segment = segment?.NullOrWhiteSpaceAsNull();

        // if invariant or all, and invariant-neutral is supported, publish invariant-neutral
        if ((culture == null || culture == "*") && (segment == null || segment == "*") &&
            PropertyType.SupportsVariation(null, null))
        {
            UnpublishValue(_pvalue);
        }

        // then deal with everything that varies
        if (_vvalues == null)
        {
            return;
        }

        // get the property values that are still relevant (wrt the property type variation),
        // and match the specified culture and segment (or anything when '*').
        IEnumerable<IPropertyValue> pvalues = _vvalues.Where(x =>
                PropertyType.SupportsVariation(x.Value.Culture, x.Value.Segment, true) && // the value variation is ok
                (culture == "*" || (x.Value.Culture?.InvariantEquals(culture) ?? false)) && // the culture matches
                (segment == "*" || (x.Value.Segment?.InvariantEquals(segment) ?? false))) // the segment matches
            .Select(x => x.Value);

        foreach (IPropertyValue pvalue in pvalues)
        {
            UnpublishValue(pvalue);
        }
    }

    /// <summary>
    ///     Sets a value.
    /// </summary>
    public void SetValue(object? value, string? culture = null, string? segment = null)
    {
        culture = culture?.NullOrWhiteSpaceAsNull();
        segment = segment?.NullOrWhiteSpaceAsNull();

        if (!PropertyType.SupportsVariation(culture, segment))
        {
            throw new NotSupportedException(
                $"Variation \"{culture ?? "<null>"},{segment ?? "<null>"}\" is not supported by the property type.");
        }

        (IPropertyValue? pvalue, var change) = GetPValue(culture, segment, true);

        if (pvalue is not null)
        {
            var origValue = pvalue.EditedValue;
            var setValue = ConvertAssignedValue(value);

            pvalue.EditedValue = setValue;

            DetectChanges(setValue, origValue, nameof(Values), PropertyValueComparer, change);
        }
    }

    public object? ConvertAssignedValue(object? value) =>
        TryConvertAssignedValue(value, true, out var converted) ? converted : null;

    protected override void PerformDeepClone(object clone)
    {
        base.PerformDeepClone(clone);

        var clonedEntity = (Property)clone;

        // need to manually assign since this is a readonly property
        clonedEntity.PropertyType = (PropertyType)PropertyType.DeepClone();
    }

    private object? GetPropertyValue(IPropertyValue? pvalue, bool published)
    {
        if (pvalue == null)
        {
            return null;
        }

        return PropertyType.SupportsPublishing
            ? published ? pvalue.PublishedValue : pvalue.EditedValue
            : pvalue.EditedValue;
    }

    private void PublishValue(IPropertyValue? pvalue)
    {
        if (pvalue == null)
        {
            return;
        }

        if (!PropertyType.SupportsPublishing)
        {
            throw new NotSupportedException("Property type does not support publishing.");
        }

        var origValue = pvalue.PublishedValue;
        pvalue.PublishedValue = ConvertAssignedValue(pvalue.EditedValue);
        DetectChanges(pvalue.EditedValue, origValue, nameof(Values), PropertyValueComparer, false);
    }

    private void UnpublishValue(IPropertyValue? pvalue)
    {
        if (pvalue == null)
        {
            return;
        }

        if (!PropertyType.SupportsPublishing)
        {
            throw new NotSupportedException("Property type does not support publishing.");
        }

        var origValue = pvalue.PublishedValue;
        pvalue.PublishedValue = ConvertAssignedValue(null);
        DetectChanges(pvalue.EditedValue, origValue, nameof(Values), PropertyValueComparer, false);
    }

    // bypasses all changes detection and is the *only* way to set the published value
    private void FactorySetValue(string? culture, string? segment, bool published, object? value)
    {
        (IPropertyValue? pvalue, _) = GetPValue(culture, segment, true);

        if (pvalue is not null)
        {
            if (published && PropertyType.SupportsPublishing)
            {
                pvalue.PublishedValue = value;
            }
            else
            {
                pvalue.EditedValue = value;
            }
        }
    }

    private (IPropertyValue?, bool) GetPValue(bool create)
    {
        var change = false;
        if (_pvalue == null)
        {
            if (!create)
            {
                return (null, false);
            }

            _pvalue = new PropertyValue();
            _values.Add(_pvalue);
            change = true;
        }

        return (_pvalue, change);
    }

    private (IPropertyValue?, bool) GetPValue(string? culture, string? segment, bool create)
    {
        if (culture == null && segment == null)
        {
            return GetPValue(create);
        }

        var change = false;
        if (_vvalues == null)
        {
            if (!create)
            {
                return (null, false);
            }

            _vvalues = new Dictionary<CompositeNStringNStringKey, IPropertyValue>();
            change = true;
        }

        var k = new CompositeNStringNStringKey(culture, segment);
        if (!_vvalues.TryGetValue(k, out IPropertyValue? pvalue))
        {
            if (!create)
            {
                return (null, false);
            }

            pvalue = _vvalues[k] = new PropertyValue();
            pvalue.Culture = culture;
            pvalue.Segment = segment;
            _values.Add(pvalue);
            change = true;
        }

        return (pvalue, change);
    }

    private static void ThrowTypeException(object? value, Type expected, string alias) =>
        throw new InvalidOperationException(
            $"Cannot assign value \"{value}\" of type \"{value?.GetType()}\" to property \"{alias}\" expecting type \"{expected}\".");

    /// <summary>
    ///     Tries to convert a value assigned to a property.
    /// </summary>
    /// <remarks>
    ///     <para></para>
    /// </remarks>
    private bool TryConvertAssignedValue(object? value, bool throwOnError, out object? converted)
    {
        var isOfExpectedType = IsOfExpectedPropertyType(value);
        if (isOfExpectedType)
        {
            converted = value;
            return true;
        }

        // isOfExpectedType is true if value is null - so if false, value is *not* null
        // "garbage-in", accept what we can & convert
        // throw only if conversion is not possible
        var s = value?.ToString();
        converted = null;

        switch (ValueStorageType)
        {
            case ValueStorageType.Nvarchar:
            case ValueStorageType.Ntext:
            {
                converted = s;
                return true;
            }

            case ValueStorageType.Integer:
                if (s.IsNullOrWhiteSpace())
                {
                    return true; // assume empty means null
                }

                Attempt<int> convInt = value.TryConvertTo<int>();
                if (convInt.Success)
                {
                    converted = convInt.Result;
                    return true;
                }

                if (throwOnError)
                {
                    ThrowTypeException(value, typeof(int), Alias);
                }

                return false;

            case ValueStorageType.Decimal:
                if (s.IsNullOrWhiteSpace())
                {
                    return true; // assume empty means null
                }

                Attempt<decimal> convDecimal = value.TryConvertTo<decimal>();
                if (convDecimal.Success)
                {
                    // need to normalize the value (change the scaling factor and remove trailing zeros)
                    // because the underlying database is going to mess with the scaling factor anyways.
                    converted = convDecimal.Result.Normalize();
                    return true;
                }

                if (throwOnError)
                {
                    ThrowTypeException(value, typeof(decimal), Alias);
                }

                return false;

            case ValueStorageType.Date:
                if (s.IsNullOrWhiteSpace())
                {
                    return true; // assume empty means null
                }

                Attempt<DateTime> convDateTime = value.TryConvertTo<DateTime>();
                if (convDateTime.Success)
                {
                    converted = convDateTime.Result;
                    return true;
                }

                if (throwOnError)
                {
                    ThrowTypeException(value, typeof(DateTime), Alias);
                }

                return false;

            default:
                throw new NotSupportedException($"Not supported storage type \"{ValueStorageType}\".");
        }
    }

    /// <summary>
    ///     Determines whether a value is of the expected type for this property type.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If the value is of the expected type, it can be directly assigned to the property.
    ///         Otherwise, some conversion is required.
    ///     </para>
    /// </remarks>
    private bool IsOfExpectedPropertyType(object? value)
    {
        // null values are assumed to be ok
        if (value == null)
        {
            return true;
        }

        // check if the type of the value matches the type from the DataType/PropertyEditor
        // then it can be directly assigned, anything else requires conversion
        Type valueType = value.GetType();
        switch (ValueStorageType)
        {
            case ValueStorageType.Integer:
                return valueType == typeof(int);
            case ValueStorageType.Decimal:
                return valueType == typeof(decimal);
            case ValueStorageType.Date:
                return valueType == typeof(DateTime);
            case ValueStorageType.Nvarchar:
                return valueType == typeof(string);
            case ValueStorageType.Ntext:
                return valueType == typeof(string);
            default:
                throw new NotSupportedException($"Not supported storage type \"{ValueStorageType}\".");
        }
    }

    /// <summary>
    ///     Used for constructing a new <see cref="Property" /> instance
    /// </summary>
    public class InitialPropertyValue
    {
        public InitialPropertyValue(string? culture, string? segment, bool published, object? value)
        {
            Culture = culture;
            Segment = segment;
            Published = published;
            Value = value;
        }

        public string? Culture { get; }

        public string? Segment { get; }

        public bool Published { get; }

        public object? Value { get; }
    }

    /// <summary>
    ///     Represents a property value.
    /// </summary>
    public class PropertyValue : IPropertyValue, IDeepCloneable, IEquatable<PropertyValue>
    {
        // TODO: Either we allow change tracking at this class level, or we add some special change tracking collections to the Property
        // class to deal with change tracking which variants have changed
        private string? _culture;
        private string? _segment;

        /// <summary>
        ///     Gets or sets the culture of the property.
        /// </summary>
        /// <remarks>
        ///     The culture is either null (invariant) or a non-empty string. If the property is
        ///     set with an empty or whitespace value, its value is converted to null.
        /// </remarks>
        public string? Culture
        {
            get => _culture;
            set => _culture = value.IsNullOrWhiteSpace() ? null : value!.ToLowerInvariant();
        }

        public object DeepClone() => Clone();

        public bool Equals(PropertyValue? other) =>
            other != null &&
            _culture == other._culture &&
            _segment == other._segment &&
            EqualityComparer<object>.Default.Equals(EditedValue, other.EditedValue) &&
            EqualityComparer<object>.Default.Equals(PublishedValue, other.PublishedValue);

        /// <summary>
        ///     Gets or sets the segment of the property.
        /// </summary>
        /// <remarks>
        ///     The segment is either null (neutral) or a non-empty string. If the property is
        ///     set with an empty or whitespace value, its value is converted to null.
        /// </remarks>
        public string? Segment
        {
            get => _segment;
            set => _segment = value?.ToLowerInvariant();
        }

        /// <summary>
        ///     Gets or sets the edited value of the property.
        /// </summary>
        public object? EditedValue { get; set; }

        /// <summary>
        ///     Gets or sets the published value of the property.
        /// </summary>
        public object? PublishedValue { get; set; }

        /// <summary>
        ///     Clones the property value.
        /// </summary>
        public IPropertyValue Clone()
            => new PropertyValue
            {
                _culture = _culture,
                _segment = _segment,
                PublishedValue = PublishedValue,
                EditedValue = EditedValue,
            };

        public override bool Equals(object? obj) => Equals(obj as PropertyValue);

        public override int GetHashCode()
        {
            var hashCode = 1885328050;
            if (_culture is not null)
            {
                hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(_culture);
            }

            if (_segment is not null)
            {
                hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(_segment);
            }

            if (EditedValue is not null)
            {
                hashCode = (hashCode * -1521134295) + EqualityComparer<object>.Default.GetHashCode(EditedValue);
            }

            if (PublishedValue is not null)
            {
                hashCode = (hashCode * -1521134295) + EqualityComparer<object>.Default.GetHashCode(PublishedValue);
            }

            return hashCode;
        }
    }
}
