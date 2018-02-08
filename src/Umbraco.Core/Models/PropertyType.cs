using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
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

        private readonly bool _isExplicitDbType;
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

        public PropertyType(IDataType dataType)
        {
            if (dataType == null) throw new ArgumentNullException(nameof(dataType));

            if(dataType.HasIdentity)
                _dataTypeId = dataType.Id;

            _propertyEditorAlias = dataType.EditorAlias;
            _valueStorageType = dataType.DatabaseType;
            _variations = ContentVariation.InvariantNeutral;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyType"/> class.
        /// </summary>
        public PropertyType(IDataType dataType, string propertyTypeAlias)
            : this(dataType)
        {
            _alias = SanitizeAlias(propertyTypeAlias);
        }

        public PropertyType(string propertyEditorAlias, ValueStorageType valueStorageType)
            : this(propertyEditorAlias, valueStorageType, false)
        { }

        public PropertyType(string propertyEditorAlias, ValueStorageType valueStorageType, string propertyTypeAlias)
            : this(propertyEditorAlias, valueStorageType, false, propertyTypeAlias)
        { }

        // fixme - need to explain and understand this explicitDbType thing here

        /// <summary>
        /// Used internally to assign an explicity database type for this property type regardless of what the underlying data type/property editor is.
        /// </summary>
        /// <param name="propertyEditorAlias"></param>
        /// <param name="valueStorageType"></param>
        /// <param name="isExplicitDbType"></param>
        internal PropertyType(string propertyEditorAlias, ValueStorageType valueStorageType, bool isExplicitDbType)
        {
            _isExplicitDbType = isExplicitDbType;
            _propertyEditorAlias = propertyEditorAlias;
            _valueStorageType = valueStorageType;
            _variations = ContentVariation.InvariantNeutral;
        }

        /// <summary>
        /// Used internally to assign an explicity database type for this property type regardless of what the underlying data type/property editor is.
        /// </summary>
        /// <param name="propertyEditorAlias"></param>
        /// <param name="valueStorageType"></param>
        /// <param name="isExplicitDbType"></param>
        /// <param name="propertyTypeAlias"></param>
        internal PropertyType(string propertyEditorAlias, ValueStorageType valueStorageType, bool isExplicitDbType, string propertyTypeAlias)
        {
            _isExplicitDbType = isExplicitDbType;
            _propertyEditorAlias = propertyEditorAlias;
            _valueStorageType = valueStorageType;
            _alias = SanitizeAlias(propertyTypeAlias);
            _variations = ContentVariation.InvariantNeutral;
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
                //don't allow setting this if an explicit declaration has been made in the ctor
                if (_isExplicitDbType) return;
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
        /// Gets or sets the regular expression for validation of legacy DataTypes fixme??
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
        /// Validates that a variation is valid for the property type.
        /// </summary>
        public bool ValidateVariation(int? languageId, string segment, bool throwIfInvalid)
        {
            ContentVariation variation;
            if (languageId.HasValue)
            {
                variation = segment != null
                    ? ContentVariation.CultureSegment
                    : ContentVariation.CultureNeutral;
            }
            else if (segment != null)
            {
                variation = ContentVariation.InvariantSegment;
            }
            else
            {
                variation = ContentVariation.InvariantNeutral;
            }
            if ((Variations & variation) == 0)
            {
                if (throwIfInvalid)
                    throw new NotSupportedException($"Variation {variation} is invalid for property type \"{Alias}\".");
                return false;
            }
            return true;
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
        public bool IsPropertyTypeValid(object value)
        {
            // null values are assumed to be ok
            if (value == null)
                return true;

            // check if the type of the value matches the type from the DataType/PropertyEditor
            var valueType = value.GetType();

            //TODO Add PropertyEditor Type validation when its relevant to introduce
            /*bool isEditorModel = value is IEditorModel;
            if (isEditorModel && DataTypeControlId != Guid.Empty)
            {
                //Find PropertyEditor by Id
                var propertyEditor = PropertyEditorResolver.Current.GetById(DataTypeControlId);

                if (propertyEditor == null)
                    return false;//Throw exception instead?

                //Get the generic parameter of the PropertyEditor and check it against the type of the passed in (object) value
                Type argument = propertyEditor.GetType().BaseType.GetGenericArguments()[0];
                return argument == type;
            }*/

            if (PropertyEditorAlias.IsNullOrWhiteSpace() == false) // fixme - always true?
            {
                // simple validation using the DatabaseType from the DataTypeDefinition
                // and the Type of the passed in value
                switch (ValueStorageType)
                {
                    // fixme breaking!
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
                }
            }

            // fixme - never reached + makes no sense?
            // fallback for simple value types when no Control Id or Database Type is set
            if (valueType.IsPrimitive || value is string)
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether a value is valid for this property type.
        /// </summary>
        public bool IsValidPropertyValue(object value)
        {
            //If the Property is mandatory and value is null or empty, return false as the validation failed
            if (Mandatory && (value == null || string.IsNullOrEmpty(value.ToString())))
                return false;

            //Check against Regular Expression for Legacy DataTypes - Validation exists and value is not null:
            if(string.IsNullOrEmpty(ValidationRegExp) == false && (value != null && string.IsNullOrEmpty(value.ToString()) == false))
            {
                try
                {
                    var regexPattern = new Regex(ValidationRegExp);
                    return regexPattern.IsMatch(value.ToString());
                }
                catch
                {
                         throw new Exception($"Invalid validation expression on property {Alias}");
                }

            }

            //TODO: We must ensure that the property value can actually be saved based on the specified database type

            //TODO Add PropertyEditor validation when its relevant to introduce
            /*if (value is IEditorModel && DataTypeControlId != Guid.Empty)
            {
                //Find PropertyEditor by Id
                var propertyEditor = PropertyEditorResolver.Current.GetById(DataTypeControlId);

                //TODO Get the validation from the PropertyEditor if a validation attribute exists
                //Will probably need to reflect the PropertyEditor in order to apply the validation
            }*/

            return true;
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
        public override object DeepClone()
        {
            var clone = (PropertyType)base.DeepClone();
            //turn off change tracking
            clone.DisableChangeTracking();
            //need to manually assign the Lazy value as it will not be automatically mapped
            if (PropertyGroupId != null)
            {
                clone._propertyGroupId = new Lazy<int>(() => PropertyGroupId.Value);
            }
            //this shouldn't really be needed since we're not tracking
            clone.ResetDirtyProperties(false);
            //re-enable tracking
            clone.EnableChangeTracking();

            return clone;
        }
    }
}
