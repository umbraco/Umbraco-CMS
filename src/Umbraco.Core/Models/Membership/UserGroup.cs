using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a Group for a Backoffice User
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class UserGroup : EntityBase, IUserGroup, IReadOnlyUserGroup
    {
        private int? _startContentId;
        private int? _startMediaId;
        private string _alias;
        private string _icon;
        private string _name;
        private IEnumerable<string> _permissions;
        private List<string> _sectionCollection;

        //Custom comparer for enumerable
        private static readonly DelegateEqualityComparer<IEnumerable<string>> StringEnumerableComparer =
            new DelegateEqualityComparer<IEnumerable<string>>(
                (enum1, enum2) => enum1.UnsortedSequenceEqual(enum2),
                enum1 => enum1.GetHashCode());

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
            get => _startMediaId;
            set => SetPropertyValueAndDetectChanges(value, ref _startMediaId, nameof(StartMediaId));
        }

        [DataMember]
        public int? StartContentId
        {
            get => _startContentId;
            set => SetPropertyValueAndDetectChanges(value, ref _startContentId, nameof(StartContentId));
        }

        [DataMember]
        public string Icon
        {
            get => _icon;
            set => SetPropertyValueAndDetectChanges(value, ref _icon, nameof(Icon));
        }

        [DataMember]
        public string Alias
        {
            get => _alias;
            set => SetPropertyValueAndDetectChanges(value.ToCleanString(CleanStringType.Alias | CleanStringType.UmbracoCase), ref _alias, nameof(Alias));
        }

        [DataMember]
        public string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
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
            get => _permissions;
            set => SetPropertyValueAndDetectChanges(value, ref _permissions, nameof(Permissions), StringEnumerableComparer);
        }

        public IEnumerable<string> AllowedSections
        {
            get => _sectionCollection;
        }

        public void RemoveAllowedSection(string sectionAlias)
        {
            if (_sectionCollection.Contains(sectionAlias))
                _sectionCollection.Remove(sectionAlias);
        }

        public void AddAllowedSection(string sectionAlias)
        {
            if (_sectionCollection.Contains(sectionAlias) == false)
                _sectionCollection.Add(sectionAlias);
        }

        public void ClearAllowedSections()
        {
            _sectionCollection.Clear();
        }

        public int UserCount { get; }

        protected override void PerformDeepClone(object clone)
        {

            base.PerformDeepClone(clone);

            var clonedEntity = (UserGroup)clone;

            //manually clone the start node props
            clonedEntity._sectionCollection = new List<string>(_sectionCollection);
        }
    }
}
