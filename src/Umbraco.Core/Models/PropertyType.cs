using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a property type.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [DebuggerDisplay("Id: {Id}, Name: {Name}, Alias: {Alias}")]
    public class PropertyType : EntityBase, IEquatable<PropertyType>
    {
        private readonly bool _forceValueStorageType;
        private string _name;
        private string _alias;
        private string _description;
        private int _dataTypeId;
        private Guid _dataTypeKey;
        private Lazy<int> _propertyGroupId;
        private string _propertyEditorAlias;
        private ValueStorageType _valueStorageType;
        private bool _mandatory;
        private string _mandatoryMessage;
        private int _sortOrder;
        private string _validationRegExp;
        private string _validationRegExpMessage;
        private ContentVariation _variations;
        private bool _labelOnTop;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyType"/> class.
        /// </summary>
        public PropertyType(IDataType dataType)
        {
            if (dataType == null) throw new ArgumentNullException(nameof(dataType));

            if(dataType.HasIdentity)
                _dataTypeId = dataType.Id;

            _propertyEditorAlias = dataType.EditorAlias;
            _valueStorageType = dataType.DatabaseType;
            _variations = ContentVariation.Nothing;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyType"/> class.
        /// </summary>
        public PropertyType(IDataType dataType, string propertyTypeAlias)
            : this(dataType)
        {
            _alias = SanitizeAlias(propertyTypeAlias);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyType"/> class.
        /// </summary>
        public PropertyType(string propertyEditorAlias, ValueStorageType valueStorageType)
            : this(propertyEditorAlias, valueStorageType, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyType"/> class.
        /// </summary>
        public PropertyType(string propertyEditorAlias, ValueStorageType valueStorageType, string propertyTypeAlias)
            : this(propertyEditorAlias, valueStorageType, false, propertyTypeAlias)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyType"/> class.
        /// </summary>
        /// <remarks>Set <paramref name="forceValueStorageType"/> to true to force the value storage type. Values assigned to
        /// the property, eg from the underlying datatype, will be ignored.</remarks>
        internal PropertyType(string propertyEditorAlias, ValueStorageType valueStorageType, bool forceValueStorageType, string propertyTypeAlias = null)
        {
            _propertyEditorAlias = propertyEditorAlias;
            _valueStorageType = valueStorageType;
            _forceValueStorageType = forceValueStorageType;
            _alias = propertyTypeAlias == null ? null : SanitizeAlias(propertyTypeAlias);
            _variations = ContentVariation.Nothing;
        }

        /// <summary>
        /// Gets a value indicating whether the content type owning this property type is publishing.
        /// </summary>
        /// <remarks>
        /// <para>A publishing content type supports draft and published values for properties.
        /// It is possible to retrieve either the draft (default) or published value of a property.
        /// Setting the value always sets the draft value, which then needs to be published.</para>
        /// <para>A non-publishing content type only supports one value for properties. Getting
        /// the draft or published value of a property returns the same thing, and publishing
        /// a value property has no effect.</para>
        /// <para>When true, getting the property value returns the edited value by default, but
        /// it is possible to get the published value using the appropriate 'published' method
        /// parameter.</para>
        /// <para>When false, getting the property value always return the edited value,
        /// regardless of the 'published' method parameter.</para>
        /// </remarks>
        public bool SupportsPublishing { get; internal set; }

        /// <summary>
        /// Gets of sets the name of the property type.
        /// </summary>
        [DataMember]
        public string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
        }

        /// <summary>
        /// Gets of sets the alias of the property type.
        /// </summary>
        [DataMember]
        public virtual string Alias
        {
            get => _alias;
            set => SetPropertyValueAndDetectChanges(SanitizeAlias(value), ref _alias, nameof(Alias));
        }

        /// <summary>
        /// Gets of sets the description of the property type.
        /// </summary>
        [DataMember]
        public string Description
        {
            get => _description;
            set => SetPropertyValueAndDetectChanges(value, ref _description, nameof(Description));
        }

        /// <summary>
        /// Gets or sets the identifier of the datatype for this property type.
        /// </summary>
        [DataMember]
        public int DataTypeId
        {
            get => _dataTypeId;
            set => SetPropertyValueAndDetectChanges(value, ref _dataTypeId, nameof(DataTypeId));
        }

        [DataMember]
        public Guid DataTypeKey
        {
            get => _dataTypeKey;
            set => SetPropertyValueAndDetectChanges(value, ref _dataTypeKey, nameof(DataTypeKey));
        }

        /// <summary>
        /// Gets or sets the alias of the property editor for this property type.
        /// </summary>
        [DataMember]
        public string PropertyEditorAlias
        {
            get => _propertyEditorAlias;
            set => SetPropertyValueAndDetectChanges(value, ref _propertyEditorAlias, nameof(PropertyEditorAlias));
        }

        /// <summary>
        /// Gets or sets the database type for storing value for this property type.
        /// </summary>
        [DataMember]
        internal ValueStorageType ValueStorageType
        {
            get => _valueStorageType;
            set
            {
                if (_forceValueStorageType) return; // ignore changes
                SetPropertyValueAndDetectChanges(value, ref _valueStorageType, nameof(ValueStorageType));
            }
        }

        /// <summary>
        /// Gets or sets the identifier of the property group this property type belongs to.
        /// </summary>
        /// <remarks>For generic properties, the value is <c>null</c>.</remarks>
        [DataMember]
        [DoNotClone]
        public Lazy<int> PropertyGroupId
        {
            get => _propertyGroupId;
            set => SetPropertyValueAndDetectChanges(value, ref _propertyGroupId, nameof(PropertyGroupId));
        }

        /// <summary>
        /// Gets or sets a value indicating whether a value for this property type is required.
        /// </summary>
        [DataMember]
        public bool Mandatory
        {
            get => _mandatory;
            set => SetPropertyValueAndDetectChanges(value, ref _mandatory, nameof(Mandatory));
        }

        /// <summary>
        /// Gets or sets the custom validation message used when a value for this PropertyType is required
        /// </summary>
        [DataMember]
        public string MandatoryMessage
        {
            get => _mandatoryMessage;
            set => SetPropertyValueAndDetectChanges(value, ref _mandatoryMessage, nameof(MandatoryMessage));
        }

        /// <summary>
        /// Gets or sets a value indicating whether the label of this property type should be displayed on top.
        /// </summary>
        [DataMember]
        public bool LabelOnTop
        {
            get => _labelOnTop;
            set => SetPropertyValueAndDetectChanges(value, ref _labelOnTop, nameof(LabelOnTop));
        }

        /// <summary>
        /// Gets of sets the sort order of the property type.
        /// </summary>
        [DataMember]
        public int SortOrder
        {
            get => _sortOrder;
            set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, nameof(SortOrder));
        }

        /// <summary>
        /// Gets or sets the regular expression validating the property values.
        /// </summary>
        [DataMember]
        public string ValidationRegExp
        {
            get => _validationRegExp;
            set => SetPropertyValueAndDetectChanges(value, ref _validationRegExp, nameof(ValidationRegExp));
        }

        /// <summary>
        /// Gets or sets the custom validation message used when a pattern for this PropertyType must be matched
        /// </summary>
        [DataMember]
        public string ValidationRegExpMessage
        {
            get => _validationRegExpMessage;
            set => SetPropertyValueAndDetectChanges(value, ref _validationRegExpMessage, nameof(ValidationRegExpMessage));
        }

        /// <summary>
        /// Gets or sets the content variation of the property type.
        /// </summary>
        public ContentVariation Variations
        {
            get => _variations;
            set => SetPropertyValueAndDetectChanges(value, ref _variations, nameof(Variations));
        }

        /// <summary>
        /// Determines whether the property type supports a combination of culture and segment.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <param name="segment">The segment.</param>
        /// <param name="wildcards">A value indicating whether wildcards are valid.</param>
        public bool SupportsVariation(string culture, string segment, bool wildcards = false)
        {
            // exact validation: cannot accept a 'null' culture if the property type varies
            //  by culture, and likewise for segment
            // wildcard validation: can accept a '*' culture or segment
            return Variations.ValidateVariation(culture, segment, true, wildcards, false);
        }

        /// <summary>
        /// Creates a new property of this property type.
        /// </summary>
        public Property CreateProperty()
        {
            return new Property(this);
        }

        /// <summary>
        /// Determines whether a value is of the expected type for this property type.
        /// </summary>
        /// <remarks>
        /// <para>If the value is of the expected type, it can be directly assigned to the property.
        /// Otherwise, some conversion is required.</para>
        /// </remarks>
        public bool IsOfExpectedPropertyType(object value)
        {
            // null values are assumed to be ok
            if (value == null)
                return true;

            // check if the type of the value matches the type from the DataType/PropertyEditor
            // then it can be directly assigned, anything else requires conversion
            var valueType = value.GetType();
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
        /// Determines whether a value can be assigned to a property.
        /// </summary>
        public bool IsValueAssignable(object value) => TryConvertAssignedValue(value, false, out _);

        /// <summary>
        /// Converts a value assigned to a property.
        /// </summary>
        /// <remarks>
        /// <para>The input value can be pretty much anything, and is converted to the actual CLR type
        /// expected by the property (eg an integer if the property values are integers).</para>
        /// <para>Throws if the value cannot be converted.</para>
        /// </remarks>
        public object ConvertAssignedValue(object value) => TryConvertAssignedValue(value, true, out var converted) ? converted : null;

        /// <summary>
        /// Tries to convert a value assigned to a property.
        /// </summary>
        /// <remarks>
        /// <para></para>
        /// </remarks>
        public bool TryConvertAssignedValue(object value, out object converted) => TryConvertAssignedValue(value, false, out converted);

        private bool TryConvertAssignedValue(object value, bool throwOnError, out object converted)
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

            var s = value.ToString();
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
                        return true; // assume empty means null
                    var convInt = value.TryConvertTo<int>();
                    if (convInt)
                    {
                        converted = convInt.Result;
                        return true;
                    }
                    if (throwOnError)
                        ThrowTypeException(value, typeof(int), Alias);
                    return false;

                case ValueStorageType.Decimal:
                    if (s.IsNullOrWhiteSpace())
                        return true; // assume empty means null
                    var convDecimal = value.TryConvertTo<decimal>();
                    if (convDecimal)
                    {
                        // need to normalize the value (change the scaling factor and remove trailing zeros)
                        // because the underlying database is going to mess with the scaling factor anyways.
                        converted = convDecimal.Result.Normalize();
                        return true;
                    }
                    if (throwOnError)
                        ThrowTypeException(value, typeof(decimal), Alias);
                    return false;

                case ValueStorageType.Date:
                    if (s.IsNullOrWhiteSpace())
                        return true; // assume empty means null
                    var convDateTime = value.TryConvertTo<DateTime>();
                    if (convDateTime)
                    {
                        converted = convDateTime.Result;
                        return true;
                    }
                    if (throwOnError)
                        ThrowTypeException(value, typeof(DateTime), Alias);
                    return false;

                default:
                    throw new NotSupportedException($"Not supported storage type \"{ValueStorageType}\".");
            }
        }

        private static void ThrowTypeException(object value, Type expected, string alias)
        {
            throw new InvalidOperationException($"Cannot assign value \"{value}\" of type \"{value.GetType()}\" to property \"{alias}\" expecting type \"{expected}\".");
        }


        /// <summary>
        /// Sanitizes a property type alias.
        /// </summary>
        private static string SanitizeAlias(string value)
        {
            //NOTE: WE are doing this because we don't want to do a ToSafeAlias when the alias is the special case of
            // being prefixed with Constants.PropertyEditors.InternalGenericPropertiesPrefix
            // which is used internally

            return value.StartsWith(Constants.PropertyEditors.InternalGenericPropertiesPrefix)
                ? value
                : value.ToCleanString(CleanStringType.Alias | CleanStringType.UmbracoCase);
        }

        /// <inheritdoc />
        public bool Equals(PropertyType other)
        {
            return other != null && (base.Equals(other) || Alias.InvariantEquals(other.Alias));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            //Get hash code for the Name field if it is not null.
            int baseHash = base.GetHashCode();

            //Get hash code for the Alias field.
            int hashAlias = Alias.ToLowerInvariant().GetHashCode();

            //Calculate the hash code for the product.
            return baseHash ^ hashAlias;
        }

        /// <inheritdoc />
        protected override void PerformDeepClone(object clone)
        {
            base.PerformDeepClone(clone);

            var clonedEntity = (PropertyType)clone;

            //need to manually assign the Lazy value as it will not be automatically mapped
            if (PropertyGroupId != null)
            {
                clonedEntity._propertyGroupId = new Lazy<int>(() => PropertyGroupId.Value);
            }
        }
    }
}
