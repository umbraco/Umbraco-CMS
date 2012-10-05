using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// A Property contains a single piece of data
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Property : Entity
    {
        private readonly PropertyType _propertyType;
        private Guid _version;
        private object _value;

        protected Property()
        {
            
        }

        public Property(PropertyType propertyType)
        {
            _propertyType = propertyType;
        }

        public Property(PropertyType propertyType, object value)
        {
            _propertyType = propertyType;
            Value = value;
        }

        public Property(PropertyType propertyType, object value, Guid version)
        {
            _propertyType = propertyType;
            _version = version;
            Value = value;
        }

        private static readonly PropertyInfo ValueSelector = ExpressionHelper.GetPropertyInfo<Property, object>(x => x.Value);
        private static readonly PropertyInfo VersionSelector = ExpressionHelper.GetPropertyInfo<Property, Guid>(x => x.Version);

        /// <summary>
        /// Returns the Alias of the PropertyType, which this Property is based on
        /// </summary>
        [DataMember]
        public string Alias { get { return _propertyType.Alias; } }

        /// <summary>
        /// Returns the Id of the PropertyType, which this Property is based on
        /// </summary>
        [IgnoreDataMember]
        internal int PropertyTypeId { get { return _propertyType.Id; } }

        /// <summary>
        /// Returns the DatabaseType that the underlaying DataType is using to store its values
        /// </summary>
        /// <remarks>Only used internally when saving the property value</remarks>
        [IgnoreDataMember]
        internal DataTypeDatabaseType DataTypeDatabaseType { get { return _propertyType.DataTypeDatabaseType; } }

        /// <summary>
        /// Gets or Sets the version id for the Property
        /// </summary>
        /// <remarks>
        /// The version will be the same for all Property objects in a collection on a Content 
        /// object, so not sure how much this makes sense but adding it to align with:
        /// umbraco.interfaces.IProperty
        /// </remarks>
        [DataMember]
        public Guid Version
        {
            get { return _version; }
            set
            {
                _version = value;
                OnPropertyChanged(VersionSelector);
            }
        }

        /// <summary>
        /// Gets or Sets the value of the Property
        /// </summary>
        /// <remarks>Setting the value will trigger a type and value validation</remarks>
        [DataMember]
        public object Value
        {
            get { return _value; }
            set
            {
                bool typeValidation = _propertyType.IsPropertyTypeValid(value);
                bool valueValidation = _propertyType.IsPropertyValueValid(value);

                if (!typeValidation && !valueValidation)
                    throw new Exception(
                        string.Format(
                            "Both Type and Value validation failed. The value type: {0} does not match the DataType in PropertyType with alias: {1}",
                            value.GetType(), Alias));

                if (!typeValidation)
                    throw new Exception(
                        string.Format(
                            "Type validation failed. The value type: {0} does not match the DataType in PropertyType with alias: {1}",
                            value.GetType(), Alias));

                if (!valueValidation)
                    throw new Exception(
                        string.Format(
                            "Validation failed for the Value, because it does not conform to the validation rules set for the PropertyType with alias: {0}",
                            Alias));

                _value = value;
                OnPropertyChanged(ValueSelector);
            }
        }
    }
}