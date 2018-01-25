using System;
using System.Collections.Generic;
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
        private string _alias;

        public MemberType(int parentId) : base(parentId)
        {
            MemberTypePropertyTypes = new Dictionary<string, MemberTypePropertyProfileAccess>();
        }

        public MemberType(IContentTypeComposition parent) : this(parent, null)
        {
        }

        public MemberType(IContentTypeComposition parent, string alias)
            : base(parent, alias)
        {
            MemberTypePropertyTypes = new Dictionary<string, MemberTypePropertyProfileAccess>();
        }

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<MemberType, string>(x => x.Alias);
        }

        /// <summary>
        /// The Alias of the ContentType
        /// </summary>
        [DataMember]
        public override string Alias
        {
            get { return _alias; }
            set
            {
                //NOTE: WE are overriding this because we don't want to do a ToSafeAlias when the alias is the special case of
                // "_umbracoSystemDefaultProtectType" which is used internally, currently there is an issue with the safe alias as it strips
                // leading underscores which we don't want in this case.
                // see : http://issues.umbraco.org/issue/U4-3968

                //TODO: BUT, I'm pretty sure we could do this with regards to underscores now:
                // .ToCleanString(CleanStringType.Alias | CleanStringType.UmbracoCase)
                // Need to ask Stephen

                var newVal = value == "_umbracoSystemDefaultProtectType"
                        ? value
                        : (value == null ? string.Empty : value.ToSafeAlias());

                SetPropertyValueAndDetectChanges(newVal, ref _alias, Ps.Value.AliasSelector);
            }
        }

        /// <summary>
        /// Gets or Sets a Dictionary of Tuples (MemberCanEdit, VisibleOnProfile, IsSensitive) by the PropertyTypes' alias.
        /// </summary>
        [DataMember]
        internal IDictionary<string, MemberTypePropertyProfileAccess> MemberTypePropertyTypes { get; private set; }

        /// <summary>
        /// Gets a boolean indicating whether a Property is editable by the Member.
        /// </summary>
        /// <param name="propertyTypeAlias">PropertyType Alias of the Property to check</param>
        /// <returns></returns>
        public bool MemberCanEditProperty(string propertyTypeAlias)
        {
            MemberTypePropertyProfileAccess propertyProfile;
            if (MemberTypePropertyTypes.TryGetValue(propertyTypeAlias, out propertyProfile))
            {
                return propertyProfile.IsEditable;
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
            MemberTypePropertyProfileAccess propertyProfile;
            if (MemberTypePropertyTypes.TryGetValue(propertyTypeAlias, out propertyProfile))
            {
                return propertyProfile.IsVisible;
            }
            return false;
        }

        /// <summary>
        /// Gets a boolean indicating whether a Property is marked as storing sensitive values on the Members profile.
        /// </summary>
        /// <param name="propertyTypeAlias">PropertyType Alias of the Property to check</param>
        /// <returns></returns>
        public bool IsSensitiveProperty(string propertyTypeAlias)
        {
            MemberTypePropertyProfileAccess propertyProfile;
            if (MemberTypePropertyTypes.TryGetValue(propertyTypeAlias, out propertyProfile))
            {
                return propertyProfile.IsSensitive;
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
            MemberTypePropertyProfileAccess propertyProfile;
            if (MemberTypePropertyTypes.TryGetValue(propertyTypeAlias, out propertyProfile))
            {
                propertyProfile.IsEditable = value;
            }
            else
            {
                var tuple = new MemberTypePropertyProfileAccess(false, value, false);
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
            MemberTypePropertyProfileAccess propertyProfile;
            if (MemberTypePropertyTypes.TryGetValue(propertyTypeAlias, out propertyProfile))
            {
                propertyProfile.IsVisible = value;
            }
            else
            {
                var tuple = new MemberTypePropertyProfileAccess(value, false, false);
                MemberTypePropertyTypes.Add(propertyTypeAlias, tuple);
            }
        }

        /// <summary>
        /// Sets a boolean indicating whether a Property is a sensitive value on the Members profile.
        /// </summary>
        /// <param name="propertyTypeAlias">PropertyType Alias of the Property to set</param>
        /// <param name="value">Boolean value, true or false</param>
        public void SetIsSensitiveProperty(string propertyTypeAlias, bool value)
        {
            MemberTypePropertyProfileAccess propertyProfile;
            if (MemberTypePropertyTypes.TryGetValue(propertyTypeAlias, out propertyProfile))
            {
                propertyProfile.IsSensitive = value;
            }
            else
            {
                var tuple = new MemberTypePropertyProfileAccess(false, false, true);
                MemberTypePropertyTypes.Add(propertyTypeAlias, tuple);
            }
        }
    }
}
