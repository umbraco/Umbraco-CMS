using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents the content type that a <see cref="Member"/> object is based on
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class MemberType : ContentTypeCompositionBase, IMemberType
    {
        //Dictionary is divided into string: PropertyTypeAlias, Tuple: MemberCanEdit, VisibleOnProfile, PropertyTypeId
        private IDictionary<string, Tuple<bool, bool, int>> _memberTypePropertyTypes;

        public MemberType(int parentId) : base(parentId)
        {
            _memberTypePropertyTypes = new Dictionary<string, Tuple<bool, bool, int>>();
        }

        public MemberType(IContentTypeComposition parent) : base(parent)
        {
            _memberTypePropertyTypes = new Dictionary<string, Tuple<bool, bool, int>>();
        }

        private static readonly PropertyInfo MemberTypePropertyTypesSelector = ExpressionHelper.GetPropertyInfo<MemberType, IDictionary<string, Tuple<bool, bool, int>>>(x => x.MemberTypePropertyTypes);

        /// <summary>
        /// Gets or Sets a Dictionary of Tuples (MemberCanEdit, VisibleOnProfile, PropertyTypeId) by the PropertyTypes' alias.
        /// </summary>
        [DataMember]
        internal IDictionary<string, Tuple<bool, bool, int>> MemberTypePropertyTypes
        {
            get { return _memberTypePropertyTypes; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _memberTypePropertyTypes = value;
                    return _memberTypePropertyTypes;
                }, _memberTypePropertyTypes, MemberTypePropertyTypesSelector);
            }
        }

        /// <summary>
        /// Gets a boolean indicating whether a Property is editable by the Member.
        /// </summary>
        /// <param name="propertyTypeAlias">PropertyType Alias of the Property to check</param>
        /// <returns></returns>
        public bool MemberCanEditProperty(string propertyTypeAlias)
        {
            if (MemberTypePropertyTypes.ContainsKey(propertyTypeAlias))
            {
                return MemberTypePropertyTypes[propertyTypeAlias].Item1;
            }

            return false;
        }

        /// <summary>
        /// Gets a boolean indicating whether a Property is visible on the Members profile.
        /// </summary>
        /// <param name="propertyTypeAlias">PropertyType Alias of the Property to check</param>
        /// <returns></returns>
        public bool MemberCanViewProperty(string propertyTypeAlias)
        {
            if (MemberTypePropertyTypes.ContainsKey(propertyTypeAlias))
            {
                return MemberTypePropertyTypes[propertyTypeAlias].Item2;
            }

            return false;
        }

        /// <summary>
        /// Sets a boolean indicating whether a Property is editable by the Member.
        /// </summary>
        /// <param name="propertyTypeAlias">PropertyType Alias of the Property to set</param>
        /// <param name="value">Boolean value, true or false</param>
        public void SetMemberCanEditProperty(string propertyTypeAlias, bool value)
        {
            if (MemberTypePropertyTypes.ContainsKey(propertyTypeAlias))
            {
                var tuple = MemberTypePropertyTypes[propertyTypeAlias];
                MemberTypePropertyTypes[propertyTypeAlias] = new Tuple<bool, bool, int>(value, tuple.Item2, tuple.Item3);
            }
            else
            {
                var propertyType = PropertyTypes.First(x => x.Alias.Equals(propertyTypeAlias));
                var tuple = new Tuple<bool, bool, int>(value, false, propertyType.Id);
                MemberTypePropertyTypes.Add(propertyTypeAlias, tuple);
            }
        }

        /// <summary>
        /// Sets a boolean indicating whether a Property is visible on the Members profile.
        /// </summary>
        /// <param name="propertyTypeAlias">PropertyType Alias of the Property to set</param>
        /// <param name="value">Boolean value, true or false</param>
        public void SetMemberCanViewProperty(string propertyTypeAlias, bool value)
        {
            if (MemberTypePropertyTypes.ContainsKey(propertyTypeAlias))
            {
                var tuple = MemberTypePropertyTypes[propertyTypeAlias];
                MemberTypePropertyTypes[propertyTypeAlias] = new Tuple<bool, bool, int>(tuple.Item1, value, tuple.Item3);
            }
            else
            {
                var propertyType = PropertyTypes.First(x => x.Alias.Equals(propertyTypeAlias));
                var tuple = new Tuple<bool, bool, int>(false, value, propertyType.Id);
                MemberTypePropertyTypes.Add(propertyTypeAlias, tuple);
            }
        }
    }
}