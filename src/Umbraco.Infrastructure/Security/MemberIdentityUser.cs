using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security
{
    /// <summary>
    /// The identity user used for the member
    /// </summary>
    public class MemberIdentityUser : UmbracoIdentityUser
    {
        private string _name;
        private string _comments;
        private string _passwordConfig;
        private IReadOnlyCollection<IReadOnlyUserGroup> _groups;

        // Custom comparer for enumerables
        private static readonly DelegateEqualityComparer<IReadOnlyCollection<IReadOnlyUserGroup>> s_groupsComparer = new DelegateEqualityComparer<IReadOnlyCollection<IReadOnlyUserGroup>>(
            (groups, enumerable) => groups.Select(x => x.Alias).UnsortedSequenceEqual(enumerable.Select(x => x.Alias)),
            groups => groups.GetHashCode());

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberIdentityUser"/> class.
        /// </summary>
        public MemberIdentityUser(int userId)
        {
            // use the property setters - they do more than just setting a field
            Id = UserIdToString(userId);
        }

        public MemberIdentityUser()
        {
        }

        /// <summary>
        ///  Used to construct a new instance without an identity
        /// </summary>
        public static MemberIdentityUser CreateNew(string username, string email, string memberTypeAlias, string name = null)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));
            }

            var user = new MemberIdentityUser();
            user.DisableChangeTracking();
            user.UserName = username;
            user.Email = email;
            user.MemberTypeAlias = memberTypeAlias;
            user.Id = null;
            user.HasIdentity = false;
            user._name = name;
            user.EnableChangeTracking();
            return user;
        }

        /// <summary>
        /// Gets or sets the member's real name
        /// </summary>
        public string Name
        {
            get => _name;
            set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
        }

        /// <summary>
        /// Gets or sets the member's comments
        /// </summary>
        public string Comments
        {
            get => _comments;
            set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _comments, nameof(Comments));
        }

        // No change tracking because the persisted value is only set with the IsLockedOut flag
        public DateTime? LastLockoutDateUtc { get; set; }

        // No change tracking because the persisted value is readonly
        public DateTime CreatedDateUtc { get; set; }

        // No change tracking because the persisted value is readonly
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the password config
        /// </summary>
        public string PasswordConfig
        {
            get => _passwordConfig;
            set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _passwordConfig, nameof(PasswordConfig));
        }

        /// <summary>
        /// Gets or sets the user groups
        /// </summary>
        public IReadOnlyCollection<IReadOnlyUserGroup> Groups
        {
            get => _groups;
            set
            {
                _groups = value.Where(x => x.Alias != null).ToArray();

                var roles = new List<IdentityUserRole<string>>();
                foreach (IdentityUserRole<string> identityUserRole in _groups.Select(x => new IdentityUserRole<string>
                {
                    RoleId = x.Alias,
                    UserId = Id
                }))
                {
                    roles.Add(identityUserRole);
                }

                // now reset the collection
                Roles = roles;

                BeingDirty.SetPropertyValueAndDetectChanges(value, ref _groups, nameof(Groups), s_groupsComparer);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the member is locked out
        /// </summary>
        public bool IsLockedOut
        {
            get
            {
                bool isLocked = LockoutEnd.HasValue && LockoutEnd.Value.ToLocalTime() >= DateTime.Now;
                return isLocked;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the member is approved
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets the alias of the member type
        /// </summary>
        public string MemberTypeAlias { get; set; }

        private static string UserIdToString(int userId) => string.Intern(userId.ToString());

        // TODO: Should we support custom member properties for persistence/retrieval?
    }
}
