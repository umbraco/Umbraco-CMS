using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security
{
    /// <summary>
    /// The identity user used for the back office
    /// </summary>
    public class BackOfficeIdentityUser : UmbracoIdentityUser
    {
        private string _culture;
        private IReadOnlyCollection<IReadOnlyUserGroup> _groups;
        private string[] _allowedSections;
        private int[] _startMediaIds;
        private int[] _startContentIds;

        private static readonly DelegateEqualityComparer<int[]> s_startIdsComparer = new DelegateEqualityComparer<int[]>(
            (groups, enumerable) => groups.UnsortedSequenceEqual(enumerable),
            groups => groups.GetHashCode());

        /// <summary>
        ///  Used to construct a new instance without an identity
        /// </summary>
        /// <param name="email">This is allowed to be null (but would need to be filled in if trying to persist this instance)</param>
        public static BackOfficeIdentityUser CreateNew(GlobalSettings globalSettings, string username, string email, string culture, string name = null)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(culture))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(culture));
            }

            var user = new BackOfficeIdentityUser(globalSettings, Array.Empty<IReadOnlyUserGroup>());
            user.DisableChangeTracking();
            user.UserName = username;
            user.Email = email;

            user.Id = null;
            user.HasIdentity = false;
            user._culture = culture;
            user.Name = name;
            user.EnableChangeTracking();
            return user;
        }

        private BackOfficeIdentityUser(GlobalSettings globalSettings, IReadOnlyCollection<IReadOnlyUserGroup> groups)
        {
            _startMediaIds = Array.Empty<int>();
            _startContentIds = Array.Empty<int>();
            _allowedSections = Array.Empty<string>();
            _culture = globalSettings.DefaultUILanguage;

            SetGroups(groups);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackOfficeIdentityUser"/> class.
        /// </summary>
        public BackOfficeIdentityUser(GlobalSettings globalSettings, int userId, IEnumerable<IReadOnlyUserGroup> groups)
            : this(globalSettings, groups.ToArray())
        {
            // use the property setters - they do more than just setting a field
            Id = UserIdToString(userId);
        }

        public int[] CalculatedMediaStartNodeIds { get; set; }
        public int[] CalculatedContentStartNodeIds { get; set; }

        /// <summary>
        /// Gets or sets content start nodes assigned to the User (not ones assigned to the user's groups)
        /// </summary>
        public int[] StartContentIds
        {
            get => _startContentIds;
            set
            {
                if (value == null)
                {
                    value = new int[0];
                }

                BeingDirty.SetPropertyValueAndDetectChanges(value, ref _startContentIds, nameof(StartContentIds), s_startIdsComparer);
            }
        }

        /// <summary>
        /// Gets or sets media start nodes assigned to the User (not ones assigned to the user's groups)
        /// </summary>
        public int[] StartMediaIds
        {
            get => _startMediaIds;
            set
            {
                if (value == null)
                {
                    value = new int[0];
                }

                BeingDirty.SetPropertyValueAndDetectChanges(value, ref _startMediaIds, nameof(StartMediaIds), s_startIdsComparer);
            }
        }

        /// <summary>
        /// Gets a readonly list of the user's allowed sections which are based on it's user groups
        /// </summary>
        public string[] AllowedSections => _allowedSections ??= _groups.SelectMany(x => x.AllowedSections).Distinct().ToArray();

        /// <summary>
        /// Gets or sets the culture
        /// </summary>
        public string Culture
        {
            get => _culture;
            set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _culture, nameof(Culture));
        }

        /// <summary>
        /// Gets or sets the user groups
        /// </summary>
        public void SetGroups(IReadOnlyCollection<IReadOnlyUserGroup> value)
        {
            // so they recalculate
            _allowedSections = null;

            _groups = value.Where(x => x.Alias != null).ToArray();

            var roles = new List<IdentityUserRole<string>>();
            foreach (IdentityUserRole<string> identityUserRole in _groups.Select(x => new IdentityUserRole<string>
            {
                RoleId = x.Alias,
                UserId = Id?.ToString()
            }))
            {
                roles.Add(identityUserRole);
            }

            // now reset the collection
            Roles = roles;
        }

        private static string UserIdToString(int userId) => string.Intern(userId.ToString());
    }
}
