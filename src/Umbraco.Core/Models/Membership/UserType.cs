using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents the Type for a Backoffice User
    /// </summary>    
    [Serializable]
    [DataContract(IsReference = true)]
    internal class UserType : Entity, IUserType
    {
        private string _alias;
        private string _name;
        private IEnumerable<string> _permissions;

        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<UserType, string>(x => x.Name);
        private static readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<UserType, string>(x => x.Alias);
        private static readonly PropertyInfo PermissionsSelector = ExpressionHelper.GetPropertyInfo<UserType, IEnumerable<string>>(x => x.Permissions);

        [DataMember]
        public string Alias
        {
            get { return _alias; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _alias = value.ToCleanString(CleanStringType.Alias | CleanStringType.UmbracoCase);
                    return _alias;
                }, _alias, AliasSelector);
            }
        }

        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _name = value;
                    return _name;
                }, _name, NameSelector);
            }
        }

        /// <summary>
        /// The set of default permissions for the user type
        /// </summary>
        /// <remarks>
        /// By default each permission is simply a single char but we've made this an enumerable{string} to support a more flexible permissions structure in the future.
        /// </remarks>
        [DataMember]
        public IEnumerable<string> Permissions
        {
            get { return _permissions; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _permissions = value;
                    return _permissions;
                }, _permissions, PermissionsSelector,
                    //Custom comparer for enumerable
                    new DelegateEqualityComparer<IEnumerable<string>>(
                        (enum1, enum2) => enum1.UnsortedSequenceEqual(enum2),
                        enum1 => enum1.GetHashCode()));
            }
        }
    }
}