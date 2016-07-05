using System;
using System.Collections;
using System.Linq;
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
        private PropertyType _propertyType;
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

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo ValueSelector = ExpressionHelper.GetPropertyInfo<Property, object>(x => x.Value);
            public readonly PropertyInfo VersionSelector = ExpressionHelper.GetPropertyInfo<Property, Guid>(x => x.Version);
        }

        private static readonly DelegateEqualityComparer<object> ValueComparer = new DelegateEqualityComparer<object>(
            (o, o1) =>
            {
                if (o == null && o1 == null) return true;

                //custom comparer for strings.                        
                if (o is string || o1 is string)
                {
                    //if one is null and another is empty then they are the same
                    if ((o as string).IsNullOrWhiteSpace() && (o1 as string).IsNullOrWhiteSpace())
                    {
                        return true;
                    }
                    if (o == null || o1 == null) return false;
                    return o.Equals(o1);
                }

                if (o == null || o1 == null) return false;

                //Custom comparer for enumerable if it is enumerable
                var enum1 = o as IEnumerable;
                var enum2 = o1 as IEnumerable;
                if (enum1 != null && enum2 != null)
                {
                    return enum1.Cast<object>().UnsortedSequenceEqual(enum2.Cast<object>());
                }
                return o.Equals(o1);
            }, o => o.GetHashCode());
        
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
        public PropertyType PropertyType { get { return _propertyType; } }
        
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
            set { SetPropertyValueAndDetectChanges(value, ref _version, Ps.Value.VersionSelector); }
        }

        private static void ThrowTypeException(object value, Type expected, string alias)
        {
            throw new Exception(string.Format("Value \"{0}\" of type \"{1}\" could not be converted"
                + " to type \"{2}\" which is expected by property type \"{3}\".",
                value, value.GetType(), expected, alias));
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
                var isOfExpectedType = _propertyType.IsPropertyTypeValid(value);

                if (isOfExpectedType == false) // isOfExpectedType is true if value is null - so if false, value is *not* null
                {
                    // "garbage-in", accept what we can & convert
                    // throw only if conversion is not possible

                    switch (_propertyType.DataTypeDatabaseType)
                    {
                        case DataTypeDatabaseType.Nvarchar:
                        case DataTypeDatabaseType.Ntext:
                            value = value.ToString();
                            break;
                        case DataTypeDatabaseType.Integer:
                            var convInt = value.TryConvertTo<int>();
                            if (convInt == false) ThrowTypeException(value, typeof(int), _propertyType.Alias);
                            value = convInt.Result;
                            break;
                        case DataTypeDatabaseType.Decimal:
                            var convDecimal = value.TryConvertTo<decimal>();
                            if (convDecimal == false) ThrowTypeException(value, typeof(decimal), _propertyType.Alias);
                            // need to normalize the value (change the scaling factor and remove trailing zeroes)
                            // because the underlying database is going to mess with the scaling factor anyways.
                            value = convDecimal.Result.Normalize();
                            break;
                        case DataTypeDatabaseType.Date:
                            var convDateTime = value.TryConvertTo<DateTime>();
                            if (convDateTime == false) ThrowTypeException(value, typeof(DateTime), _propertyType.Alias);
                            value = convDateTime.Result;
                            break;
                    }
                }

                SetPropertyValueAndDetectChanges(value, ref _value, Ps.Value.ValueSelector, ValueComparer);
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

        public override object DeepClone()
        {
            var clone = (Property)base.DeepClone();
            //turn off change tracking
            clone.DisableChangeTracking();
            //need to manually assign since this is a readonly property
            clone._propertyType = (PropertyType)PropertyType.DeepClone();
            //this shouldn't really be needed since we're not tracking
            clone.ResetDirtyProperties(false);
            //re-enable tracking
            clone.EnableChangeTracking();
            
            return clone;
        }
    }
}