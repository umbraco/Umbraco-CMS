using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a Group for a Backoffice User
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class UserGroup : Entity, IUserGroup, IReadOnlyUserGroup
    {
        private int? _startContentId;
        private int? _startMediaId;
        private string _alias;
        private string _icon;
        private string _name;
        private IEnumerable<string> _permissions;
        private readonly List<string> _sectionCollection;

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<UserGroup, string>(x => x.Name);
            public readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<UserGroup, string>(x => x.Alias);
            public readonly PropertyInfo PermissionsSelector = ExpressionHelper.GetPropertyInfo<UserGroup, IEnumerable<string>>(x => x.Permissions);
            public readonly PropertyInfo IconSelector = ExpressionHelper.GetPropertyInfo<UserGroup, string>(x => x.Icon);
            public readonly PropertyInfo StartContentIdSelector = ExpressionHelper.GetPropertyInfo<UserGroup, int?>(x => x.StartContentId);
            public readonly PropertyInfo StartMediaIdSelector = ExpressionHelper.GetPropertyInfo<UserGroup, int?>(x => x.StartMediaId);

            //Custom comparer for enumerable
            public readonly DelegateEqualityComparer<IEnumerable<string>> StringEnumerableComparer =
                new DelegateEqualityComparer<IEnumerable<string>>(
                    (enum1, enum2) => enum1.UnsortedSequenceEqual(enum2),
                    enum1 => enum1.GetHashCode());
        }

        /// <summary>
        /// Constructor to create a new user group
        /// </summary>
        public UserGroup()
        {
            _sectionCollection = new List<string>();
        }

        /// <summary>
        /// Constructor to create an existing user group
        /// </summary>
        /// <param name="userCount"></param>
        /// <param name="alias"></param>
        /// <param name="name"></param>
        /// <param name="permissions"></param>
        /// <param name="icon"></param>
        public UserGroup(int userCount, string alias, string name, IEnumerable<string> permissions, string icon)
            : this()
        {
            UserCount = userCount;
            _alias = alias;
            _name = name;
            _permissions = permissions;            
            _icon = icon;
        }

        [DataMember]
        public int? StartMediaId
        {
            get { return _startMediaId; }
            set { SetPropertyValueAndDetectChanges(value, ref _startMediaId, Ps.Value.StartMediaIdSelector); }
        }

        [DataMember]
        public int? StartContentId
        {
            get { return _startContentId; }
            set { SetPropertyValueAndDetectChanges(value, ref _startContentId, Ps.Value.StartContentIdSelector); }
        }

        [DataMember]
        public string Icon
        {
            get { return _icon; }
            set { SetPropertyValueAndDetectChanges(value, ref _icon, Ps.Value.IconSelector); }
        }

        [DataMember]
        public string Alias
        {
            get { return _alias; }
            set
            {
                SetPropertyValueAndDetectChanges(
                    value.ToCleanString(CleanStringType.Alias | CleanStringType.UmbracoCase),
                    ref _alias,
                    Ps.Value.AliasSelector);
            }
        }

        [DataMember]
        public string Name
        {
            get { return _name; }
            set { SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector); }
        }

        /// <summary>
        /// The set of default permissions for the user group
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
                SetPropertyValueAndDetectChanges(value, ref _permissions, Ps.Value.PermissionsSelector,
                    Ps.Value.StringEnumerableComparer);
            }
        }

        public IEnumerable<string> AllowedSections
        {
            get { return _sectionCollection; }
        }

        public void RemoveAllowedSection(string sectionAlias)
        {
            if (_sectionCollection.Contains(sectionAlias))
            {
                _sectionCollection.Remove(sectionAlias);
            }
        }

        public void AddAllowedSection(string sectionAlias)
        {
            if (_sectionCollection.Contains(sectionAlias) == false)
            {
                _sectionCollection.Add(sectionAlias);
            }
        }

        public void ClearAllowedSections()
        {
            _sectionCollection.Clear();
        }

        public int UserCount { get; private set; }
    }
}