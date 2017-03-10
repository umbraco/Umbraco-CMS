using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Models
{

    /// <summary>
    /// Defines the type of a <see cref="Property"/> object
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [DebuggerDisplay("Id: {Id}, Name: {Name}, Alias: {Alias}")]
    public class PropertyType : Entity, IEquatable<PropertyType>
    {
        private readonly bool _isExplicitDbType;
        private string _name;
        private string _alias;
        private string _description;
        private int _dataTypeDefinitionId;
        private Lazy<int> _propertyGroupId;
        private string _propertyEditorAlias;
        private DataTypeDatabaseType _dataTypeDatabaseType;
        private bool _mandatory;
        private string _helpText;
        private int _sortOrder;
        private string _validationRegExp;

        public PropertyType(IDataTypeDefinition dataTypeDefinition)
        {
            if (dataTypeDefinition == null) throw new ArgumentNullException("dataTypeDefinition");

            if(dataTypeDefinition.HasIdentity)
                _dataTypeDefinitionId = dataTypeDefinition.Id;

            _propertyEditorAlias = dataTypeDefinition.PropertyEditorAlias;
            _dataTypeDatabaseType = dataTypeDefinition.DatabaseType;
        }

        public PropertyType(IDataTypeDefinition dataTypeDefinition, string propertyTypeAlias)
            : this(dataTypeDefinition)
        {
            _alias = GetAlias(propertyTypeAlias);
        }

        public PropertyType(string propertyEditorAlias, DataTypeDatabaseType dataTypeDatabaseType)
            : this(propertyEditorAlias, dataTypeDatabaseType, false)
        {
        }

        public PropertyType(string propertyEditorAlias, DataTypeDatabaseType dataTypeDatabaseType, string propertyTypeAlias)
            : this(propertyEditorAlias, dataTypeDatabaseType, false, propertyTypeAlias)
        {
        }

        /// <summary>
        /// Used internally to assign an explicity database type for this property type regardless of what the underlying data type/property editor is.
        /// </summary>
        /// <param name="propertyEditorAlias"></param>
        /// <param name="dataTypeDatabaseType"></param>
        /// <param name="isExplicitDbType"></param>
        internal PropertyType(string propertyEditorAlias, DataTypeDatabaseType dataTypeDatabaseType, bool isExplicitDbType)
        {
            _isExplicitDbType = isExplicitDbType;
            _propertyEditorAlias = propertyEditorAlias;
            _dataTypeDatabaseType = dataTypeDatabaseType;
        }

        /// <summary>
        /// Used internally to assign an explicity database type for this property type regardless of what the underlying data type/property editor is.
        /// </summary>
        /// <param name="propertyEditorAlias"></param>
        /// <param name="dataTypeDatabaseType"></param>
        /// <param name="isExplicitDbType"></param>
        /// <param name="propertyTypeAlias"></param>
        internal PropertyType(string propertyEditorAlias, DataTypeDatabaseType dataTypeDatabaseType, bool isExplicitDbType, string propertyTypeAlias)
        {
            _isExplicitDbType = isExplicitDbType;
            _propertyEditorAlias = propertyEditorAlias;
            _dataTypeDatabaseType = dataTypeDatabaseType;
            _alias = GetAlias(propertyTypeAlias);
        }

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<PropertyType, string>(x => x.Name);
            public readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<PropertyType, string>(x => x.Alias);
            public readonly PropertyInfo DescriptionSelector = ExpressionHelper.GetPropertyInfo<PropertyType, string>(x => x.Description);
            public readonly PropertyInfo DataTypeDefinitionIdSelector = ExpressionHelper.GetPropertyInfo<PropertyType, int>(x => x.DataTypeDefinitionId);
            public readonly PropertyInfo PropertyEditorAliasSelector = ExpressionHelper.GetPropertyInfo<PropertyType, string>(x => x.PropertyEditorAlias);
            public readonly PropertyInfo DataTypeDatabaseTypeSelector = ExpressionHelper.GetPropertyInfo<PropertyType, DataTypeDatabaseType>(x => x.DataTypeDatabaseType);
            public readonly PropertyInfo MandatorySelector = ExpressionHelper.GetPropertyInfo<PropertyType, bool>(x => x.Mandatory);
            public readonly PropertyInfo HelpTextSelector = ExpressionHelper.GetPropertyInfo<PropertyType, string>(x => x.HelpText);
            public readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<PropertyType, int>(x => x.SortOrder);
            public readonly PropertyInfo ValidationRegExpSelector = ExpressionHelper.GetPropertyInfo<PropertyType, string>(x => x.ValidationRegExp);
            public readonly PropertyInfo PropertyGroupIdSelector = ExpressionHelper.GetPropertyInfo<PropertyType, Lazy<int>>(x => x.PropertyGroupId);
        }

        /// <summary>
        /// Gets of Sets the Name of the PropertyType
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector); }
        }

        /// <summary>
        /// Gets of Sets the Alias of the PropertyType
        /// </summary>
        [DataMember]
        public string Alias
        {
            get { return _alias; }
            set { SetPropertyValueAndDetectChanges(GetAlias(value), ref _alias, Ps.Value.AliasSelector); }
        }

        /// <summary>
        /// Gets of Sets the Description for the PropertyType
        /// </summary>
        [DataMember]
        public string Description
        {
            get { return _description; }
            set { SetPropertyValueAndDetectChanges(value, ref _description, Ps.Value.DescriptionSelector); }
        }

        /// <summary>
        /// Gets of Sets the Id of the DataType (Definition), which the PropertyType is "wrapping"
        /// </summary>
        /// <remarks>This is actually the Id of the <see cref="IDataTypeDefinition"/></remarks>
        [DataMember]
        public int DataTypeDefinitionId
        {
            get { return _dataTypeDefinitionId; }
            set { SetPropertyValueAndDetectChanges(value, ref _dataTypeDefinitionId, Ps.Value.DataTypeDefinitionIdSelector); }
        }

        [DataMember]
        public string PropertyEditorAlias
        {
            get { return _propertyEditorAlias; }
            set { SetPropertyValueAndDetectChanges(value, ref _propertyEditorAlias, Ps.Value.PropertyEditorAliasSelector); }
        }

        /// <summary>
        /// Gets of Sets the Id of the DataType control
        /// </summary>
        /// <remarks>This is the Id of the actual DataType control</remarks>
        [Obsolete("Property editor's are defined by a string alias from version 7 onwards, use the PropertyEditorAlias property instead. This method will return a generated GUID for any property editor alias not explicitly mapped to a legacy ID")]
        public Guid DataTypeId
        {
            get
            {
                return LegacyPropertyEditorIdToAliasConverter.GetLegacyIdFromAlias(
                    _propertyEditorAlias, LegacyPropertyEditorIdToAliasConverter.NotFoundLegacyIdResponseBehavior.GenerateId).Value;
            }
            set
            {
                var alias = LegacyPropertyEditorIdToAliasConverter.GetAliasFromLegacyId(value, true);
                PropertyEditorAlias = alias;
            }
        }

        /// <summary>
        /// Gets or Sets the DatabaseType for which the DataType's value is saved as
        /// </summary>
        [DataMember]
        internal DataTypeDatabaseType DataTypeDatabaseType
        {
            get { return _dataTypeDatabaseType; }
            set
            {
                //don't allow setting this if an explicit declaration has been made in the ctor
                if (_isExplicitDbType) return;

                SetPropertyValueAndDetectChanges(value, ref _dataTypeDatabaseType, Ps.Value.DataTypeDatabaseTypeSelector);                
            }
        }

        /// <summary>
        /// Gets or sets the identifier of the PropertyGroup this PropertyType belongs to.
        /// </summary>
        /// <remarks>For generic properties, the value is <c>null</c>.</remarks>
        [DataMember]
        internal Lazy<int> PropertyGroupId
        {
            get { return _propertyGroupId; }
            set { SetPropertyValueAndDetectChanges(value, ref _propertyGroupId, Ps.Value.PropertyGroupIdSelector); }
        }

        /// <summary>
        /// Gets of Sets the Boolean indicating whether a value for this PropertyType is required
        /// </summary>
        [DataMember]
        public bool Mandatory
        {
            get { return _mandatory; }
            set { SetPropertyValueAndDetectChanges(value, ref _mandatory, Ps.Value.MandatorySelector); }
        }

        /// <summary>
        /// Gets of Sets the Help text for the current PropertyType
        /// </summary>
        [DataMember]
        [Obsolete("Not used anywhere, will be removed in future versions")]
        public string HelpText
        {
            get { return _helpText; }
            set { SetPropertyValueAndDetectChanges(value, ref _helpText, Ps.Value.HelpTextSelector); }
        }

        /// <summary>
        /// Gets of Sets the Sort order of the PropertyType, which is used for sorting within a group
        /// </summary>
        [DataMember]
        public int SortOrder
        {
            get { return _sortOrder; }
            set { SetPropertyValueAndDetectChanges(value, ref _sortOrder, Ps.Value.SortOrderSelector); }
        }

        /// <summary>
        /// Gets or Sets the RegEx for validation of legacy DataTypes
        /// </summary>
        [DataMember]
        public string ValidationRegExp
        {
            get { return _validationRegExp; }
            set { SetPropertyValueAndDetectChanges(value, ref _validationRegExp, Ps.Value.ValidationRegExpSelector); }
        }

        private string GetAlias(string value)
        {
            //NOTE: WE are doing this because we don't want to do a ToSafeAlias when the alias is the special case of
            // being prefixed with Constants.PropertyEditors.InternalGenericPropertiesPrefix
            // which is used internally

            return value.StartsWith(Constants.PropertyEditors.InternalGenericPropertiesPrefix)
                        ? value
                        : value.ToCleanString(CleanStringType.Alias | CleanStringType.UmbracoCase);
        }

        /// <summary>
        /// Create a new Property object from a "raw" database value.
        /// </summary>
        /// <remarks>Can be used for the "old" values where no serialization type exists</remarks>
        /// <param name="value"></param>
        /// <param name="version"> </param>
        /// <param name="id"> </param>
        /// <returns></returns>
        internal Property CreatePropertyFromRawValue(object value, Guid version, int id)
        {
            return new Property(id, version, this, value);
        }

        /// <summary>
        /// Create a new Property object from a "raw" database value.
        /// In some cases the value will need to be deserialized.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="serializationType"> </param>
        /// <returns></returns>
        internal Property CreatePropertyFromRawValue(object value, string serializationType)
        {
            //The value from the db needs to be deserialized and then added to the property
            //if its not a simple type (Integer, Date, Nvarchar, Ntext)
            /*if (DataTypeDatabaseType == DataTypeDatabaseType.Object)
            {
                Type type = Type.GetType(serializationType);
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(value.ToString()));
                var objValue = _service.FromStream(stream, type);
                return new Property(this, objValue);
            }*/

            return new Property(this, value);
        }

        /// <summary>
        /// Create a new Property object that conforms to the Type of the DataType
        /// and can be validated according to DataType validation / Mandatory-check.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Property CreatePropertyFromValue(object value)
        {
            //Note that validation will occur when setting the value on the Property
            return new Property(this, value);
        }

        /// <summary>
        /// Gets a value indicating whether the value is of the expected type
        /// for the property, and can be assigned to the property "as is".
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if the value is of the expected type for the property,
        /// and can be assigned to the property "as is". Otherwise, false, to indicate
        /// that some conversion is required.</returns>
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
                switch (DataTypeDatabaseType)
                {
                    // fixme breaking!
                    case DataTypeDatabaseType.Integer:
                        return valueType == typeof(int);
                    case DataTypeDatabaseType.Decimal:
                        return valueType == typeof(decimal);
                    case DataTypeDatabaseType.Date:
                        return valueType == typeof(DateTime);
                    case DataTypeDatabaseType.Nvarchar:
                        return valueType == typeof(string);
                    case DataTypeDatabaseType.Ntext:
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
        /// Validates the Value from a Property according to the validation settings
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True if valid, otherwise false</returns>
        public bool IsPropertyValueValid(object value)
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
                         throw new Exception(string .Format("Invalid validation expression on property {0}",this.Alias));
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

        public bool Equals(PropertyType other)
        {
            if (base.Equals(other)) return true;

            //Check whether the PropertyType's properties are equal.
            return Alias.InvariantEquals(other.Alias);
        }

        public override int GetHashCode()
        {
            //Get hash code for the Name field if it is not null.
            int baseHash = base.GetHashCode();

            //Get hash code for the Alias field.
            int hashAlias = Alias.ToLowerInvariant().GetHashCode();

            //Calculate the hash code for the product.
            return baseHash ^ hashAlias;
        }

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