using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
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
        private static PropertySelectors _selectors;

        private readonly bool _forceValueStorageType;
        private string _name;
        private string _alias;
        private string _description;
        private int _dataTypeId;
        private Lazy<int> _propertyGroupId;
        private string _propertyEditorAlias;
        private ValueStorageType _valueStorageType;
        private bool _mandatory;
        private int _sortOrder;
        private string _validationRegExp;
        private ContentVariation _variations;

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

        private static PropertySelectors Selectors => _selectors ?? (_selectors = new PropertySelectors());

        private class PropertySelectors
        {
            public readonly PropertyInfo Name = ExpressionHelper.GetPropertyInfo<PropertyType, string>(x => x.Name);
            public readonly PropertyInfo Alias = ExpressionHelper.GetPropertyInfo<PropertyType, string>(x => x.Alias);
            public readonly PropertyInfo Description = ExpressionHelper.GetPropertyInfo<PropertyType, string>(x => x.Description);
            public readonly PropertyInfo DataTypeId = ExpressionHelper.GetPropertyInfo<PropertyType, int>(x => x.DataTypeId);
            public readonly PropertyInfo PropertyEditorAlias = ExpressionHelper.GetPropertyInfo<PropertyType, string>(x => x.PropertyEditorAlias);
            public readonly PropertyInfo ValueStorageType = ExpressionHelper.GetPropertyInfo<PropertyType, ValueStorageType>(x => x.ValueStorageType);
            public readonly PropertyInfo Mandatory = ExpressionHelper.GetPropertyInfo<PropertyType, bool>(x => x.Mandatory);
            public readonly PropertyInfo SortOrder = ExpressionHelper.GetPropertyInfo<PropertyType, int>(x => x.SortOrder);
            public readonly PropertyInfo ValidationRegExp = ExpressionHelper.GetPropertyInfo<PropertyType, string>(x => x.ValidationRegExp);
            public readonly PropertyInfo PropertyGroupId = ExpressionHelper.GetPropertyInfo<PropertyType, Lazy<int>>(x => x.PropertyGroupId);
            public readonly PropertyInfo VaryBy = ExpressionHelper.GetPropertyInfo<PropertyType, ContentVariation>(x => x.Variations);
        }

        /// <summary>
        /// Gets a value indicating whether the content type, owning this property type, is publishing.
        /// </summary>
        public bool IsPublishing { get; internal set; }

        /// <summary>
        /// Gets of sets the name of the property type.
        /// </summary>
        [DataMember]
        public string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, Selectors.Name);
        }

        /// <summary>
        /// Gets of sets the alias of the property type.
        /// </summary>
        [DataMember]
        public string Alias
        {
            get => _alias;
            set => SetPropertyValueAndDetectChanges(SanitizeAlias(value), ref _alias, Selectors.Alias);
        }

        /// <summary>
        /// Gets of sets the description of the property type.
        /// </summary>
        [DataMember]
        public string Description
        {
            get => _description;
            set => SetPropertyValueAndDetectChanges(value, ref _description, Selectors.Description);
        }

        /// <summary>
        /// Gets or sets the identifier of the datatype for this property type.
        /// </summary>
        [DataMember]
        public int DataTypeId
        {
            get => _dataTypeId;
            set => SetPropertyValueAndDetectChanges(value, ref _dataTypeId, Selectors.DataTypeId);
        }

        /// <summary>
        /// Gets or sets the alias of the property editor for this property type.
        /// </summary>
        [DataMember]
        public string PropertyEditorAlias
        {
            get => _propertyEditorAlias;
            set => SetPropertyValueAndDetectChanges(value, ref _propertyEditorAlias, Selectors.PropertyEditorAlias);
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
                SetPropertyValueAndDetectChanges(value, ref _valueStorageType, Selectors.ValueStorageType);
            }
        }

        /// <summary>
        /// Gets or sets the identifier of the property group this property type belongs to.
        /// </summary>
        /// <remarks>For generic properties, the value is <c>null</c>.</remarks>
        [DataMember]
        internal Lazy<int> PropertyGroupId
        {
            get => _propertyGroupId;
            set => SetPropertyValueAndDetectChanges(value, ref _propertyGroupId, Selectors.PropertyGroupId);
        }

        /// <summary>
        /// Gets of sets a value indicating whether a value for this property type is required.
        /// </summary>
        [DataMember]
        public bool Mandatory
        {
            get => _mandatory;
            set => SetPropertyValueAndDetectChanges(value, ref _mandatory, Selectors.Mandatory);
        }

        /// <summary>
        /// Gets of sets the sort order of the property type.
        /// </summary>
        [DataMember]
        public int SortOrder
        {
            get => _sortOrder;
            set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, Selectors.SortOrder);
        }

        /// <summary>
        /// Gets or sets the regular expression validating the property values.
        /// </summary>
        [DataMember]
        public string ValidationRegExp
        {
            get => _validationRegExp;
            set => SetPropertyValueAndDetectChanges(value, ref _validationRegExp, Selectors.ValidationRegExp);
        }

        /// <summary>
        /// Gets or sets the content variation of the property type.
        /// </summary>
        public ContentVariation Variations
        {
            get => _variations;
            set => SetPropertyValueAndDetectChanges(value, ref _variations, Selectors.VaryBy);
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
        /// <para>The input value can be pretty much anything, and is converted to the actual Clr type
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
                        // need to normalize the value (change the scaling factor and remove trailing zeroes)
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


        //fixme - this and other value validation methods should be a service level (not a model) thing. Changing this to internal for now
        /// <summary>
        /// Determines whether a value is valid for this property type.
        /// </summary>
        internal bool IsPropertyValueValid(object value)
        {
            var editor = Current.PropertyEditors[_propertyEditorAlias]; // fixme inject?
            var configuration = Current.Services.DataTypeService.GetDataType(_dataTypeId).Configuration; // fixme inject?
            var valueEditor = editor.GetValueEditor(configuration);
            return !valueEditor.Validate(value, Mandatory, ValidationRegExp).Any();
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
