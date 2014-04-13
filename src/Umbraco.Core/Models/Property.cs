using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Mappers;

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
        private readonly PropertyTags _tagSupport = new PropertyTags();

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

        public Property(int id, Guid version, PropertyType propertyType, object value)
        {
            Id = id;
            _propertyType = propertyType;
            _version = version;
            Value = value;
        }

        private static readonly PropertyInfo ValueSelector = ExpressionHelper.GetPropertyInfo<Property, object>(x => x.Value);
        private static readonly PropertyInfo VersionSelector = ExpressionHelper.GetPropertyInfo<Property, Guid>(x => x.Version);
        
        /// <summary>
        /// Returns the instance of the tag support, by default tags are not enabled
        /// </summary>
        internal PropertyTags TagSupport
        {
            get { return _tagSupport; }
        }

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
        /// <remarks>
        /// Only used internally when saving the property value.
        /// </remarks>
        [IgnoreDataMember]
        internal DataTypeDatabaseType DataTypeDatabaseType
        {
            get { return _propertyType.DataTypeDatabaseType; }
        }

        /// <summary>
        /// Returns the PropertyType, which this Property is based on
        /// </summary>
        [IgnoreDataMember]
        internal PropertyType PropertyType { get { return _propertyType; } }
        
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
                SetPropertyValueAndDetectChanges(o =>
                {
                    _version = value;
                    return _version;
                }, _version, VersionSelector);
            }
        }

        /// <summary>
        /// Gets or Sets the value of the Property
        /// </summary>
        /// <remarks>
        /// Setting the value will trigger a type validation. 
        /// The type of the value has to be valid in order to be saved.
        /// </remarks>
        [DataMember]
        public object Value
        {
            get { return _value; }
            set
            {
                bool typeValidation = _propertyType.IsPropertyTypeValid(value);

                if (!typeValidation)
                    throw new Exception(
                        string.Format(
                            "Type validation failed. The value type: '{0}' does not match the DataType in PropertyType with alias: '{1}'",
                            value == null ? "null" : value.GetType().Name, Alias));

                SetPropertyValueAndDetectChanges(o =>
                {
                    _value = value;
                    return _value;
                }, _value, ValueSelector);
            }
        }

        /// <summary>
        /// Boolean indicating whether the current value is valid
        /// </summary>
        /// <remarks>
        /// A valid value implies that it is ready for publishing.
        /// Invalid property values can be saved, but not published.
        /// </remarks>
        /// <returns>True is property value is valid, otherwise false</returns>
        public bool IsValid()
        {
            return IsValid(Value);
        }

        /// <summary>
        /// Boolean indicating whether the passed in value is valid
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True is property value is valid, otherwise false</returns>
        public bool IsValid(object value)
        {
            return _propertyType.IsPropertyValueValid(value);
        }
    }
}